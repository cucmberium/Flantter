using CoreTweet;
using CoreTweet.Core;
using Newtonsoft.Json.Linq;
using NotificationsExtensions.Tiles;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Flantter.MilkyWay.BackgroundTask
{
    public sealed class BackgroundWorker : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            
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
                var json = string.Empty;

                var readStorageFile = await ApplicationData.Current.RoamingFolder.GetFileAsync("setting.xml");
                using (var s = await readStorageFile.OpenStreamForReadAsync())
                using (var st = new System.IO.StreamReader(s))
                {
                    json = st.ReadToEnd();
                }

                var jTokens = JToken.Parse(json);
                var jaccounts = jTokens.First(x => (x as JProperty)?.Name == "Accounts") as JProperty;
                var accounts = jaccounts.Value.ToObject<List<AccountSetting>>();

                var account = accounts.First(x => x.IsEnabled);
                var tokens = Tokens.Create(account.ConsumerKey, account.ConsumerSecret, account.AccessToken, account.AccessTokenSecret);
                
                var type = (int)Windows.Storage.ApplicationData.Current.RoamingSettings.Values["TileNotification"];
                
                if (type == 1)
                {
                    var statuses = await tokens.Statuses.MentionsTimelineAsync(count => 5);
                    foreach (var status in statuses)
                    {
                        this.UpdateTileNotification(status.User.Name + "(@" + status.User.ScreenName + ")" + "\n" + status.Text);
                    }
                }
                else if (type == 2)
                {
                    var statuses = (await tokens.Statuses.HomeTimelineAsync(count => 20)).Select(x => x.RetweetedStatus == null ? x : x.RetweetedStatus);
                    foreach (var status in statuses)
                    {
                        if (status.Entities.Media == null || status.Entities.Media.Length == 0)
                            continue;

                        this.UpdateTileNotification(status.User.Name + "(@" + status.User.ScreenName + ")" + "\n" + status.Text, status.Entities.Media.First().MediaUrl);
                    }
                }                
            }
            catch
            {
            }

            deferral.Complete();
            return;
        }

        private void UpdateTileNotification(string text, string imageUrl = "")
        {

            var tileBindingContent = new TileBindingContentAdaptive
            {
                Children =
                {
                    new TileText { Text = text, Style = TileTextStyle.Caption, Wrap = true },
                }
            };

            if (!string.IsNullOrWhiteSpace(imageUrl))
                tileBindingContent.PeekImage = new TilePeekImage { Source = new TileImageSource(imageUrl) };

            var tileBinding = new TileBinding
            {
                Branding = TileBranding.Auto,
                Content = tileBindingContent,
                DisplayName = "Flantter.MilkyWay",
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
    }

    internal class AccountSetting
    {
        private string _Name;
        public string Name
        {
            get { return this._Name; }
            set { this._Name = value; }
        }

        private string _ScreenName;
        public string ScreenName
        {
            get { return this._ScreenName; }
            set { this._ScreenName = value; }
        }

        private long _UserId;
        public long UserId
        {
            get { return this._UserId; }
            set { this._UserId = value; }
        }

        private string _ConsumerKey;
        public string ConsumerKey
        {
            get { return _ConsumerKey; }
            set { this._ConsumerKey = value; }
        }

        private string _ConsumerSecret;
        public string ConsumerSecret
        {
            get { return _ConsumerSecret; }
            set { this._ConsumerSecret = value; }
        }

        private string _AccessToken;
        public string AccessToken
        {
            get { return _AccessToken; }
            set { this._AccessToken = value; }
        }

        private string _AccessTokenSecret;
        public string AccessTokenSecret
        {
            get { return _AccessTokenSecret; }
            set { this._AccessTokenSecret = value; }
        }

        public List<ColumnSetting> Column { get; set; }

        private bool _IncludeFollowingsActivity;
        public bool IncludeFollowingsActivity
        {
            get { return _IncludeFollowingsActivity; }
            set { this._IncludeFollowingsActivity = value; }
        }

        private bool _PossiblySensitive;
        public bool PossiblySensitive
        {
            get { return _PossiblySensitive; }
            set { this._PossiblySensitive = value; }
        }

        private string _ProfileImageUrl;
        public string ProfileImageUrl
        {
            get { return _ProfileImageUrl; }
            set { this._ProfileImageUrl = value; }
        }

        private string _ProfileBannerUrl;
        public string ProfileBannerUrl
        {
            get { return _ProfileBannerUrl; }
            set { this._ProfileBannerUrl = value; }
        }

        private bool _IsEnabled;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { this._IsEnabled = value; }
        }
    }

    internal class ColumnSetting
    {
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { this._Name = value; }
        }

        /*private SettingSupport.ColumnTypeEnum _Action;
        public SettingSupport.ColumnTypeEnum Action
        {
            get { return _Action; }
            set { this._Action = value; }
        }*/

        private string _Parameter;
        public string Parameter
        {
            get { return _Parameter; }
            set { this._Parameter = value; }
        }

        private string _Filter;
        public string Filter
        {
            get { return _Filter; }
            set { this._Filter = value; }
        }

        private bool _DisableStartupRefresh;
        public bool DisableStartupRefresh
        {
            get { return _DisableStartupRefresh; }
            set { this._DisableStartupRefresh = value; }
        }

        private bool _AutoRefresh;
        public bool AutoRefresh
        {
            get { return _AutoRefresh; }
            set { this._AutoRefresh = value; }
        }

        private double _AutoRefreshTimerInterval;
        public double AutoRefreshTimerInterval
        {
            get { return _AutoRefreshTimerInterval; }
            set { this._AutoRefreshTimerInterval = value; }
        }

        private bool _Streaming;
        public bool Streaming
        {
            get { return _Streaming; }
            set { this._Streaming = value; }
        }

        private int _Index;
        public int Index
        {
            get { return _Index; }
            set { this._Index = value; }
        }

        private int _FetchingNumberOfTweet;
        public int FetchingNumberOfTweet
        {
            get { return _FetchingNumberOfTweet; }
            set { this._FetchingNumberOfTweet = value; }
        }

        private long _Identifier;
        public long Identifier
        {
            get { return _Identifier; }
            set { this._Identifier = value; }
        }
    }
}
