using System.Threading.Tasks;
using Windows.UI.Xaml;
using Flantter.MilkyWay.Views.Contents.Authorize;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class AuthorizeAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            return ExecuteAsync((AuthorizeNotification) parameter);
        }

        private async Task ExecuteAsync(AuthorizeNotification authorizeNotification)
        {
            var authorizePopup = new AuthorizePopup();
            var account = await authorizePopup.ShowAsync();
            authorizeNotification.Result = account;
        }
    }

    public class AuthorizeNotification : Notification
    {
        public AccountInfo Result { get; set; }
    }
}