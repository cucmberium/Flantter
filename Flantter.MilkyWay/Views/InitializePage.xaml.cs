﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Contents.Authorize;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Prism.Windows.Mvvm;

namespace Flantter.MilkyWay.Views
{
    public sealed partial class InitializePage : SessionStateAwarePage
    {
        public InitializePage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Frame.BackStack.Clear();
            Frame.ForwardStack.Clear();

            var applicationView = ApplicationView.GetForCurrentView();
            applicationView.TitleBar.BackgroundColor =
                ((SolidColorBrush) Application.Current.Resources["TitleBarBackgroundBrush"]).Color;
            applicationView.TitleBar.ButtonBackgroundColor =
                ((SolidColorBrush) Application.Current.Resources["TitleBarButtonBackgroundBrush"]).Color;
            applicationView.TitleBar.ButtonForegroundColor =
                ((SolidColorBrush) Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color;
            applicationView.TitleBar.ButtonInactiveBackgroundColor =
                ((SolidColorBrush) Application.Current.Resources["TitleBarButtonInactiveBackgroundBrush"]).Color;
            applicationView.TitleBar.ButtonInactiveForegroundColor =
                ((SolidColorBrush) Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color;

            await (Resources["InitialAnimation"] as Storyboard).BeginAsync();

            var authorizePopup = new AuthorizePopup();

            while (true)
            {
                await Task.Delay(500);

                var account = await authorizePopup.ShowAsync();
                if (account == null)
                    continue;

                AdvancedSettingService.AdvancedSetting.MuteClients = new ObservableCollection<string>();
                AdvancedSettingService.AdvancedSetting.MuteUsers = new ObservableCollection<string>();
                AdvancedSettingService.AdvancedSetting.MuteWords = new ObservableCollection<string>();

                AdvancedSettingService.AdvancedSetting.Accounts = new ObservableCollection<AccountSetting>();
                var accountSetting = new AccountSetting
                {
                    AccessToken = account.AccessToken,
                    AccessTokenSecret = account.AccessTokenSecret,
                    ConsumerKey = account.ConsumerKey,
                    ConsumerSecret = account.ConsumerSecret,
                    ScreenName = account.ScreenName,
                    UserId = account.UserId,
                    Platform = account.Service == "Twitter"
                        ? SettingSupport.PlatformEnum.Twitter
                        : SettingSupport.PlatformEnum.Mastodon,
                    Instance = account.Instance,

                    Column = new ObservableCollection<ColumnSetting>
                    {
                        new ColumnSetting
                        {
                            Action = SettingSupport.ColumnTypeEnum.Home,
                            AutoRefresh = false,
                            AutoRefreshTimerInterval = 60.0,
                            Filter = "()",
                            Name = "Home",
                            Parameter = string.Empty,
                            Streaming = true,
                            Index = 0,
                            DisableStartupRefresh = false,
                            FetchingNumberOfTweet = 100,
                            Identifier = DateTime.Now.Ticks
                        },
                        new ColumnSetting
                        {
                            Action = SettingSupport.ColumnTypeEnum.Mentions,
                            AutoRefresh = false,
                            AutoRefreshTimerInterval = 180.0,
                            Filter = "()",
                            Name = "Mentions",
                            Parameter = string.Empty,
                            Streaming = false,
                            Index = 1,
                            DisableStartupRefresh = false,
                            FetchingNumberOfTweet = 40,
                            Identifier = DateTime.Now.Ticks + 1
                        },
                        new ColumnSetting
                        {
                            Action = SettingSupport.ColumnTypeEnum.Favorites,
                            AutoRefresh = false,
                            AutoRefreshTimerInterval = 180.0,
                            Filter = "()",
                            Name = "Favorites",
                            Parameter = string.Empty,
                            Streaming = false,
                            Index = 2,
                            DisableStartupRefresh = false,
                            FetchingNumberOfTweet = 40,
                            Identifier = DateTime.Now.Ticks + 4
                        }
                    },
                    IsEnabled = true
                };
                if (accountSetting.Platform == SettingSupport.PlatformEnum.Mastodon)
                    accountSetting.Column.Add(new ColumnSetting
                    {
                        Action = SettingSupport.ColumnTypeEnum.Local,
                        AutoRefresh = false,
                        AutoRefreshTimerInterval = 180.0,
                        Filter = "()",
                        Name = "Local",
                        Parameter = string.Empty,
                        Streaming = true,
                        Index = 3,
                        DisableStartupRefresh = false,
                        FetchingNumberOfTweet = 40,
                        Identifier = DateTime.Now.Ticks + 5
                    });

                AdvancedSettingService.AdvancedSetting.Accounts.Add(accountSetting);

                try
                {
                    await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
                }
                catch
                {
                }

                break;
            }

            Frame.Navigate(typeof(MainPage), "");
        }
    }
}