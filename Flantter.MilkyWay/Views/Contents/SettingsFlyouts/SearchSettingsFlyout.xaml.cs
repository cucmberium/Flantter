using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Flantter.MilkyWay.Views.Contents.SettingsFlyouts
{
    public sealed partial class SearchSettingsFlyout : ExtendedSettingsFlyout
    {
        public SearchSettingsFlyoutViewModel ViewModel
        {
            get { return (SearchSettingsFlyoutViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(SearchSettingsFlyoutViewModel), typeof(SearchSettingsFlyout), null);

        public SearchSettingsFlyout()
        {
            this.InitializeComponent();
            this.SizeChanged += SearchSettingsFlyout_SizeChanged;
            SearchSettingsFlyout_SizeChanged(null, null);

            this.SearchSettingsFlyoutPivot.SelectionChanged += SearchSettingsFlyoutPivot_SelectionChanged;

            this.SearchSettingsFlyoutStatusSearchBox.FocusOnKeyboardInput = true;
        }

        private void SearchSettingsFlyoutPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SearchSettingsFlyoutPivot.SelectedIndex == 0)
            {

                this.SearchSettingsFlyoutUserSearchBox.FocusOnKeyboardInput = false;
                this.SearchSettingsFlyoutStatusSearchBox.FocusOnKeyboardInput = true;
            }
            else if (this.SearchSettingsFlyoutPivot.SelectedIndex == 1)
            {
                this.SearchSettingsFlyoutStatusSearchBox.FocusOnKeyboardInput = false;
                this.SearchSettingsFlyoutUserSearchBox.FocusOnKeyboardInput = true;
            }
            else
            {
                this.SearchSettingsFlyoutStatusSearchBox.FocusOnKeyboardInput = false;
                this.SearchSettingsFlyoutUserSearchBox.FocusOnKeyboardInput = false;
            }
        }

        private void SearchSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;
            
            this.Width = width;

            this.SearchSettingsFlyoutGrid.Width = width;
            this.SearchSettingsFlyoutGrid.Height = Window.Current.Bounds.Height - 70;
        }
    }
}
