using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Util
{
    [ContentProperty(Name = "Actions")]
    public class MessengerTriggerBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty MessengerProperty =
            DependencyProperty.Register(
                "Messenger",
                typeof(Messenger),
                typeof(MessengerTriggerBehavior),
                new PropertyMetadata(null, MessengerChanged));

        public static readonly DependencyProperty ActionsProperty =
            DependencyProperty.Register("Actions", typeof(ActionCollection), typeof(MessengerTriggerBehavior),
                new PropertyMetadata(null));

        public Messenger Messenger
        {
            get => (Messenger) GetValue(MessengerProperty);
            set => SetValue(MessengerProperty, value);
        }

        public ActionCollection Actions
        {
            get
            {
                var result = (ActionCollection) GetValue(ActionsProperty);
                if (result == null)
                    Actions = result = new ActionCollection();
                return result;
            }
            set => SetValue(ActionsProperty, value);
        }

        public DependencyObject AssociatedObject { get; private set; }

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;
        }

        public void Detach()
        {
            if (Messenger != null)
                Messenger.Raised -= MessengerRaised;
        }

        private static void MessengerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (MessengerTriggerBehavior) d;
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


        private async void MessengerRaised(object sender, MessengerEventArgs e)
        {
            await Task.WhenAll(Interaction.ExecuteActions(this, Actions, e.Notification).OfType<Task>());
            e.Callback();
        }
    }

    public class Messenger
    {
        public event EventHandler<MessengerEventArgs> Raised;

        public Task<T> Raise<T>(T n)
            where T : Notification
        {
            var source = new TaskCompletionSource<T>();
            var h = Raised;
            h?.Invoke(this, new MessengerEventArgs
            {
                Notification = n,
                Callback = () => source.SetResult(n)
            });
            return source.Task;
        }
    }

    public class MessengerEventArgs : EventArgs
    {
        public Notification Notification { get; set; }

        public Action Callback { get; set; }
    }

    public class Notification
    {
        public string Title { get; set; }
        public object Content { get; set; }
    }
}