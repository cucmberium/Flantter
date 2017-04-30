using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Flantter.MilkyWay.Views.Contents;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class TweetAreaSuggestionBehavior : DependencyObject, IBehavior
    {
        public static readonly DependencyProperty MessengerProperty =
            DependencyProperty.Register(
                "Messenger",
                typeof(Messenger),
                typeof(TweetAreaSuggestionBehavior),
                new PropertyMetadata(null, MessengerChanged));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(TweetAreaSuggestionPopup),
                new PropertyMetadata(null, CommandChanged));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(TweetAreaSuggestionPopup),
                new PropertyMetadata(null, CommandParameterChanged));

        public static readonly DependencyProperty IsTopAppBarProperty =
            DependencyProperty.Register(
                "IsTopAppBar",
                typeof(bool),
                typeof(AppBarShowBehavior),
                new PropertyMetadata(false));

        public TweetAreaSuggestionPopup SuggestionPopup;

        public TweetAreaSuggestionBehavior()
        {
            SuggestionPopup = new TweetAreaSuggestionPopup();
        }

        public Messenger Messenger
        {
            get => (Messenger) GetValue(MessengerProperty);
            set => SetValue(MessengerProperty, value);
        }

        public ICommand Command
        {
            get => (ICommand) GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public bool IsTopAppBar
        {
            get => (bool) GetValue(IsTopAppBarProperty);
            set => SetValue(IsTopAppBarProperty, value);
        }

        public DependencyObject AssociatedObject { get; set; }

        public void Attach(DependencyObject AssociatedObject)
        {
            this.AssociatedObject = AssociatedObject;

            ((ExtendedTextBox) this.AssociatedObject).PreKeyDown += TextBox_PreKeyDown;
        }

        public void Detach()
        {
            if (AssociatedObject != null)
                ((ExtendedTextBox) AssociatedObject).PreKeyDown -= TextBox_PreKeyDown;
        }

        private static void MessengerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Messengerプロパティに変更があったらイベントを購読する
            var self = (TweetAreaSuggestionBehavior) d;
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

        private static void CommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as TweetAreaSuggestionBehavior;
            if (behavior == null)
                return;

            behavior.SuggestionPopup.Command = (ICommand) e.NewValue;
        }

        private static void CommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as TweetAreaSuggestionBehavior;
            if (behavior == null)
                return;

            behavior.SuggestionPopup.CommandParameter = e.NewValue;
        }

        private void TextBox_PreKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (SuggestionPopup == null)
                return;

            if (!SuggestionPopup.Popup.IsOpen)
                return;

            switch (e.Key)
            {
                case VirtualKey.Up:
                    SuggestionPopup.IncrementSelectedIndex(true);
                    e.Handled = true;
                    break;
                case VirtualKey.Down:
                case VirtualKey.Tab:
                    SuggestionPopup.IncrementSelectedIndex();
                    e.Handled = true;
                    break;
                case VirtualKey.Enter:
                    SuggestionPopup.ListBoxItemSelect();
                    e.Handled = true;
                    break;
                case VirtualKey.Escape:
                    SuggestionPopup.Hide();
                    e.Handled = true;
                    break;
            }
        }

        private void MessengerRaised(object sender, MessengerEventArgs e)
        {
            var notification = e.Notification as SuggestionNotification;

            if (!notification.IsOpen || notification.SuggestWords == null || !notification.SuggestWords.Any())
            {
                SuggestionPopup.Hide();
                e.Callback();
                return;
            }

            if (((ExtendedTextBox) AssociatedObject).SelectionStart < 1)
            {
                SuggestionPopup.Hide();
                e.Callback();
                return;
            }

            SuggestionPopup.Items.Clear();
            foreach (var item in notification.SuggestWords)
                SuggestionPopup.Items.Add(new SuggestionItem {Text = item});

            var itemCount = notification.SuggestWords.Count();
            if (itemCount > 4)
                itemCount = 4;

            var suggestionRect =
                ((ExtendedTextBox) AssociatedObject).GetRectFromCharacterIndex(
                    ((ExtendedTextBox) AssociatedObject).SelectionStart - 1, true);
            var ttv = ((ExtendedTextBox) AssociatedObject).TransformToVisual(Window.Current.Content);
            var screenCoords = ttv.TransformPoint(new Point(0, 0));

            if (!SuggestionPopup.Popup.IsOpen)
            {
                if (IsTopAppBar)
                    SuggestionPopup.SetPosition(
                        suggestionRect.Top + screenCoords.Y + 1.8 * ((ExtendedTextBox) AssociatedObject).FontSize + 4,
                        suggestionRect.Left + screenCoords.X);
                else
                    SuggestionPopup.SetPosition(suggestionRect.Top + screenCoords.Y - itemCount * 40 - 6,
                        suggestionRect.Left + screenCoords.X);

                SuggestionPopup.Show();
                ((ExtendedTextBox) AssociatedObject).Focus(FocusState.Keyboard);
            }
            else
            {
                if (IsTopAppBar)
                    SuggestionPopup.SetPosition(
                        suggestionRect.Top + screenCoords.Y + 1.8 * ((ExtendedTextBox) AssociatedObject).FontSize + 4,
                        null);
                else
                    SuggestionPopup.SetPosition(suggestionRect.Top + screenCoords.Y - itemCount * 40 - 6, null);
            }

            SuggestionPopup.Show();

            e.Callback();
        }
    }

    public class SuggestionNotification : Notification
    {
        public IEnumerable<string> SuggestWords { get; set; }

        public bool IsOpen { get; set; }
    }
}