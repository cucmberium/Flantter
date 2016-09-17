using CoreTweet;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Contents.ShareContract;
using Flantter.MilkyWay.Views.Util;
using Newtonsoft.Json.Linq;
using Prism.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// 空のアプリケーション テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234227 を参照してください

namespace Flantter.MilkyWay
{
    sealed partial class App : PrismApplication
    {
        public App()
        {
            Microsoft.HockeyApp.HockeyClient.Current.Configure("12d0f9780e5645e3bf16ee0557054a03");
            this.InitializeComponent();

            this.UnhandledException += App_UnhandledException;
            this.Suspending += App_Suspending;
            this.Resuming += App_Resuming;
        }

        private void App_Suspending(object sender, SuspendingEventArgs e)
        {
            if (BackgroundTaskRegistration.AllTasks.Count > 1)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                    task.Value.Unregister(true);
            }

            SettingService.Setting.LatestNotificationDate = DateTimeOffset.Now;

            if (SettingService.Setting.TileNotification == SettingSupport.TileNotificationEnum.None && !SettingService.Setting.BackgroundNotification)
                return;

            try
            {
                var trigger = new TimeTrigger(15, false);
                var taskBuilder = new BackgroundTaskBuilder();

                taskBuilder.Name = "Flantter_BackgroundTask";
                taskBuilder.TaskEntryPoint = "Flantter.MilkyWay.BackgroundTask.BackgroundWorker";
                taskBuilder.SetTrigger(trigger);
                taskBuilder.Register();
            }
            catch
            {
            }
        }

        private void App_Resuming(object sender, object e)
        {
            if (BackgroundTaskRegistration.AllTasks.Count > 1)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                    task.Value.Unregister(true);
            }

            SettingService.Setting.LatestNotificationDate = DateTimeOffset.Now;
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var stacktrace = e.Exception.StackTrace; // 別変数に最初に入れないと次アクセスからNull , 一番最初のUnhandledExceptionじゃないとNull
            if (SettingService.Setting.PreventForcedTermination)
                e.Handled = true;

            System.Diagnostics.Debug.WriteLine(stacktrace);
        }

        protected override async Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
#if _DEBUG
            // デバッグ用コードをここに突っ込む
            var sw = System.Diagnostics.Stopwatch.StartNew();
            //Models.Filter.Compiler.Compile("(!(User.ScreenName In [\"cucmberium\", \"cucmberium_sub\"] || User.Id !In [10, 20, 30]) && RetweetCount >= FavoriteCount * 10 + 10 / (2 + 3))");

            //Models.Filter.Compiler.Compile("(Text RegexMatch \"(ふらん|フラン)ちゃんかわいい\" || Text Contains \"Flantter\")");

            //Models.Filter.Compiler.Compile("(Text StartsWith \"島風\" || Text EndsWith \"天津風\")");

            Models.Filter.Compiler.Compile("(Random(0, 10) == 0 && Text Contains \"チノちゃん\")");

            sw.Stop();
            System.Diagnostics.Debug.WriteLine(sw.Elapsed);
#endif
            if (BackgroundTaskRegistration.AllTasks.Count > 1)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                    task.Value.Unregister(true);
            }

            await AdvancedSettingService.AdvancedSetting.LoadFromAppSettings();

            SettingService.Setting.LatestNotificationDate = DateTimeOffset.Now;

            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size { Width = 320, Height = 500 });

            if (AdvancedSettingService.AdvancedSetting.Accounts == null || AdvancedSettingService.AdvancedSetting.Accounts.Count == 0)
                this.NavigationService.Navigate("Initialize", args.Arguments);
            else
                this.NavigationService.Navigate("Main", args.Arguments);

            this.DeviceGestureService.GoBackRequested += (s, e) =>
            {
                var behavior = Flantter.MilkyWay.Views.Behaviors.ShowSettingsFlyoutAction.GetForCurrentView();
                if (behavior == null)
                    return;

                if (behavior.ShowingPopupCount == 0)
                    return;

                e.Handled = true;
                e.Cancel = true;
                behavior.HideTopPopup();
            };

            //return Task.FromResult<object>(null);
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            return base.OnInitializeAsync(args);
        }

        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs e)
        {
            base.OnShareTargetActivated(e);
            var shareTargetPage = new StatusShareContract();
            shareTargetPage.Activate(e);
        }

        protected async override void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);

            if (e is ToastNotificationActivatedEventArgs)
            {
                var args = e as ToastNotificationActivatedEventArgs;

                if (string.IsNullOrWhiteSpace(args.Argument))
                    return;

                var data = args.Argument.Split(new char[] { ',' });
                var type = data.ElementAt(0);
                var screenName = data.ElementAt(1);
                var targetScreenName = data.ElementAt(2);
                var tweet = args.UserInput["tweet"] as string;

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

                var account = accounts.First(x => x.ScreenName == screenName);
                var tokens = Tokens.Create(account.ConsumerKey, account.ConsumerSecret, account.AccessToken, account.AccessTokenSecret);
                if (type == "mention")
                {
                    var id = long.Parse(data.ElementAt(3));
                    await tokens.Statuses.UpdateAsync(status => "@" + targetScreenName + " " + tweet, in_reply_to_status_id => id);
                }
                else if (type == "dm")
                {
                    await tokens.DirectMessages.NewAsync(text => tweet, screen_name => targetScreenName);
                }
            }
            
        }
    }
}