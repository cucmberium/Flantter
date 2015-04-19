using Flantter.MilkyWay.Common;
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
    public static class TexbblockNavigationServiceBehavior
    {
        public static string GetText(DependencyObject obj) { return obj.GetValue(TextProperty) as string; }
        public static void SetText(DependencyObject obj, string value) { obj.SetValue(TextProperty, value); }

        public static object GetEntities(DependencyObject obj) { return obj.GetValue(EntitiesProperty) as object; }
        public static void SetEntities(DependencyObject obj, object value) { obj.SetValue(EntitiesProperty, value); }

        public static string GetLink(DependencyObject obj) { return obj.GetValue(LinkProperty) as string; }
        public static void SetLink(DependencyObject obj, string value) { obj.SetValue(LinkProperty, value); }

        public static readonly DependencyProperty EntitiesProperty =
            DependencyProperty.RegisterAttached("Entities", typeof(object), typeof(TexbblockNavigationServiceBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text", typeof(string), typeof(TexbblockNavigationServiceBehavior), new PropertyMetadata(null, OnPropertyChanged));

        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.RegisterAttached("Link", typeof(string), typeof(TexbblockNavigationServiceBehavior), new PropertyMetadata(null));

        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = obj as TextBlock;

            if (textBlock == null)
                return;

            textBlock.Inlines.Clear();

            var text = GetText(obj);
            var entities = GetEntities(obj) as object;            

            if (string.IsNullOrEmpty(text))
                return;

            /*if (entities == null)
            {
                textBlock.Inlines.Add(GenerateText(text));
                return;
            }

            if (entities.UserMentionsEntities == null && entities.HashTagEntities == null && entities.UrlEntities == null)
            {
                textBlock.Inlines.Add(GenerateText(text));
                return;
            }*/

            foreach (var inline in GenerateInlines(text))
                textBlock.Inlines.Add(inline);

            textBlock.Inlines.Add(new Run() { Text = " " });
        }

        private static IEnumerable<Inline> GenerateInlines(string text)
        {
            foreach (var token in Tokenize(text))
            {
                switch (token.Type)
                {
                    case TextToken.TextTokenId.Text:
                        yield return GenerateText(token.Value as string);
                        break;
                    case TextToken.TextTokenId.HashTag:
                        yield return GenerateLink((string)token.Value, "hashtag://" + (string)token.Value);
                        break;
                    case TextToken.TextTokenId.UserMention:
                        yield return GenerateLink((string)token.Value, "usermention://" + (string)token.Value);
                        break;
                    case TextToken.TextTokenId.Url:
                        /*if (entities == null)
                            continue;

                        if (entities.UrlEntities == null)
                            continue;

                        var urlEntities = entities.UrlEntities.Where(x => x.Url == (string)token.Value);
                        if (urlEntities.Count() == 0)
                            continue;

                        yield return GenerateLink(urlEntities.First().DisplayUrl, !string.IsNullOrWhiteSpace(urlEntities.First().ExpandedUrl) ? urlEntities.First().ExpandedUrl : urlEntities.First().Url);*/
                        yield return GenerateLink((string)token.Value, (string)token.Value);
                        break;
                }
            }
        }

        private static Inline GenerateText(string text)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            var run = new Run() { Text = text };

            /*var binding = new Binding
            {
                Source = Theme.ThemeService.Theme,
                Path = new PropertyPath("TweetFieldTweetListTweetTextTextblockForegroundBrush"),
                Mode = BindingMode.OneWay
            };
            BindingOperations.SetBinding(run, Run.ForegroundProperty, binding);*/

            run.Foreground = (Brush)Application.Current.Resources["TweetFieldTweetListTweetTextTextblockForegroundBrush"];

            return run;
        }

        private static Inline GenerateLink(string text, string linkUrl)
        {
            if (string.IsNullOrEmpty(text))
                text = "";
            if (string.IsNullOrEmpty(linkUrl))
                linkUrl = "";

            var hyperLink = new Hyperlink() { };
            hyperLink.Inlines.Add(new Run() { Text = text });
            hyperLink.Click += HyperLink_Click;
            hyperLink.SetValue(LinkProperty, linkUrl);

            hyperLink.Foreground = (Brush)Application.Current.Resources["TweetFieldTweetListTweetHyperlinkTextblockForegroundBrush"];

            return hyperLink;
        }

        private static IEnumerable<TextToken> Tokenize(string text)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            // var entitiesList = new List<EntityInfo>();

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
                return;
            }

            var match = Regex.Match(linkUrl, @"https?://twitter.com/(#!/)?([a-zA-Z0-9_])+/status(es)?/(?<Id>[0-9]+)$", RegexOptions.IgnoreCase);
            var match2 = Regex.Match(linkUrl, @"https?://twitter.com/(#!/)?(?<ScreenName>([a-zA-Z0-9_])+)$", RegexOptions.IgnoreCase);
			if (match.Success)
			{ }
			else if (match2.Success)
			{ }
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
