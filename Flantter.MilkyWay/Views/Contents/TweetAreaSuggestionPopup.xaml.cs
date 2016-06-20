using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class TweetAreaSuggestionPopup : UserControl
    {
        public TweetAreaSuggestionPopup()
        {
            this.InitializeComponent();
            this.Items = new ObservableCollection<SuggestionItem>();

            this._Popup = new Popup
            {
                Child = this,
                IsLightDismissEnabled = false,
                Opacity = 1
            };

            this.ListBox.AddHandler(ListBox.KeyDownEvent, new KeyEventHandler(ListBox_KeyDown), true);
        }

        public Popup _Popup;

        public ObservableCollection<SuggestionItem> Items
        {
            get { return (ObservableCollection<SuggestionItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<SuggestionItem>), typeof(TweetAreaSuggestionPopup), null);

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(TweetAreaSuggestionPopup), new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return (object)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(TweetAreaSuggestionPopup), new PropertyMetadata(null));

        public void Show()
        {
            this._Popup.IsOpen = true;
        }

        public void Hide()
        {
            this._Popup.IsOpen = false;
        }

        public void SetPosition(double? top, double? left)
        {
            if (top.HasValue)
                Canvas.SetTop(this._Popup, top.Value);

            if (left.HasValue)
                Canvas.SetLeft(this._Popup, left.Value);
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
            if (ListBox.SelectedIndex != -1 && Command != null)
            {
                this.Command.Execute(Items[ListBox.SelectedIndex].Text);
                /*if (this.CommandParameter == null)
                    this.Command.Execute(Items[ListBox.SelectedIndex].Text);
                else
                    this.Command.Execute(CommandParameter);*/
            }

            this.Hide();
        }

        private void ListBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case VirtualKey.Escape:
                    this.Hide();
                    break;
                case VirtualKey.Enter:
                    this.ListBoxItemSelect();
                    break;
            }
        }

        private void ListBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.ListBoxItemSelect();
        }
    }

    public class SuggestionItem
    {
        public string Text { get; set; }
    }
}
