using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;

namespace Flantter.MilkyWay.Views.Util
{
    public class MessageDialogAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            return this.ExecuteAsync((MessageDialogNotification)parameter);
        }

        private async Task ExecuteAsync(MessageDialogNotification confirmMessageDialogNotification)
        {
            Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog(confirmMessageDialogNotification.Message, confirmMessageDialogNotification.Title);
            await msg.ShowAsync();
        }
    }

    public class MessageDialogNotification : Notification
    {
        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; }
    }
}
