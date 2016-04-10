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
    public class ConfirmMessageDialogAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            return this.ExecuteAsync((ConfirmMessageDialogNotification)parameter);
        }

        private async Task ExecuteAsync(ConfirmMessageDialogNotification confirmMessageDialogNotification)
        {
            bool result = false;
            Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog(confirmMessageDialogNotification.Message, confirmMessageDialogNotification.Title);
            msg.Commands.Add(new Windows.UI.Popups.UICommand("Yes", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = true; })));
            msg.Commands.Add(new Windows.UI.Popups.UICommand("No", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = false; })));
            await msg.ShowAsync();

            confirmMessageDialogNotification.Result = result;
        }
    }

    public class ConfirmMessageDialogNotification : Notification
    {
        /// <summary>
        /// メッセージ
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 結果
        /// </summary>
        public bool Result { get; set; }
    }
}
