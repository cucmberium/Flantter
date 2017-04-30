using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Notifications;
using CoreTweet;
using Newtonsoft.Json.Linq;
using NotificationsExtensions.Tiles;
using NotificationsExtensions.Toasts;

namespace Flantter.MilkyWay.BackgroundTask
{
    public sealed class BackgroundWorker : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            try
            {
#if _DEBUG
                var toastContent = new ToastContent();
                toastContent.Visual = new ToastVisual();
                toastContent.Visual.TitleText = new ToastText() { Text = "DebugMsg" };
                toastContent.Visual.BodyTextLine1 = new ToastText() { Text = "Background task is running!!" };

                var toast = new ToastNotification(toastContent.GetXml());
                ToastNotificationManager.CreateToastNotifier().Show(toast);
#endif
                string json;

                var readStorageFile = await ApplicationData.Current.RoamingFolder.GetFileAsync("setting.xml");
                using (var s = await readStorageFile.OpenStreamForReadAsync())
                using (var st = new StreamReader(s))
                {
                    json = st.ReadToEnd();
                }

                var jTokens = JToken.Parse(json);
                var jaccounts = jTokens.First(x => (x as JProperty)?.Name == "Accounts") as JProperty;
                var accounts = jaccounts.Value.ToObject<List<AccountSetting>>();

                var resourceLoader = new ResourceLoader();

                var tileNotificationType =
                    ApplicationData.Current.RoamingSettings.Values.ContainsKey("TileNotification")
                        ? (int) ApplicationData.Current.RoamingSettings.Values["TileNotification"]
                        : 0;
                if (tileNotificationType != 0)
                {
                    var account = accounts.First(x => x.IsEnabled);
                    var tokens = Tokens.Create(account.ConsumerKey, account.ConsumerSecret, account.AccessToken,
                        account.AccessTokenSecret);

                    if (tileNotificationType == 1)
                    {
                        var statuses = await tokens.Statuses.MentionsTimelineAsync(count => 5);
                        foreach (var status in statuses)
                            UpdateTileNotification(status.User.Name + "(@" + status.User.ScreenName + ")" + "\n" +
                                                   status.Text);
                    }
                    else if (tileNotificationType == 2)
                    {
                        var statuses =
                            (await tokens.Statuses.HomeTimelineAsync(count => 20)).Select(x => x.RetweetedStatus ?? x);
                        foreach (var status in statuses)
                        {
                            if (status.Entities.Media == null || status.Entities.Media.Length == 0)
                                continue;

                            UpdateTileNotification(
                                status.User.Name + "(@" + status.User.ScreenName + ")" + "\n" + status.Text,
                                status.Entities.Media.First().MediaUrl);
                        }
                    }
                }

                var backgroundNotification =
                    ApplicationData.Current.RoamingSettings.Values.ContainsKey("BackgroundNotification") &&
                    (bool) ApplicationData.Current.RoamingSettings.Values["BackgroundNotification"];
                if (backgroundNotification)
                {
                    var latestNotificationDate =
                        new DateTimeOffset(
                            (long) ApplicationData.Current.LocalSettings.Values["LatestNotificationDate"],
                            DateTimeOffset.Now.Offset);

                    foreach (var account in accounts)
                    {
                        var tokens = Tokens.Create(account.ConsumerKey, account.ConsumerSecret, account.AccessToken,
                            account.AccessTokenSecret);

                        var statuses =
                            (await tokens.Statuses.MentionsTimelineAsync(count => 10)).Where(
                                x => x.CreatedAt.ToLocalTime() > latestNotificationDate);
                        foreach (var status in statuses)
                            PopupToastNotification("Mention",
                                string.Format(resourceLoader.GetString("Notification_Mention_Mention"),
                                    status.User.Name), status.Text, status.User.ProfileImageUrl,
                                "Reply to @" + status.User.ScreenName, "Assets/Tweet.png",
                                "mention" + "," + account.ScreenName + "," + status.User.ScreenName + "," + status.Id,
                                status.Entities?.Media?.Length != 0 ? status.Entities.Media.First().MediaUrl : "");

                        var directMessages =
                            (await tokens.DirectMessages.ReceivedAsync(count => 10)).Where(
                                x => x.CreatedAt.ToLocalTime() > latestNotificationDate);
                        foreach (var dm in directMessages)
                            PopupToastNotification("DirectMessage",
                                string.Format(resourceLoader.GetString("Notification_DirectMessage_DirectMessage"),
                                    dm.Sender.Name), dm.Text, dm.Sender.ProfileImageUrl,
                                "Send DM to @" + dm.Sender.ScreenName, "Assets/DM.png",
                                "dm" + "," + account.ScreenName + "," + dm.Sender.ScreenName);
                    }

                    ApplicationData.Current.LocalSettings.Values["LatestNotificationDate"] = DateTimeOffset.Now.Ticks;
                }
            }
            catch
            {
            }

