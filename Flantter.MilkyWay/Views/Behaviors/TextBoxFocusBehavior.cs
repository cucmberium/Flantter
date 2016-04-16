using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class TextBoxFocusBehavior : DependencyObject, IBehavior
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
        }

        public void Detach()
        {
        }

        public Messenger Messenger
        {
            get { return (Messenger)GetValue(MessengerProperty); }
            set { SetValue(MessengerProperty, value); }
        }

        public static readonly DependencyProperty MessengerProperty =
            DependencyProperty.Register(
                "Messenger",
                typeof(Messenger),
                typeof(TextBoxFocusBehavior),
                new PropertyMetadata(null, MessengerChanged));

        private static void MessengerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Messengerプロパティに変更があったらイベントを購読する
            var self = (TextBoxFocusBehavior)d;
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
            var notification = e.Notification as Notification;
            var textBox = this.AssociatedObject as TextBox;
            if (textBox == null)
                return;

            textBox.Focus(FocusState.Programmatic);
            e.Callback();
        }
    }
}
