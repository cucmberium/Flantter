using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Setting;
using NotificationsExtensions.Tiles;
using NotificationsExtensions.Toasts;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Notifications;

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
        TweetCompleted,
    }

    public enum TileNotificationType
    {
        Images,
        Mentions,
    }

    public class Core
    {
        private static Core _Instance = new Core();

        public static Core Instance
        {
            get { return _Instance; }
        }

        private Core()
        {
        }

        private ResourceLoader _ResourceLoader;
        private IDisposable observeTweetReceive;
        public void Initialize()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);

            _ResourceLoader = new ResourceLoader();

            observeTweetReceive = Observable.FromEvent<EventHandler<TweetEventArgs>, TweetEventArgs>(
                h => (sender, e) => h(e),
                h => Connecter.Instance.TweetReceive_CommandExecute += h,
                h => Connecter.Instance.TweetReceive_CommandExecute -= h)
                .Where(e => e.Streaming)
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(e => 
                {
                    switch (e.Type)
                    {
                        case TweetEventArgs.TypeEnum.Status:
                            if (e.Parameter.Contains("favorites://"))
                                break;

                            if (e.Status.HasRetweetInformation && e.Status.User.Id == e.UserId && e.Parameter.Contains("home://"))
                                this.PopupToastNotification(PopupNotificationType.Retweet, string.Format(_ResourceLoader.GetString("Notification_Retweet_Retweet"), e.Status.RetweetInformation.User.Name), e.Status.Text, e.Status.RetweetInformation.User.ProfileImageUrl);

                            if (e.Status.InReplyToUserId == e.UserId && e.Parameter.Contains("home://"))
                                this.PopupToastNotification(PopupNotificationType.Mention, string.Format(_ResourceLoader.GetString("Notification_Mention_Mention"), e.Status.User.Name), e.Status.Text, e.Status.User.ProfileImageUrl, e.Status.Entities.Media.Count != 0 ? e.Status.Entities.Media.First().MediaThumbnailUrl : "");

                            if (e.Status.Entities.Media.Where(x => x.Type == "Image").Count() > 0 && !e.Status.User.IsProtected)
                                this.UpdateTileNotification(TileNotificationType.Images, e.Status.User.Name + "(@" + e.Status.User.ScreenName + ")" + "\n" + e.Status.Text, e.Status.Entities.Media.Where(x => x.Type == "Image").First().MediaUrl);

                            if (e.Status.InReplyToUserId == e.UserId)
                                this.UpdateTileNotification(TileNotificationType.Mentions, e.Status.User.Name + "(@" + e.Status.User.ScreenName + ")" + "\n" + e.Status.Text);
                            
                            break;

                        case TweetEventArgs.TypeEnum.DirectMessage:
                            if (e.DirectMessage.Recipient.Id == e.UserId)
                                this.PopupToastNotification(PopupNotificationType.DirectMessage, string.Format(_ResourceLoader.GetString("Notification_DirectMessage_DirectMessage"), e.DirectMessage.Sender.Name), e.DirectMessage.Text, e.DirectMessage.Sender.ProfileImageUrl);

                            break;

                        case TweetEventArgs.TypeEnum.EventMessage:
                            if (e.EventMessage.Target.Id == e.UserId && e.EventMessage.Source.Id != e.UserId && e.EventMessage.Type == "Favorite")
                                this.PopupToastNotification(PopupNotificationType.Favorite, string.Format(_ResourceLoader.GetString("Notification_Favorite_Favorite"), e.EventMessage.Source.Name), e.EventMessage.TargetStatus.Text, e.EventMessage.Source.ProfileImageUrl);

                            else if (e.EventMessage.Target.Id == e.UserId && e.EventMessage.Source.Id != e.UserId && e.EventMessage.Type == "QuotedTweet")
                                this.PopupToastNotification(PopupNotificationType.QuotedTweet, string.Format(_ResourceLoader.GetString("Notification_QuotedTweet_QuotedTweet"), e.EventMessage.Source.Name), e.EventMessage.TargetStatus.Text, e.EventMessage.Source.ProfileImageUrl);

                            else if (e.EventMessage.Target.Id == e.UserId && e.EventMessage.Source.Id != e.UserId && e.EventMessage.Type == "Unfavorite")
                                this.PopupToastNotification(PopupNotificationType.Unfavorite, string.Format(_ResourceLoader.GetString("Notification_Unfavorite_Unfavorite"), e.EventMessage.Source.Name), e.EventMessage.TargetStatus.Text, e.EventMessage.Source.ProfileImageUrl);

                            else if (e.EventMessage.Target.Id == e.UserId && e.EventMessage.Source.Id != e.UserId && e.EventMessage.Type == "Follow")
                                this.PopupToastNotification(PopupNotificationType.Follow, string.Format(_ResourceLoader.GetString("Notification_Follow_Follow"), e.EventMessage.Source.Name), imageUrl: e.EventMessage.Source.ProfileImageUrl);

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
                    new TileText { Text = text, Style = TileTextStyle.Caption, Wrap = true },
                }
            };

            if (!string.IsNullOrWhiteSpace(imageUrl))
                tileBindingContent.PeekImage = new TilePeekImage { Source = imageUrl };

            var tileBinding = new TileBinding
            {
                Branding = TileBranding.Auto,
                Content = tileBindingContent,
                DisplayName = "Flantter",
            };

            var tileContent = new TileContent
            {
                Visual = new TileVisual
                {
                    TileSmall = tileBinding,
                    TileMedium = tileBinding,
                    TileLarge = tileBinding,
                    TileWide = tileBinding,
                }
            };

            var n = new TileNotification(tileContent.GetXml());
            TileUpdateManager.CreateTileUpdaterForApplication().Update(n);
            
        }

        public void PopupToastNotification(PopupNotificationType type, string text, string text2 = "", string imageUrl = "", string inlineImageUrl = "")
        {
            switch (type)
            {
                case PopupNotificationType.DirectMessage:
                    if (!SettingService.Setting.DirectMessageNotification)
                        return;

                    break;
                    
                case PopupNotificationType.Favorite:
                    if (!SettingService.Setting.FavoriteNotification)
                        return;

                    break;

                case PopupNotificationType.Follow:
                    if (!SettingService.Setting.FollowNotification)
                        return;

                    break;

                case PopupNotificationType.Mention:
                    if (!SettingService.Setting.MentionNotification)
                        return;

                    break;

                case PopupNotificationType.QuotedTweet:
                    if (!SettingService.Setting.QuotedTweetNotification)
                        return;

                    break;

                case PopupNotificationType.Retweet:
                    if (!SettingService.Setting.RetweetNotification)
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

            var toastContent = new ToastContent();
            toastContent.Visual = new ToastVisual();
            toastContent.Visual.TitleText = new ToastText() { Text = type.ToString() };
            toastContent.Visual.BodyTextLine1 = new ToastText() { Text = text };

            if (!string.IsNullOrWhiteSpace(text2))
                toastContent.Visual.BodyTextLine2 = new ToastText() { Text = text2 };

            if (!string.IsNullOrWhiteSpace(imageUrl))
                toastContent.Visual.AppLogoOverride = new ToastAppLogo() { Source = new ToastImageSource(imageUrl) };

            if (!string.IsNullOrWhiteSpace(inlineImageUrl))
                toastContent.Visual.InlineImages.Add(new ToastImage() { Source = new ToastImageSource(inlineImageUrl) });

            if (!SettingService.Setting.NotificationSound)
                toastContent.Audio = new ToastAudio() { Silent = true };

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
