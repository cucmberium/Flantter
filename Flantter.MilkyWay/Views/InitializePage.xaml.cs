using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Contents;
using Microsoft.Practices.Prism.StoreApps;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using WinRTXamlToolkit.AwaitableUI;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace Flantter.MilkyWay.Views
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class InitializePage : VisualStateAwarePage
    {
        private ResourceLoader resourceLoader;

        public InitializePage()
        {
            this.InitializeComponent();
            resourceLoader = new ResourceLoader();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            this.Frame.BackStack.Clear();
            this.Frame.ForwardStack.Clear();

            await (this.Resources["InitialAnimation"] as Storyboard).BeginAsync();

            var authorizePopup = new AuthorizePopup();

            while (true)
            {
                await Task.Delay(500);

                var account = await authorizePopup.ShowAsync();
                if (account == null)
                    continue;

                AdvancedSettingService.AdvancedSetting.Account = new ObservableCollection<AccountSetting>();
                AdvancedSettingService.AdvancedSetting.Account.Add(new AccountSetting()
                {
                    AccessToken = account.AccessToken,
                    AccessTokenSecret = account.AccessTokenSecret,
                    ConsumerKey = account.ConsumerKey,
                    ConsumerSecret = account.ConsumerSecret,
                    ScreenName = account.ScreenName,
                    UserId = account.UserId,
                    Column = new ObservableCollection<ColumnSetting>() 
                    {
                        new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.Home, AutoRefresh = false, AutoRefreshTimerInterval = 60.0, Filter = "()", Name = "Home", Parameter = string.Empty, Streaming = true, Index = 0, DisableStartupRefresh = false, FetchingNumberOfTweet = 100 },
                        new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.Mentions, AutoRefresh = false, AutoRefreshTimerInterval = 180.0, Filter = "()", Name = "Mentions", Parameter = string.Empty, Streaming = false, Index = 1, DisableStartupRefresh = false, FetchingNumberOfTweet = 40 },
                        new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.DirectMessages, AutoRefresh = false, AutoRefreshTimerInterval = 180.0, Filter = "()", Name = "DirectMessages", Parameter = string.Empty, Streaming = false, Index = 2, DisableStartupRefresh = false, FetchingNumberOfTweet = 40 },
                        new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.Events, AutoRefresh = false, AutoRefreshTimerInterval = 180.0, Filter = "()", Name = "Events", Parameter = string.Empty, Streaming = false, Index = 3, DisableStartupRefresh = false, FetchingNumberOfTweet = 100 },
                        new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.Favorites, AutoRefresh = false, AutoRefreshTimerInterval = 180.0, Filter = "()", Name = "Favorites", Parameter = string.Empty, Streaming = false, Index = 4, DisableStartupRefresh = false, FetchingNumberOfTweet = 40 },
                    },
                    IsEnabled = true,
                });

                try
                {
                    var advancedSetting = await ApplicationData.Current.RoamingFolder.CreateFileAsync("account.xml", CreationCollisionOption.ReplaceExisting);
                    using (var s = await advancedSetting.OpenStreamForWriteAsync())
                        AdvancedSettingService.AdvancedSetting.SaveToStream(s);
                }
                catch (Exception ex)
                {
                }

                break;
            }

            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
