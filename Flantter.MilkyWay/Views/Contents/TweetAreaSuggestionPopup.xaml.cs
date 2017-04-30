using System.Collections.ObjectModel;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class TweetAreaSuggestionPopup : UserControl
    {
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<SuggestionItem>),
                typeof(TweetAreaSuggestionPopup), null);

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(TweetAreaSuggestionPopup),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(TweetAreaSuggestionPopup),
                new PropertyMetadata(null));

        public Popup Popup;

        public TweetAreaSuggestionPopup()
        {
            InitializeComponent();
            Items = new ObservableCollection<SuggestionItem>();

            Popup = new Popup
            {
                Child = this,
                IsLightDismissEnabled = false,
                Opacity = 1
            };

            ListBox.AddHandler(KeyDownEvent, new KeyEventHandler(ListBox_KeyDown), true);
        }

        public ObservableCollection<SuggestionItem> Items
        {
            get => (ObservableCollection<SuggestionItem>) GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
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

        public void Show()
        {
            Popup.IsOpen = true;
        }

        public void Hide()
        {
            Popup.IsOpen = false;
        }

        public void SetPosition(double? top, double? left)
        {
            if (top.HasValue)
                Canvas.SetTop(Popup, top.Value);

            if (left.HasValue)
                Canvas.SetLeft(Popup, left.Value);
        }

        public void IncrementSelectedIndex(bool decrement = false)
        {
            if (decrement)
            {
                if (ListBox.SelectedIndex > 0)
                    ListBox.SelectedIndex -= 1;
                else
                    ListBox.SelectedIndex = ListBox.Items.Count - 1;
            }
            else
            {
                if (ListBox.SelectedIndex < ListBox.Items.Count - 1)
                    ListBox.SelectedIndex += 1;
                else
                    ListBox.SelectedIndex = 0;
            }

            ListBox.ScrollIntoView(ListBox.Items[ListBox.SelectedIndex]);
        }

        public void ListBoxItemSelect()
        {
            if (ListBox.SelectedIndex != -1)
                Command?.Execute(Items[ListBox.SelectedIndex].Text);

            Hide();
        }

        private void ListBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Escape:
                    Hide();
                    break;
                case VirtualKey.Enter:
                    ListBoxItemSelect();
                    break;
            }
        }

        private void ListBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ListBoxItemSelect();
        }
    }

    public class SuggestionItem
    {
        public string Text { get; set; }
    }
}