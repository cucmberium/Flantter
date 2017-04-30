using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Util
{
    public class ConfirmMessageDialogAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            return ExecuteAsync((ConfirmMessageDialogNotification) parameter);
        }

        private async Task ExecuteAsync(ConfirmMessageDialogNotification confirmMessageDialogNotification)
        {
            var result = false;
            var msg = new MessageDialog(confirmMessageDialogNotification.Message,
                confirmMessageDialogNotification.Title);
            msg.Commands.Add(new UICommand("Yes", _ => { result = true; }));
            msg.Commands.Add(new UICommand("No", _ => { result = false; }));
            await msg.ShowAsync();

            confirmMessageDialogNotification.Result = result;
        }
    }

    public class ConfirmMessageDialogNotification : Notification
    {
        public string Message { get; set; }

        public bool Result { get; set; }
    }
}