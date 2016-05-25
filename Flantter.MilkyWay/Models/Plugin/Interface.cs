using Flantter.MilkyWay.Setting;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

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

    public static class Utility
    {
    }

    public static class Notification
    {
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
    }
}
