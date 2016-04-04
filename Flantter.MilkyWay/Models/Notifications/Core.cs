using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Setting;
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
    public enum NotificationType
    {
        System,
        Retweet,
        Mention,
        DirectMessage,
        Favorite,
        Unfavorite,
        Follow,
        QuotedTweet,
    }

    public class Core
    {
        private static Core _Instance = new Core();

        public static Core Instance
        {
            get { return _Instance; }
        }

        private ResourceLoader _ResourceLoader;
        private IDisposable observeTweetReceive;
        public void Initialize()
        {
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
                            if (e.Status.HasRetweetInformation && e.Status.User.Id == e.UserId)
                                this.PopupToastNotification(NotificationType.Retweet, string.Format(_ResourceLoader.GetString("Notification_Retweet_Retweet"), e.Status.RetweetInformation.User.Name), e.Status.Text, e.Status.RetweetInformation.User.ProfileImageUrl);

                            if (e.Status.InReplyToUserId == e.UserId)
                                this.PopupToastNotification(NotificationType.Mention, string.Format(_ResourceLoader.GetString("Notification_Mention_Mention"), e.Status.User.Name), e.Status.Text, e.Status.User.ProfileImageUrl);

                            break;

                        case TweetEventArgs.TypeEnum.DirectMessage:
                            if (e.DirectMessage.Recipient.Id == e.UserId)
                                this.PopupToastNotification(NotificationType.DirectMessage, string.Format(_ResourceLoader.GetString("Notification_DirectMessage_DirectMessage"), e.DirectMessage.Sender.Name), e.DirectMessage.Text, e.DirectMessage.Sender.ProfileImageUrl);

                            break;

                        case TweetEventArgs.TypeEnum.EventMessage:
                            if (e.EventMessage.Target.Id == e.UserId && e.EventMessage.Source.Id != e.UserId && e.EventMessage.Type == "Favorite")
                                this.PopupToastNotification(NotificationType.Favorite, string.Format(_ResourceLoader.GetString("Notification_Favorite_Favorite"), e.EventMessage.Source.Name), e.EventMessage.TargetStatus.Text, e.EventMessage.Source.ProfileImageUrl);

                            else if (e.EventMessage.Target.Id == e.UserId && e.EventMessage.Source.Id != e.UserId && e.EventMessage.Type == "QuotedTweet")
                                this.PopupToastNotification(NotificationType.QuotedTweet, string.Format(_ResourceLoader.GetString("Notification_QuotedTweet_QuotedTweet"), e.EventMessage.Source.Name), e.EventMessage.TargetStatus.Text, e.EventMessage.Source.ProfileImageUrl);

                            else if (e.EventMessage.Target.Id == e.UserId && e.EventMessage.Source.Id != e.UserId && e.EventMessage.Type == "Unfavorite")
                                this.PopupToastNotification(NotificationType.Unfavorite, string.Format(_ResourceLoader.GetString("Notification_Unfavorite_Unfavorite"), e.EventMessage.Source.Name), e.EventMessage.TargetStatus.Text, e.EventMessage.Source.ProfileImageUrl);

                            else if (e.EventMessage.Target.Id == e.UserId && e.EventMessage.Source.Id != e.UserId && e.EventMessage.Type == "Follow")
                                this.PopupToastNotification(NotificationType.Follow, string.Format(_ResourceLoader.GetString("Notification_Follow_Follow"), e.EventMessage.Source.Name), imageUrl: e.EventMessage.Source.ProfileImageUrl);

                            break;
                    }
                });
        }

        public void PopupToastNotification(NotificationType type, string text, string text2 = "", string imageUrl = "")
        {
            switch (type)
            {
                case NotificationType.DirectMessage:
                    if (!SettingService.Setting.DirectMessageNotification)
                        return;

                    break;
                    
                case NotificationType.Favorite:
                    if (!SettingService.Setting.FavoriteNotification)
                        return;

                    break;

                case NotificationType.Follow:
                    if (!SettingService.Setting.FollowNotification)
                        return;

                    break;

                case NotificationType.Mention:
                    if (!SettingService.Setting.MentionNotification)
                        return;

                    break;

                case NotificationType.QuotedTweet:
                    if (!SettingService.Setting.QuotedTweetNotification)
                        return;

                    break;

                case NotificationType.Retweet:
                    if (!SettingService.Setting.RetweetNotification)
                        return;

                    break;

                case NotificationType.System:
                    if (!SettingService.Setting.SystemNotification)
                        return;

                    break;

                case NotificationType.Unfavorite:
                    if (!SettingService.Setting.UnfavoriteNotification)
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

            if (!SettingService.Setting.NotificationSound)
                toastContent.Audio = new ToastAudio() { Silent = true };

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
