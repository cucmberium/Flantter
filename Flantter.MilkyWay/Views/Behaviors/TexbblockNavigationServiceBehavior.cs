using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Apis;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class TexbblockNavigationServiceBehavior
    {
        public static readonly DependencyProperty EntitiesProperty =
            DependencyProperty.RegisterAttached("Entities", typeof(object), typeof(TexbblockNavigationServiceBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text", typeof(string), typeof(TexbblockNavigationServiceBehavior),
                new PropertyMetadata(null, OnPropertyChanged));

        public static readonly DependencyProperty LinkProperty =
            DependencyProperty.RegisterAttached("Link", typeof(string), typeof(TexbblockNavigationServiceBehavior),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DeficientEntityProperty =
            DependencyProperty.RegisterAttached("DeficientEntity", typeof(bool),
                typeof(TexbblockNavigationServiceBehavior), new PropertyMetadata(false));

        public static readonly DependencyProperty TextForegroundProperty =
            DependencyProperty.RegisterAttached("TextForeground", typeof(object),
                typeof(TexbblockNavigationServiceBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty LinkForegroundProperty =
            DependencyProperty.RegisterAttached("LinkForeground", typeof(object),
                typeof(TexbblockNavigationServiceBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty EmojisProperty =
            DependencyProperty.RegisterAttached("Emojis", typeof(object), typeof(TexbblockNavigationServiceBehavior),
                new PropertyMetadata(null));

        public static string GetText(DependencyObject obj)
        {
            return obj.GetValue(TextProperty) as string;
        }

        public static void SetText(DependencyObject obj, string value)
        {
            obj.SetValue(TextProperty, value);
        }

        public static object GetEntities(DependencyObject obj)
        {
            return obj.GetValue(EntitiesProperty);
        }

        public static void SetEntities(DependencyObject obj, object value)
        {
            obj.SetValue(EntitiesProperty, value);
        }

        public static string GetLink(DependencyObject obj)
        {
            return obj.GetValue(LinkProperty) as string;
        }

        public static void SetLink(DependencyObject obj, string value)
        {
            obj.SetValue(LinkProperty, value);
        }

        public static bool GetDeficientEntity(DependencyObject obj)
        {
            return (bool) obj.GetValue(DeficientEntityProperty);
        }

        public static void SetDeficientEntity(DependencyObject obj, bool value)
        {
            obj.SetValue(DeficientEntityProperty, value);
        }

        public static SolidColorBrush GetTextForeground(DependencyObject obj)
        {
            return obj.GetValue(TextForegroundProperty) as SolidColorBrush;
        }

        public static void SetTextForeground(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(TextForegroundProperty, value);
        }

        public static SolidColorBrush GetLinkForeground(DependencyObject obj)
        {
            return obj.GetValue(LinkForegroundProperty) as SolidColorBrush;
        }

        public static void SetLinkForeground(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(LinkForegroundProperty, value);
        }

        public static object GetEmojis(DependencyObject obj)
        {
            return obj.GetValue(LinkForegroundProperty);
        }

        public static void SetEmojis(DependencyObject obj, object value)
        {
            obj.SetValue(LinkForegroundProperty, value);
        }


        private static void OnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = obj as TextBlock;
            if (textBlock == null)
                return;

            textBlock.Inlines.Clear();

            var text = GetText(obj);
            var entities = GetEntities(obj) as Entities;
            var emojis = GetEmojis(obj) as List<Emoji>;

            if (string.IsNullOrEmpty(text))
                return;

            foreach (var inline in GenerateInlines(obj, text, entities, emojis))
                textBlock.Inlines.Add(inline);

            textBlock.Inlines.Add(new Run {Text = " "});
        }

        private static IEnumerable<Inline> GenerateInlines(DependencyObject obj, string text, Entities entities = null, List<Emoji> emojis = null)
        {
            foreach (var token in Tokenize(obj, text, entities, emojis))
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
                    case TextPartType.Emoji:
                        yield return GenerateEmoji(obj, token.Text);
                        break;
                }
        }

        private static Inline GenerateText(DependencyObject obj, string text)
        {
            if (text == null)
                text = "";

            var run = new Run {Text = text};

            // ベタ書きなのは軽量化とバグを減らすため
            // 仮想化したListViewだとItemを使い回すときにPropertyがたまにnullになってしまうことがある。なんで？
            var foreground = GetTextForeground(obj);
            if (foreground == null)
                run.Foreground = (Brush) Application.Current.Resources["TweetTextTextblockForegroundBrush"];
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

            var hyperLink = new Hyperlink();
            hyperLink.Inlines.Add(new Run {Text = text});
            hyperLink.Click += HyperLink_Click;
            hyperLink.SetValue(LinkProperty, linkUrl);

            var foreground = GetLinkForeground(obj);
            if (foreground == null)
                hyperLink.Foreground =
                    (Brush) Application.Current.Resources["TweetTextHyperlinkTextblockForegroundBrush"];
            else
                hyperLink.Foreground = foreground;

            return hyperLink;
        }

        private static Inline GenerateEmoji(DependencyObject obj, string url)
        {
            var inlineUiContainer = new InlineUIContainer();
            var image = new Image
            {
                Source = new BitmapImage(new Uri(url)),
                Width = SettingService.Setting.FontSize + 2,
                Height = SettingService.Setting.FontSize + 2,
                Margin = new Thickness(0, -2, 0, -2)
            };
            inlineUiContainer.Child = image;
            
            return inlineUiContainer;
        }
        
        private static IEnumerable<TextPart> TokenizeImpl(string text, List<Emoji> emojis)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            var escapedText = text.ResolveEntity().EscapeEntity();

            escapedText = TweetRegexPatterns.VaridUrlEx.Replace(escapedText, m =>
                m.Groups[TweetRegexPatterns.ValidUrlGroupBefore] + "<U>" +
                // # => &sharp; @ => &at;(ハッシュタグ, メンションで再識別されることを防ぐ)
                m.Groups[TweetRegexPatterns.ValidUrlGroupUrl].Value.Replace("#", "&sharp;").Replace("@", "&at;") +
                "<");

            escapedText = TweetRegexPatterns.ValidMastodonMentionOrList.Replace(escapedText, m =>
                m.Groups[TweetRegexPatterns.ValidMastodonMentionOrListGroupBefore].Value +
                "<A>" +
                m.Groups[TweetRegexPatterns.ValidMastodonMentionOrListGroupAt].Value +
                m.Groups[TweetRegexPatterns.ValidMastodonMentionOrListGroupUsername].Value +
                m.Groups[TweetRegexPatterns.ValidMastodonMentionOrListGroupList].Value +
                m.Groups[TweetRegexPatterns.ValidMastodonMentionOrListGroupMastodonDomain].Value +
                "<");

            escapedText = TweetRegexPatterns.ValidHashtag.Replace(escapedText, m =>
                m.Groups[TweetRegexPatterns.ValidHashtagGroupBefore].Value +
                "<H>" +
                m.Groups[TweetRegexPatterns.ValidHashtagGroupHash].Value +
                m.Groups[TweetRegexPatterns.ValidHashtagGroupTag].Value +
                "<");

            // if (emojis != null && emojis.Count >= 1)
            // {
            //     foreach (var emoji in emojis)
            //         escapedText = escapedText.Replace(":" + emoji.Shortcode + ":", "<E>" + emoji.StaticUrl + "<");
            // }

            var splitted = escapedText.Split(new[] {'<'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in splitted)
                if (s.Contains('>'))
                {
                    var kind = s[0];
                    var body = s.Substring(2).ResolveEntity();
                    switch (kind)
                    {
                        case 'U':
                            // "&sharp;" => "#" , "&at;" => "@" ,  "https://" => "" ,  "http://" => "", "www." => ""
                            var displayUrl = body.Replace("&sharp;", "#")
                                .Replace("&at;", "@")
                                .Replace("http://", "")
                                .Replace("https://", "")
                                .Replace("www.", "");
                            if (displayUrl.Length >= 31)
                                displayUrl = displayUrl.Substring(0, 30) + "...";
                            yield return new TextPart
                            {
                                RawText = body.Replace("&sharp;", "#").Replace("&at;", "@"),
                                Text = displayUrl,
                                Type = TextPartType.Url
                            };
                            break;
                        case 'A':
                            yield return new TextPart {RawText = body, Text = body, Type = TextPartType.UserMention};
                            break;
                        case 'H':
                            yield return new TextPart {RawText = body, Text = body, Type = TextPartType.Hashtag};
                            break;
                        case 'E':
                            yield return new TextPart {RawText = body, Text = body, Type = TextPartType.Emoji};
                            break;
                        default:
                            throw new InvalidOperationException("Invalid grouping:" + kind);
                    }
                }
                else
                {
                    yield return new TextPart {RawText = s.ResolveEntity(), Type = TextPartType.Plain};
                }
        }

        private static IEnumerable<TextPart> Tokenize(DependencyObject sender, string text, Entities entities, List<Emoji> emojis)
        {
            if (string.IsNullOrEmpty(text))
                yield break;

            if (entities == null || entities.HashTags == null || entities.UserMentions == null || entities.Urls == null)
                foreach (var token in TokenizeImpl(text, emojis))
                    yield return token;
            else if (GetDeficientEntity(sender) || entities.DeficientEntity)
                foreach (var token in TokenizeImpl(text, emojis))
                {
                    if (token.Type == TextPartType.Url && entities.Urls.Any(x => x.Url == token.RawText))
                    {
                        try
                        {
                            token.Text = entities.Urls.First(x => x.Url == token.RawText).DisplayUrl;
                        }
                        catch
                        {
                        }
                    }
                    else if (token.Type == TextPartType.UserMention)
                    {
                        try
                        {
                            token.RawText = entities.UserMentions.First(x => x.ScreenName == token.Text.Substring(1)).Id.ToString();
                        }
                        catch
                        {
                        }
                    }

                    yield return token;
                }
            else if (emojis != null)
                foreach (var token in TokenizeImpl(text, emojis))
                    yield return token;
            else
                foreach (var token in ExtractTextParts.EnumerateTextParts(text, entities))
                    yield return token;
        }

        private static async void HyperLink_Click(object sender, HyperlinkClickEventArgs e)
        {
            var hyperLink = sender as Hyperlink;

            var linkUrl = hyperLink?.GetValue(LinkProperty) as string;

            if (string.IsNullOrEmpty(linkUrl))
                return;

            if (linkUrl.StartsWith("hashtag://"))
            {
                var hashTag = linkUrl.Replace("hashtag://", "");
                Notice.Instance.ShowSearchCommand.Execute(hashTag);
                return;
            }
            if (linkUrl.StartsWith("usermention://"))
            {
                var userMention = linkUrl.Replace("usermention://", "");
                if (userMention.StartsWith("@"))
                    Notice.Instance.ShowUserProfileCommand.Execute(userMention.Substring(1));
                else
                    Notice.Instance.ShowUserProfileCommand.Execute(long.Parse(userMention));
                return;
            }

            await Launcher.LaunchUriAsync(new Uri(linkUrl));
        }
    }
}