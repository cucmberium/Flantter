using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Twitter;
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
            foreach (var token in Tokenize(obj, text, entities))
            {
                switch (token.Type)
                {
                    case TextPartType.Plain:
                        yield return GenerateText(obj, token.RawText.ResolveEntity());
                        break;
                    case TextPartType.Cashtag:
                        yield return GenerateText(obj, token.RawText.ResolveEntity());
                        break;
                    case TextPartType.Hashtag:
                        yield return GenerateLink(obj, token.Text, "hashtag://" + token.RawText);
                        break;
                    case TextPartType.UserMention:
                        yield return GenerateLink(obj, token.Text, "usermention://" + token.RawText);
                        break;
                    case TextPartType.Url:
                        yield return GenerateLink(obj, token.Text, token.RawText);
                        break;
                }
            }
        }

        private static Inline GenerateText(DependencyObject obj, string text)
        {
            if (text == null)
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
            if (text == null)
                text = "";
            if (linkUrl == null)
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

        private static IEnumerable<TextPart> TokenizeImpl(string text)
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
                            // "&sharp;" => "#"  ,  "https://" => ""
                            yield return new TextPart() { RawText = body.Replace("&sharp;", "#"), Text = body.Replace("&sharp;", "#").Replace("https://", ""), Type = TextPartType.Url };
                            break;
                        case 'A':
                            yield return new TextPart() { RawText = body, Text = body, Type = TextPartType.UserMention };
                            break;
                        case 'H':
                            yield return new TextPart() { RawText = body, Text = body, Type = TextPartType.Hashtag };
                            break;
                        default:
                            throw new InvalidOperationException("invalid grouping:" + kind);
                    }
                }
                else
                {
                    yield return new TextPart() { RawText = s.ResolveEntity(), Type = TextPartType.Plain };
                }
            }
        }

        private static IEnumerable<TextPart> Tokenize(DependencyObject sender, string text, Entities entities)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            if (entities == null || GetDeficientEntity(sender))
            {
                foreach (var token in TokenizeImpl(text))
                    yield return token;
            }
            else
            {
                foreach (var token in ExtractTextParts.EnumerateTextParts(text, entities))
                    yield return token;
            }
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
                ViewModels.Services.Notice.Instance.ShowSearchCommand.Execute(hashTag);
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
                ViewModels.Services.Notice.Instance.ShowStatusDetailCommand.Execute(long.Parse(statusMatch.Groups["Id"].ToString()));
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
}
