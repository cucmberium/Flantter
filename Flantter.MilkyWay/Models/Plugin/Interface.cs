using Flantter.MilkyWay.Setting;
using Jint;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.Plugin
{
    public static class Debug
    {
        public static void Log(object log)
        {
            System.Diagnostics.Debug.WriteLine(log);
        }
    }

    public static class Filter
    {
        public static void RegisterFunction(string functionName, int argumentCount, Delegate dele)
        {
            Flantter.MilkyWay.Models.Filter.FilterFunctions.Register(functionName, argumentCount, dele);
        }

        public static void UnregisterFunction(string functionName)
        {
            Flantter.MilkyWay.Models.Filter.FilterFunctions.Unregister(functionName);
        }
    }

    public static class Event
    {
        public class FlEventArgs : EventArgs
        {
            public object Info;
        }

        private static Dictionary<string, IDisposable> EventStore = new Dictionary<string, IDisposable>();
        private static event EventHandler<FlEventArgs> TweetReceivedAtColumn;

        public static void RegisterFunction(string eventName, Delegate dele)
        {
            if (EventStore.ContainsKey(eventName + dele.ToString()))
                return;
            
            try
            {
                var eventInfo = typeof(Event).GetEvent(eventName, BindingFlags.Static | BindingFlags.NonPublic);
                var iDisposable = Observable.FromEvent<FlEventArgs>(
                        x => eventInfo.AddEventHandler(null, x),
                        x => eventInfo.RemoveEventHandler(null, x)
                    ).SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x => dele.DynamicInvoke(x));
                EventStore[eventName + dele.ToString()] = iDisposable;
            }
            catch
            {
            }
        }

        public static void UnregisterFunction(string eventName, Delegate dele)
        {
            if (!EventStore.ContainsKey(eventName + dele.ToString()))
                return;

            try
            {
                var iDisposable = EventStore[eventName + dele.ToString()];
                EventStore.Remove(eventName + dele.ToString());
                iDisposable.Dispose();
            }
            catch
            {
            }
        }
    }

    public static class Utility
    {
        public static void CopyToClipboard(string str)
        {
            throw new NotImplementedException();
        }

        public static string GetActiveUserScreenName()
        {
            throw new NotImplementedException();
        }

        public static long GetActiveUserUserId()
        {
            throw new NotImplementedException();
        }

        public static string GetUserScreenNameList()
        {
            throw new NotImplementedException();
        }

        public static long GetUserIdList()
        {
            throw new NotImplementedException();
        }

        public static void PlaySound(string path)
        {
            throw new NotImplementedException();
        }

        public static void PopupToastNotification(string text, string imageUrl = "")
        {
            var toastContent = new ToastContent();
            toastContent.Visual = new ToastVisual();
            toastContent.Visual.BodyTextLine1 = new ToastText() { Text = text };

            if (!string.IsNullOrWhiteSpace(imageUrl))
                toastContent.Visual.AppLogoOverride = new ToastAppLogo() { Source = new ToastImageSource(imageUrl) };

            if (!SettingService.Setting.NotificationSound)
                toastContent.Audio = new ToastAudio() { Silent = true };

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public static void RunInUIThread(Delegate func)
        {
            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                func.DynamicInvoke();
            else
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => func.DynamicInvoke()).AsTask().Wait();
        }

        public static void AddMuteUser(string screenname)
        {
            throw new NotImplementedException();
        }

        public static void DeleteMuteUser(string screenname)
        {
            throw new NotImplementedException();
        }

        public static void AddMuteClient(string screenname)
        {
            throw new NotImplementedException();
        }

        public static void DeleteMuteClient(string screenname)
        {
            throw new NotImplementedException();
        }

        public static void ShowMessageBox(string message, string title)
        {
            throw new NotImplementedException();
        }

        public static bool ShowMessageBoxYesNo(string message, string title)
        {
            throw new NotImplementedException();
        }

        public static WebViewColumn AddWebviewColumn(string screenname)
        {
            throw new NotImplementedException();
        }

        public static void DeleteWebviewColumn(WebViewColumn column)
        {
            throw new NotImplementedException();
        }

        public class WebViewColumn
        {
            public WebView WebView { get; set; }
        }
    }
}
