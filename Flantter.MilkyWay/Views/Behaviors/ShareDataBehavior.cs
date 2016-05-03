using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class ShareDataBehavior : DependencyObject, IBehavior
    {
        private DependencyObject _AssociatedObject;
        public DependencyObject AssociatedObject
        {
            get { return this._AssociatedObject; }
            set { this._AssociatedObject = value; }
        }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
        }

        public void Detach()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested -= DataTransferManager_DataRequested;
        }

        public ShareDataNotification LatestNotificationData { get; set; }

        public Messenger Messenger
        {
            get { return (Messenger)GetValue(MessengerProperty); }
            set { SetValue(MessengerProperty, value); }
        }

        public static readonly DependencyProperty MessengerProperty =
            DependencyProperty.Register(
                "Messenger",
                typeof(Messenger),
                typeof(ShareDataBehavior),
                new PropertyMetadata(null, MessengerChanged));

        private static void MessengerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Messengerプロパティに変更があったらイベントを購読する
            var self = (ShareDataBehavior)d;
            if (e.OldValue != null)
            {
                var m = (Messenger)e.OldValue;
                m.Raised -= self.MessengerRaised;
            }

            if (e.NewValue != null)
            {
                var m = (Messenger)e.NewValue;
                m.Raised += self.MessengerRaised;
            }
        }

        private void MessengerRaised(object sender, MessengerEventArgs e)
        {
            var notification = e.Notification as ShareDataNotification;
            this.LatestNotificationData = notification;
            DataTransferManager.ShowShareUI();
        }
        
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            //var deferral = args.Request.GetDeferral();

            args.Request.Data.SetWebLink(new Uri(this.LatestNotificationData.Url));
            args.Request.Data.SetText(this.LatestNotificationData.Text);

            args.Request.Data.Properties.ApplicationName = "Flantter";
            args.Request.Data.Properties.Title = this.LatestNotificationData.Title;
            args.Request.Data.Properties.Description = this.LatestNotificationData.Description;

            //deferral.Complete();
        }
    }

    public class ShareDataNotification : Notification
    {
        public string Description { get; set; }

        public string Text { get; set; }

        public string Url { get; set; }
    }
}