            deferral.Complete();
        }

        private void UpdateTileNotification(string text, string imageUrl = "")
        {
            var tileBindingContent = new TileBindingContentAdaptive
            {
                Children =
                {
                    new TileText {Text = text, Style = TileTextStyle.Caption, Wrap = true}
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


        private void PopupToastNotification(string type, string text, string text2 = "", string imageUrl = "",
            string textboxPlaceholder = "", string buttonImageUrl = "", string param = "", string inlineImageUrl = "")
        {
            var toastContent = new ToastContent
            {
                Visual = new ToastVisual
                {
                    TitleText = new ToastText {Text = type},
                    BodyTextLine1 = new ToastText {Text = text}
                }
            };

            if (!string.IsNullOrWhiteSpace(text2))
                toastContent.Visual.BodyTextLine2 = new ToastText {Text = text2};

            if (!string.IsNullOrWhiteSpace(imageUrl))
                toastContent.Visual.AppLogoOverride = new ToastAppLogo {Source = new ToastImageSource(imageUrl)};

            if (!string.IsNullOrWhiteSpace(inlineImageUrl))
                toastContent.Visual.InlineImages.Add(new ToastImage {Source = new ToastImageSource(inlineImageUrl)});

            if (!string.IsNullOrWhiteSpace(textboxPlaceholder) && !string.IsNullOrWhiteSpace(buttonImageUrl) &&
                !string.IsNullOrWhiteSpace(param))
            {
                var toastAction = new ToastActionsCustom
                {
                    Inputs = {new ToastTextBox("tweet") {PlaceholderContent = textboxPlaceholder}},
                    Buttons = {new ToastButton("send", param) {TextBoxId = "tweet", ImageUri = buttonImageUrl}}
                };

                toastContent.Actions = toastAction;
            }

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }

    internal class AccountSetting
    {
        public string Name { get; set; }

        public string ScreenName { get; set; }

        public long UserId { get; set; }

        public string ConsumerKey { get; set; }

        public string ConsumerSecret { get; set; }

        public string AccessToken { get; set; }

        public string AccessTokenSecret { get; set; }

        public List<ColumnSetting> Column { get; set; }

        public bool IncludeFollowingsActivity { get; set; }

        public bool PossiblySensitive { get; set; }

        public string ProfileImageUrl { get; set; }

        public string ProfileBannerUrl { get; set; }

        public bool IsEnabled { get; set; }
    }

    internal class ColumnSetting
    {
        /*private SettingSupport.ColumnTypeEnum _Action;
        public SettingSupport.ColumnTypeEnum Action
        {
            get { return _Action; }
            set { this._Action = value; }
        }*/

        public string Name { get; set; }

        public string Parameter { get; set; }

        public string Filter { get; set; }

        public bool DisableStartupRefresh { get; set; }

        public bool AutoRefresh { get; set; }

        public double AutoRefreshTimerInterval { get; set; }

        public bool Streaming { get; set; }

        public int Index { get; set; }

        public int FetchingNumberOfTweet { get; set; }

        public long Identifier { get; set; }
    }
}