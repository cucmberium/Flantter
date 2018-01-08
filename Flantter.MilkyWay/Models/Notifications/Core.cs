using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Notifications;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Setting;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Flantter.MilkyWay.Models.Notifications
{
    public enum PopupNotificationType
    {
        System,
        Retweet,
        Mention,
        DirectMessage,
        Favorite,
        Unfavorite,
        Follow,
        QuotedTweet,
        TweetCompleted
    }

    public enum TileNotificationType
    {
        Images,
        Mentions
    }

    public class Core
    {
        private ResourceLoader _resourceLoader;

        private Core()
        {
        }

        public static Core Instance { get; } = new Core();

        public void Initialize()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);

            _resourceLoader = new ResourceLoader();

            Observable.FromEvent<EventHandler<TweetEventArgs>, TweetEventArgs>(
                    h => (sender, e) => h(e),
                    h => Connecter.Instance.TweetReceiveCommandExecute += h,
                    h => Connecter.Instance.TweetReceiveCommandExecute -= h)
                .Where(e => e.Streaming)
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(e =>
                {
                    switch (e.Type)
                    {
                        case TweetEventArgs.TypeEnum.Status:
                            if (e.Parameter.Contains("favorites://"))
                                break;

                            if (e.Status.HasRetweetInformation && e.Status.User.Id == e.UserId &&
                                e.Status.RetweetInformation.User.Id != e.UserId && e.Parameter.Contains("home://"))
                                PopupToastNotification(PopupNotificationType.Retweet,
                                    string.Format(_resourceLoader.GetString("Notification_Retweet_Retweet"),
                                        "@" + e.Status.RetweetInformation.User.ScreenName), e.Status.Text,
                                    e.Status.RetweetInformation.User.ProfileImageUrl);

                            else if (e.Status.InReplyToUserId == e.UserId && e.Parameter.Contains("home://") &&
                                     !e.Status.HasRetweetInformation)
                                PopupToastNotification(PopupNotificationType.Mention,
                                    string.Format(_resourceLoader.GetString("Notification_Mention_Mention"),
                                        "@" + e.Status.User.ScreenName), e.Status.Text, e.Status.User.ProfileImageUrl,
                                    e.Status.Entities.Media.Count != 0
                                        ? e.Status.Entities.Media.First().MediaThumbnailUrl
                                        : "");

                            if (e.Status.Entities.Media.Any(x => x.Type == "Image") &&
                                !e.Status.User.IsProtected)
                                UpdateTileNotification(TileNotificationType.Images,
                                    e.Status.User.Name + "(@" + e.Status.User.ScreenName + ")" + "\n" + e.Status.Text,
                                    e.Status.Entities.Media.First(x => x.Type == "Image").MediaUrl);

                            else if (e.Status.InReplyToUserId == e.UserId)
                                UpdateTileNotification(TileNotificationType.Mentions,
                                    e.Status.User.Name + "(@" + e.Status.User.ScreenName + ")" + "\n" + e.Status.Text);

                            break;

                        case TweetEventArgs.TypeEnum.DirectMessage:
                            if (e.DirectMessage.Recipient.Id == e.UserId)
                                PopupToastNotification(PopupNotificationType.DirectMessage,
                                    string.Format(_resourceLoader.GetString("Notification_DirectMessage_DirectMessage"),
                                        "@" + e.DirectMessage.Sender.ScreenName), e.DirectMessage.Text,
                                    e.DirectMessage.Sender.ProfileImageUrl);

                            break;

                        case TweetEventArgs.TypeEnum.EventMessage:
                            if (e.EventMessage.Target == null)
                            {
                                if (e.EventMessage.Source.Id == e.UserId)
                                    break;
                            }
                            else
                            {
                                if (e.EventMessage.Target.Id != e.UserId || e.EventMessage.Source.Id == e.UserId)
                                    break;
                            }

                            switch (e.EventMessage.Type)
                            {
                                case "Favorite":
                                    PopupToastNotification(PopupNotificationType.Favorite,
                                        string.Format(_resourceLoader.GetString("Notification_Favorite_Favorite"),
                                            "@" + e.EventMessage.Source.ScreenName), e.EventMessage.TargetStatus.Text,
                                        e.EventMessage.Source.ProfileImageUrl);
                                    break;
                                case "QuotedTweet":
                                    PopupToastNotification(PopupNotificationType.QuotedTweet,
                                        string.Format(_resourceLoader.GetString("Notification_QuotedTweet_QuotedTweet"),
                                            "@" + e.EventMessage.Source.ScreenName), e.EventMessage.TargetStatus.Text,
                                        e.EventMessage.Source.ProfileImageUrl);
                                    break;
                                case "Unfavorite":
                                    PopupToastNotification(PopupNotificationType.Unfavorite,
                                        string.Format(_resourceLoader.GetString("Notification_Unfavorite_Unfavorite"),
                                            "@" + e.EventMessage.Source.ScreenName), e.EventMessage.TargetStatus.Text,
                                        e.EventMessage.Source.ProfileImageUrl);
                                    break;
                                case "Follow":
                                    PopupToastNotification(PopupNotificationType.Follow,
                                        string.Format(_resourceLoader.GetString("Notification_Follow_Follow"),
                                            "@" + e.EventMessage.Source.ScreenName),
                                        imageUrl: e.EventMessage.Source.ProfileImageUrl);
                                    break;
                            }
                            break;
                    }
                });
        }

        public void UpdateTileNotification(TileNotificationType type, string text, string imageUrl = "")
        {
            switch (type)
            {
                case TileNotificationType.Images:
                    if (SettingService.Setting.TileNotification != SettingSupport.TileNotificationEnum.Images)
                        return;

                    break;

                case TileNotificationType.Mentions:
                    if (SettingService.Setting.TileNotification != SettingSupport.TileNotificationEnum.Mentions)
                        return;

                    break;
            }

            var tileBindingContent = new TileBindingContentAdaptive
            {
                Children =
                {
                    new AdaptiveText {Text = text, HintStyle = AdaptiveTextStyle.Caption, HintWrap = true}
                }
            };

            if (!string.IsNullOrWhiteSpace(imageUrl))
                tileBindingContent.PeekImage = new TilePeekImage {Source = imageUrl};

            var tileBinding = new TileBinding
            {
                Branding = TileBranding.Auto,
                Content = tileBindingContent,
                DisplayName = "Flantter"
            };

            var tileContent = new TileContent
            {
                Visual = new TileVisual
                {
                    TileSmall = tileBinding,
                    TileMedium = tileBinding,
                    TileLarge = tileBinding,
                    TileWide = tileBinding
                }
            };

            var n = new TileNotification(tileContent.GetXml());
            TileUpdateManager.CreateTileUpdaterForApplication().Update(n);
        }

        public void PopupToastNotification(PopupNotificationType type, string text, string text2 = "",
            string imageUrl = "", string inlineImageUrl = "")
        {
            switch (type)
            {
                case PopupNotificationType.DirectMessage:
                    if (!SettingService.Setting.DirectMessageNotification)
                        return;

                    if (SettingService.Setting.PushNotification)
                        return;

                    break;
                case PopupNotificationType.Favorite:
                    if (!SettingService.Setting.FavoriteNotification)
                        return;

                    if (SettingService.Setting.PushNotification)
                        return;

                    break;
                case PopupNotificationType.Follow:
                    if (!SettingService.Setting.FollowNotification)
                        return;

                    if (SettingService.Setting.PushNotification)
                        return;

                    break;
                case PopupNotificationType.Mention:
                    if (!SettingService.Setting.MentionNotification)
                        return;

                    if (SettingService.Setting.PushNotification)
                        return;

                    break;
                case PopupNotificationType.QuotedTweet:
                    if (!SettingService.Setting.QuotedTweetNotification)
                        return;

                    break;
                case PopupNotificationType.Retweet:
                    if (!SettingService.Setting.RetweetNotification)
                        return;

                    if (SettingService.Setting.PushNotification)
                        return;

                    break;
                case PopupNotificationType.System:
                    if (!SettingService.Setting.SystemNotification)
                        return;

                    break;
                case PopupNotificationType.Unfavorite:
                    if (!SettingService.Setting.UnfavoriteNotification)
                        return;

                    break;
                case PopupNotificationType.TweetCompleted:
                    if (!SettingService.Setting.TweetCompleteNotification)
                        return;

                    break;
            }

            var toastContent = new ToastContent
            {
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = text
                            }
                        }
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(text2))
                toastContent.Visual.BindingGeneric.Children.Add(new AdaptiveText {Text = text2});

            if (!string.IsNullOrWhiteSpace(imageUrl))
                toastContent.Visual.BindingGeneric.AppLogoOverride = new ToastGenericAppLogo {Source = imageUrl};

            if (!string.IsNullOrWhiteSpace(inlineImageUrl))
                toastContent.Visual.BindingGeneric.Children.Add(new AdaptiveImage {Source = inlineImageUrl});

            if (!SettingService.Setting.NotificationSound)
                toastContent.Audio = new ToastAudio {Silent = true};

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}