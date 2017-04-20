using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Util
{
    public class MessageDialogAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            return ExecuteAsync((MessageDialogNotification) parameter);
        }

        private async Task ExecuteAsync(MessageDialogNotification confirmMessageDialogNotification)
        {
            var msg = new MessageDialog(confirmMessageDialogNotification.Message,
                confirmMessageDialogNotification.Title);
            await msg.ShowAsync();
        }
    }

    public class MessageDialogNotification : Notification
    {
        /// <summary>
        ///     メッセージ
        /// </summary>
        public string Message { get; set; }
    }
}