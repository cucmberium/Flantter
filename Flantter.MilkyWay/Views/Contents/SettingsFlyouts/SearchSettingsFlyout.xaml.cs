using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.ViewModels.SettingsFlyouts;
using Flantter.MilkyWay.Views.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        }

        private void SearchSettingsFlyout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = Window.Current.Bounds.Width;

            if (width < 320)
                width = 320;
            else if (width >= 400)
                width = 400;
            
            this.Width = width;

            this.SearchSettingsFlyoutPivot.Width = width;
        }

        public async void FocusToStatusSearchBox()
        {
            await Task.Delay(50);
            this.SearchSettingsFlyoutStatusSearchBox.Focus(FocusState.Keyboard);
        }
    }
}
