using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class ShareDataBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty MessengerProperty =
            DependencyProperty.Register(
                "Messenger",
                typeof(Messenger),
                typeof(ShareDataBehavior),
                new PropertyMetadata(null, MessengerChanged));

        public ShareDataNotification LatestNotificationData { get; set; }

        public Messenger Messenger
        {
            get => (Messenger) GetValue(MessengerProperty);
            set => SetValue(MessengerProperty, value);
        }

        public DependencyObject AssociatedObject { get; set; }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
        }

        public void Detach()
        {
            var dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested -= DataTransferManager_DataRequested;
        }

        private static void MessengerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Messengerプロパティに変更があったらイベントを購読する
            var self = (ShareDataBehavior) d;
            if (e.OldValue != null)
            {
                var m = (Messenger) e.OldValue;
                m.Raised -= self.MessengerRaised;
            }

            if (e.NewValue != null)
            {
                var m = (Messenger) e.NewValue;
                m.Raised += self.MessengerRaised;
            }
        }

        private void MessengerRaised(object sender, MessengerEventArgs e)
        {
            var notification = e.Notification as ShareDataNotification;
            LatestNotificationData = notification;
            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            //var deferral = args.Request.GetDeferral();
            if (!string.IsNullOrWhiteSpace(LatestNotificationData.Url))
                args.Request.Data.SetWebLink(new Uri(LatestNotificationData.Url));
            args.Request.Data.SetText(LatestNotificationData.Text);

            args.Request.Data.Properties.ApplicationName = "Flantter";
            args.Request.Data.Properties.Title = LatestNotificationData.Title;
            args.Request.Data.Properties.Description = LatestNotificationData.Description;

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