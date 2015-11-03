using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Twitter.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class TexbblockNavigationServiceBehavior
    {
        public static string GetText(DependencyObject obj) { return obj.GetValue(TextProperty) as string; }
        public static void SetText(DependencyObject obj, string value) { obj.SetValue(TextProperty, value); }

        public static object GetEntities(DependencyObject obj) { return obj.GetValue(EntitiesProperty) as object; }
        public static void SetEntities(DependencyObject obj, object value) { obj.SetValue(EntitiesProperty, value); }

        public static string GetLink(DependencyObject obj) { return obj.GetValue(LinkProperty) as string; }
        public static void SetLink(DependencyObject obj, string value) { obj.SetValue(LinkProperty, value); }

        public static bool GetDeficientEntity(DependencyObject obj) { return (bool)obj.GetValue(DeficientEntityProperty); }
        public static void SetDeficientEntity(DependencyObject obj, bool value) { obj.SetValue(DeficientEntityProperty, value); }

        public static readonly DependencyProperty EntitiesProperty =
            DependencyProperty.RegisterAttached("Entities", typeof(object), typeof(TexbblockNavigationServiceBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text", typeof(string), typeof(TexbblockNavigationServiceBehavior), new PropertyMetadata(null, OnPropertyChanged));

        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.RegisterAttached("Link", typeof(string), typeof(TexbblockNavigationServiceBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty DeficientEntityProperty =
            DependencyProperty.RegisterAttached("DeficientEntity", typeof(bool), typeof(TexbblockNavigationServiceBehavior), new PropertyMetadata(false));

        public static SolidColorBrush GetTextForeground(DependencyObject obj) { return obj.GetValue(TextForegroundProperty) as SolidColorBrush; }
        public static void SetTextForeground(DependencyObject obj, SolidColorBrush value) { obj.SetValue(TextForegroundProperty, value); }
        public static readonly DependencyProperty TextForegroundProperty =
            DependencyProperty.RegisterAttached("TextForeground", typeof(object), typeof(TexbblockNavigationServiceBehavior), new PropertyMetadata(null));

        public static SolidColorBrush GetLinkForeground(DependencyObject obj) { return obj.GetValue(LinkForegroundProperty) as SolidColorBrush; }
        public static void SetLinkForeground(DependencyObject obj, SolidColorBrush value) { obj.SetValue(LinkForegroundProperty, value); }
        public static readonly DependencyProperty LinkForegroundProperty =
            DependencyProperty.RegisterAttached("LinkForeground", typeof(object), typeof(TexbblockNavigationServiceBehavior), new PropertyMetadata(null));

        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = obj as TextBlock;

            if (textBlock == null)
                return;

            textBlock.Inlines.Clear();

            var text = GetText(obj);
            var entities = GetEntities(obj) as Entities;            

            if (string.IsNullOrEmpty(text))
                return;

            foreach (var inline in GenerateInlines(obj, text, entities))
                textBlock.Inlines.Add(inline);

            textBlock.Inlines.Add(new Run() { Text = " " });
        }

        private static IEnumerable<Inline> GenerateInlines(DependencyObject obj, string text, Entities entities = null)
        {
            foreach (var token in Tokenize(text))
            {
                switch (token.Type)
                {
                    case TextToken.TextTokenId.Text:
                        yield return GenerateText(obj, token.Value as string);
                        break;
                    case TextToken.TextTokenId.HashTag:
                        if (entities != null && !GetDeficientEntity(obj))
                        {
                            var userMentions = entities.HashTags.Where(x => "#" + x.Tag == (string)token.Value);
                            if (userMentions.Count() == 0)
                                yield return GenerateText(obj, token.Value as string);
                            else
                                yield return GenerateLink(obj, (string)token.Value, "hashtag://" + (string)token.Value);
                        }
                        else
                        {
                            yield return GenerateLink(obj, (string)token.Value, "hashtag://" + (string)token.Value);
                        }
                        break;
                    case TextToken.TextTokenId.UserMention:
                        if (entities != null && !GetDeficientEntity(obj))
                        {
                            var userMentions = entities.UserMentions.Where(x => "@" + x.ScreenName == (string)token.Value);
                            if (userMentions.Count() == 0)
                                yield return GenerateText(obj, token.Value as string);
                            else
                                yield return GenerateLink(obj, (string)token.Value, "usermention://" + (string)token.Value);
                        }
                        else
                        {
                            yield return GenerateLink(obj, (string)token.Value, "usermention://" + (string)token.Value);
                        }
                        break;
                    case TextToken.TextTokenId.Url:
                        if (entities != null)
                        {
                            var urls = entities.Urls.Where(x => x.Url == (string)token.Value);
                            if (urls.Count() == 0)
                                yield return GenerateText(obj, token.Value as string);
                            else
                                yield return GenerateLink(obj, urls.First().DisplayUrl, !string.IsNullOrWhiteSpace(urls.First().ExpandedUrl) ? urls.First().ExpandedUrl : urls.First().Url);
                        }
                        else
                        {
                            yield return GenerateLink(obj, (string)token.Value, (string)token.Value);
                        }
                        break;
                }
            }
        }

        private static Inline GenerateText(DependencyObject obj, string text)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            var run = new Run() { Text = text };

            // ベタ書きなのは軽量化とバグを減らすため
            // 仮想化したListViewだとItemを使い回すときにPropertyがたまにnullになってしまうことがある。なんで？
            var foreground = GetTextForeground(obj);
            if (foreground == null)
                run.Foreground = (Brush)Application.Current.Resources["TweetTextTextblockForegroundBrush"];
            else
                run.Foreground = foreground;

            return run;
        }

        private static Inline GenerateLink(DependencyObject obj, string text, string linkUrl)
        {
            if (string.IsNullOrEmpty(text))
                text = "";
            if (string.IsNullOrEmpty(linkUrl))
                linkUrl = "";

            var hyperLink = new Hyperlink() { };
            hyperLink.Inlines.Add(new Run() { Text = text });
            hyperLink.Click += HyperLink_Click;
            hyperLink.SetValue(LinkProperty, linkUrl);

            var foreground = GetLinkForeground(obj);
            if (foreground == null)
                hyperLink.Foreground = (Brush)Application.Current.Resources["TweetTextHyperlinkTextblockForegroundBrush"];
            else
                hyperLink.Foreground = foreground;

            return hyperLink;
        }

        private static IEnumerable<TextToken> Tokenize(string text)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            var escapedText = text.ResolveEntity().EscapeEntity();

            escapedText = TweetRegexPatterns.ValidUrl.Replace(escapedText, m =>
                m.Groups[TweetRegexPatterns.ValidUrlGroupBefore] + "<U>" +
                    // # => &sharp; (ハッシュタグで再識別されることを防ぐ)
                m.Groups[TweetRegexPatterns.ValidUrlGroupUrl].Value.Replace("#", "&sharp;") +
                "<");

            escapedText = TweetRegexPatterns.ValidMentionOrList.Replace(escapedText, m =>
                m.Groups[TweetRegexPatterns.ValidMentionOrListGroupBefore].Value +
                "<A>" +
                m.Groups[TweetRegexPatterns.ValidMentionOrListGroupAt].Value +
                m.Groups[TweetRegexPatterns.ValidMentionOrListGroupUsername].Value +
                m.Groups[TweetRegexPatterns.ValidMentionOrListGroupList].Value +
                "<");

            escapedText = TweetRegexPatterns.ValidHashtag.Replace(escapedText, m =>
                m.Groups[TweetRegexPatterns.ValidHashtagGroupBefore].Value +
                "<H>" +
                m.Groups[TweetRegexPatterns.ValidHashtagGroupHash].Value +
                m.Groups[TweetRegexPatterns.ValidHashtagGroupTag].Value +
                "<");

            var splitted = escapedText.Split(new[] { '<' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in splitted)
            {
                if (s.Contains('>'))
                {
                    var kind = s[0];
                    var body = s.Substring(2).ResolveEntity();
                    switch (kind)
                    {
                        case 'U':
                            // &sharp; => #
                            yield return new TextToken(TextToken.TextTokenId.Url, body.Replace("&sharp;", "#"));
                            break;
                        case 'A':
                            yield return new TextToken(TextToken.TextTokenId.UserMention, body);
                            break;
                        case 'H':
                            yield return new TextToken(TextToken.TextTokenId.HashTag, body);
                            break;
                        default:
                            throw new InvalidOperationException("invalid grouping:" + kind);
                    }
                }
                else
                {
                    yield return new TextToken(TextToken.TextTokenId.Text, s.ResolveEntity());
                }
            }

            /*if (entities.UrlEntities != null)
            {
                foreach (var url in entities.UrlEntities)
                    entitiesList.Add(new EntityInfo() { Entities = url, Start = url.Start, End = url.End, Type = TextToken.TextTokenId.Url });
            }
            if (entities.HashTagEntities != null)
            {
                foreach (var hashTag in entities.HashTagEntities)
                    entitiesList.Add(new EntityInfo() { Entities = hashTag, Start = hashTag.Start, End = hashTag.End, Type = TextToken.TextTokenId.HashTag });
            }
            if (entities.UserMentionsEntities != null)
            {
                foreach (var userMention in entities.UserMentionsEntities)
                    entitiesList.Add(new EntityInfo() { Entities = userMention, Start = userMention.Start, End = userMention.End, Type = TextToken.TextTokenId.UserMention });
            }

            entitiesList = new List<EntityInfo>(entitiesList.OrderBy((entity) => entity.Start));

            var lastPosition = 0;
            foreach (var entity in entitiesList)
            {
                if (entity.Start != lastPosition)
                    yield return new TextToken(TextToken.TextTokenId.Text, text.SubstringByTextElements(lastPosition, entity.Start - lastPosition));
                

                yield return new TextToken(entity.Type, entity.Entities);
                lastPosition = entity.End;
            }

            if (lastPosition < text.LengthInTextElements())
                yield return new TextToken(TextToken.TextTokenId.Text, text.SubstringByTextElements(lastPosition, text.LengthInTextElements() - lastPosition));*/
        }

        private static async void HyperLink_Click(object sender, HyperlinkClickEventArgs e)
        {
            var hyperLink = sender as Hyperlink;
            if (hyperLink == null)
                return;

            var linkUrl = hyperLink.GetValue(LinkProperty) as string;

            if (string.IsNullOrEmpty(linkUrl))
                return;

            if (linkUrl.StartsWith("hashtag://"))
            {
                var hashTag = linkUrl.Replace("hashtag://", "");
                return;
            }
            else if (linkUrl.StartsWith("usermention://"))
            {
                var userMention = linkUrl.Replace("usermention://", "");
                ViewModels.Services.Notice.Instance.ShowUserProfileCommand.Execute(userMention.Replace("@", ""));
                return;
            }
            
            var statusMatch = TweetRegexPatterns.StatusUrl.Match(linkUrl);
            var userMatch = TweetRegexPatterns.UserUrl.Match(linkUrl);
			if (statusMatch.Success)
			{

            }
			else if (userMatch.Success)
			{
                ViewModels.Services.Notice.Instance.ShowUserProfileCommand.Execute(userMatch.Groups["ScreenName"].ToString());
            }
			else
			{
				await Launcher.LaunchUriAsync(new Uri(linkUrl));
			}
        }
    }

    public class EntityInfo
    {
        public int Start { get; set; }
        public int End { get; set; }
        public object Entities { get; set; }
        public TextToken.TextTokenId Type { get; set; }
    }

    public struct TextToken
    {
        public enum TextTokenId
        {
            Text,
            HashTag,
            UserMention,
            Url
        }

        public TextTokenId Type;
        public object Value;

        public TextToken(TextTokenId Type, object Value)
            : this()
        {
            this.Type = Type;
            this.Value = Value;
        }
    }
}
