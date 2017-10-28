using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Behaviors;
using Flantter.MilkyWay.Views.Contents.ShareContract;
using Microsoft.HockeyApp;
using Prism.Windows;

namespace Flantter.MilkyWay
{
    sealed partial class App : PrismApplication
    {
        private bool _appLaunched;

        public App()
        {
            InitializeComponent();

            HockeyClient.Current.Configure("12d0f9780e5645e3bf16ee0557054a03")
                .SetExceptionDescriptionLoader(ex => ex.ToString());

            UnhandledException += App_UnhandledException;
            Suspending += App_Suspending;
            Resuming += App_Resuming;

            RequestedTheme = (ApplicationTheme) Enum.Parse(typeof(ApplicationTheme),
                SettingService.Setting.Theme.ToString(), true);
        }

        private async void App_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
            
            deferral.Complete();
        }

        private void App_Resuming(object sender, object e)
        {
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // 別変数に最初に入れないと次アクセスからNull , 一番最初のUnhandledExceptionじゃないとNull
            var stacktrace = e.Exception.StackTrace;
            if (SettingService.Setting.PreventForcedTermination)
                e.Handled = true;

            Debug.WriteLine(stacktrace);
        }

        protected override async Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
#if _DEBUG // デバッグ用コードをここに突っ込む
            var sw = System.Diagnostics.Stopwatch.StartNew();
            //Models.Filter.Compiler.Compile("(!(User.ScreenName In [\"cucmberium\", \"cucmberium_sub\"] || User.Id !In [10, 20, 30]) && RetweetCount >= FavoriteCount * 10 + 10 / (2 + 3))");

            //Models.Filter.Compiler.Compile("(Text RegexMatch \"(ふらん|フラン)ちゃんかわいい\" || Text Contains \"Flantter\")");

            //Models.Filter.Compiler.Compile("(Text StartsWith \"島風\" || Text EndsWith \"天津風\")");

            Models.Filter.Compiler.Compile("(Random(0, 10) == 0 && Text Contains \"チノちゃん\")");

            sw.Stop();
            System.Diagnostics.Debug.WriteLine(sw.Elapsed);
#endif
            await AdvancedSettingService.AdvancedSetting.LoadFromAppSettings();

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size {Width = 320, Height = 500});

            if (AdvancedSettingService.AdvancedSetting.Accounts == null ||
                AdvancedSettingService.AdvancedSetting.Accounts.Count == 0)
                NavigationService.Navigate("Initialize", "");
            else
                NavigationService.Navigate("Main", "");

            DeviceGestureService.GoBackRequested += (s, e) =>
            {
                var behavior = ShowSettingsFlyoutAction.GetForCurrentView();
                if (behavior == null)
                    return;

                if (behavior.ShowingPopupCount == 0)
                    return;

                e.Handled = true;
                e.Cancel = true;
                behavior.HideTopPopup();
            };

            _appLaunched = true;
        }

        protected override async Task OnActivateApplicationAsync(IActivatedEventArgs args)
        {
            if (!_appLaunched)
            {
                await AdvancedSettingService.AdvancedSetting.LoadFromAppSettings();

                ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size {Width = 320, Height = 500});

                if (AdvancedSettingService.AdvancedSetting.Accounts == null ||
                    AdvancedSettingService.AdvancedSetting.Accounts.Count == 0)
                    NavigationService.Navigate("Initialize", "");
                else
                    NavigationService.Navigate("Main", "");

                DeviceGestureService.GoBackRequested += (s, e) =>
                {
                    var behavior = ShowSettingsFlyoutAction.GetForCurrentView();
                    if (behavior == null)
                        return;

                    if (behavior.ShowingPopupCount == 0)
                        return;

                    e.Handled = true;
                    e.Cancel = true;
                    behavior.HideTopPopup();
                };

                _appLaunched = true;
            }
        }

        protected override async void OnShareTargetActivated(ShareTargetActivatedEventArgs e)
        {
            base.OnShareTargetActivated(e);
            if (!_appLaunched)
                await AdvancedSettingService.AdvancedSetting.LoadFromAppSettings();
            var shareTargetPage = new StatusShareContract();
            shareTargetPage.Activate(e);
        }
    }
}