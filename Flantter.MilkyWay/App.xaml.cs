using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Behaviors;
using Flantter.MilkyWay.Views.Contents.ShareContract;
using Microsoft.HockeyApp;
using Newtonsoft.Json.Linq;
using Prism.Windows;

namespace Flantter.MilkyWay
{
    sealed partial class App : PrismApplication
    {
        public App()
        {
            HockeyClient.Current.Configure("12d0f9780e5645e3bf16ee0557054a03");
            InitializeComponent();

            UnhandledException += App_UnhandledException;
            Suspending += App_Suspending;
            Resuming += App_Resuming;
        }

        private void App_Suspending(object sender, SuspendingEventArgs e)
        {
            if (BackgroundTaskRegistration.AllTasks.Count > 1)
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                    task.Value.Unregister(true);

            SettingService.Setting.LatestNotificationDate = DateTimeOffset.Now;

            if (SettingService.Setting.TileNotification == SettingSupport.TileNotificationEnum.None &&
                !SettingService.Setting.BackgroundNotification)
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
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                    task.Value.Unregister(true);

            SettingService.Setting.LatestNotificationDate = DateTimeOffset.Now;
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var stacktrace = e.Exception.StackTrace; // 別変数に最初に入れないと次アクセスからNull , 一番最初のUnhandledExceptionじゃないとNull
            if (SettingService.Setting.PreventForcedTermination)
                e.Handled = true;

            Debug.WriteLine(stacktrace);
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
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                    task.Value.Unregister(true);

            await AdvancedSettingService.AdvancedSetting.LoadFromAppSettings();

            SettingService.Setting.LatestNotificationDate = DateTimeOffset.Now;

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

            //return Task.FromResult<object>(null);
        }

        protected override async void OnShareTargetActivated(ShareTargetActivatedEventArgs e)
        {
            base.OnShareTargetActivated(e);
            await AdvancedSettingService.AdvancedSetting.LoadFromAppSettings();
            var shareTargetPage = new StatusShareContract();
            shareTargetPage.Activate(e);
        }

        protected override async void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);

            if (e is ToastNotificationActivatedEventArgs)
            {
                var args = e as ToastNotificationActivatedEventArgs;

                if (string.IsNullOrWhiteSpace(args.Argument))
                    return;

                var data = args.Argument.Split(',');
                var type = data.ElementAt(0);
                var screenName = data.ElementAt(1);
                var targetScreenName = data.ElementAt(2);
                var tweet = args.UserInput["tweet"] as string;

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

                var account = accounts.First(x => x.ScreenName == screenName);
                var tokens = Tokens.Create(account.ConsumerKey, account.ConsumerSecret, account.AccessToken,
                    account.AccessTokenSecret, account.UserId, account.ScreenName, account.Instance);
                if (type == "mention")
                {
                    var id = long.Parse(data.ElementAt(3));
                    await tokens.Statuses.UpdateAsync(status => "@" + targetScreenName + " " + tweet,
                        in_reply_to_status_id => id);
                }
                else if (type == "dm")
                {
                    await tokens.DirectMessages.NewAsync(text => tweet, screen_name => targetScreenName);
                }
            }
        }
    }
}