using Flantter.MilkyWay.Views.Contents.Authorize;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class AuthorizeAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            return this.ExecuteAsync((AuthorizeNotification)parameter);
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
        /// <summary>
        /// メッセージ
        /// </summary>
        public AccountInfo Result { get; set; }
    }
}
