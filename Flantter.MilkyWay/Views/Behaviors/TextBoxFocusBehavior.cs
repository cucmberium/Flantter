using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class TextBoxFocusBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty MessengerProperty =
            DependencyProperty.Register(
                "Messenger",
                typeof(Messenger),
                typeof(TextBoxFocusBehavior),
                new PropertyMetadata(null, MessengerChanged));

        public Messenger Messenger
        {
            get => (Messenger) GetValue(MessengerProperty);
            set => SetValue(MessengerProperty, value);
        }

        public DependencyObject AssociatedObject { get; set; }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;
        }

        public void Detach()
        {
        }

        private static void MessengerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Messengerプロパティに変更があったらイベントを購読する
            var self = (TextBoxFocusBehavior) d;
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
            var textBox = AssociatedObject as TextBox;
            if (textBox == null)
                return;

            textBox.Focus(FocusState.Programmatic);
            e.Callback();
        }
    }
}