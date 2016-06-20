using Flantter.MilkyWay.Views.Contents;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class TweetAreaSuggestionBehavior : DependencyObject, IBehavior
    {
        public TweetAreaSuggestionBehavior()
        {
            this.SuggestionPopup = new TweetAreaSuggestionPopup();
        }

        public TweetAreaSuggestionPopup SuggestionPopup;

        private DependencyObject _AssociatedObject;
        public DependencyObject AssociatedObject
        {
            get { return this._AssociatedObject; }
            set { this._AssociatedObject = value; }
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
                typeof(TweetAreaSuggestionBehavior),
                new PropertyMetadata(null, MessengerChanged));

        private static void MessengerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Messengerプロパティに変更があったらイベントを購読する
            var self = (TweetAreaSuggestionBehavior)d;
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

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(TweetAreaSuggestionPopup), new PropertyMetadata(null, CommandChanged));
        
        public object CommandParameter
        {
            get { return (object)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(TweetAreaSuggestionPopup), new PropertyMetadata(null, CommandParameterChanged));

        private static void CommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as TweetAreaSuggestionBehavior;
            if (behavior == null)
                return;

            behavior.SuggestionPopup.Command = (ICommand)e.NewValue;
        }

        private static void CommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as TweetAreaSuggestionBehavior;
            if (behavior == null)
                return;

            behavior.SuggestionPopup.CommandParameter = (object)e.NewValue;
        }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            ((ExtendedTextBox)this.AssociatedObject).PreKeyDown += TextBox_PreKeyDown;
        }

        public void Detach()
        {
            if (this.AssociatedObject != null)
            {
                ((ExtendedTextBox)this.AssociatedObject).PreKeyDown -= TextBox_PreKeyDown;
            }
        }

        private void TextBox_PreKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (this.SuggestionPopup == null)
                return;

            if (!this.SuggestionPopup._Popup.IsOpen)
                return;

            switch (e.Key)
            {
                case VirtualKey.Up:
                    this.SuggestionPopup.IncrementSelectedIndex(true);
                    e.Handled = true;
                    break;
                case VirtualKey.Down:
                case VirtualKey.Tab:
                    this.SuggestionPopup.IncrementSelectedIndex();
                    e.Handled = true;
                    break;
                case VirtualKey.Enter:
                    this.SuggestionPopup.ListBoxItemSelect();
                    e.Handled = true;
                    break;
                case VirtualKey.Escape:
                    this.SuggestionPopup.Hide();
                    e.Handled = true;
                    break;
            }
        }

        private void MessengerRaised(object sender, MessengerEventArgs e)
        {
            var notification = e.Notification as SuggestionNotification;
            
            if (!notification.IsOpen || notification.SuggestWords == null || notification.SuggestWords.Count() == 0)
            {
                this.SuggestionPopup.Hide();
                e.Callback();
                return;
            }

            if (((ExtendedTextBox)this.AssociatedObject).SelectionStart < 1)
            {
                this.SuggestionPopup.Hide();
                e.Callback();
                return;
            }

            this.SuggestionPopup.Items.Clear();
            foreach (var item in notification.SuggestWords)
                this.SuggestionPopup.Items.Add(new SuggestionItem() { Text = item });

            var itemCount = notification.SuggestWords.Count();
            if (itemCount > 4)
                itemCount = 4;

            var suggestionRect = ((ExtendedTextBox)this.AssociatedObject).GetRectFromCharacterIndex(((ExtendedTextBox)this.AssociatedObject).SelectionStart - 1, true);
            var ttv = ((ExtendedTextBox)this.AssociatedObject).TransformToVisual(Window.Current.Content);
            var screenCoords = ttv.TransformPoint(new Point(0, 0));

            if (!this.SuggestionPopup._Popup.IsOpen)
            {
                this.SuggestionPopup.SetPosition(suggestionRect.Top + screenCoords.Y - (itemCount * 40) - 6, suggestionRect.Left + screenCoords.X);
                this.SuggestionPopup.Show();
                ((ExtendedTextBox)this.AssociatedObject).Focus(FocusState.Keyboard);
            }
            else
            {
                this.SuggestionPopup.SetPosition(suggestionRect.Top + screenCoords.Y - (itemCount * 40) - 6, null);
            }

            this.SuggestionPopup.Show();

            e.Callback();
        }
    }

    public class SuggestionNotification : Notification
    {
        public IEnumerable<string> SuggestWords { get; set; }
        
        public bool IsOpen { get; set; }
    }
}
