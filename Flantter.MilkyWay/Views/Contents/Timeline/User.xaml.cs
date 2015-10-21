using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Flantter.MilkyWay.Views.Contents.Timeline
{
    public sealed partial class User : UserControl, IRecycleItem
    {
        public void ResetItem()
        {
            if (CommandGridLoaded)
            {
                this.CommandGrid.Visibility = Visibility.Collapsed;
                this.CommandGrid.Height = 0;
            }

            SetIsSelected(this, false);
        }

        public UserViewModel ViewModel
        {
            get { return (UserViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(UserViewModel), typeof(User), null);

        public static bool GetIsSelected(DependencyObject obj) { return (bool)obj.GetValue(IsSelectedProperty); }
        public static void SetIsSelected(DependencyObject obj, bool value) { obj.SetValue(IsSelectedProperty, value); }
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(User), new PropertyMetadata(false, IsSelectedPropertyChanged));
        
        public User()
        {
            this.InitializeComponent();
            this.Loaded += (s, e) =>
            {
                SelectorItem selector = null;
                DependencyObject dp = this;
                while ((dp = VisualTreeHelper.GetParent(dp)) != null)
                {
                    var i = dp as SelectorItem;
                    if (i != null) { selector = i; break; }
                }

                this.SetBinding(IsSelectedProperty, new Binding
                {
                    Path = new PropertyPath("IsSelected"),
                    Source = selector,
                    Mode = BindingMode.TwoWay
                });
            };
        }

        private static void IsSelectedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            CommandGrid_PropertyChanged(obj, e);
        }

        #region CommandGrid 関連
        public bool CommandGridLoaded = false;
        private static void CommandGrid_PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var status = obj as User;
            var grid = status.FindName("CommandGrid") as Grid;

            if (grid == null)
                return;

            status.CommandGridLoaded = true;

            if ((bool)e.NewValue)
                (grid.Resources["TweetCommandBarOpenAnimation"] as Storyboard).Begin();
            else
                (grid.Resources["TweetCommandBarCloseAnimation"] as Storyboard).Begin();
        }
        #endregion
    }
}
