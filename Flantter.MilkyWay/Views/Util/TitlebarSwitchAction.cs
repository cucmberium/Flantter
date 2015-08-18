using Microsoft.Xaml.Interactivity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;

namespace Flantter.MilkyWay.Views.Util
{
    public class TitleBarSwitchAction : DependencyObject, IAction
    {
        public static readonly DependencyProperty TitleBarVisibilityProperty
        = DependencyProperty.Register("TitleBarVisibility", typeof(bool), typeof(TitleBarSwitchAction), null);

        public bool TitleBarVisibility
        {
            get { return (bool)this.GetValue(TitleBarVisibilityProperty); }
            set { this.SetValue(TitleBarVisibilityProperty, value); }
        }

        public object Execute(object sender, object parameter)
        {
            var page = sender as Page;
            if (page == null)
                return 0;

            var coreApplicationView = CoreApplication.GetCurrentView();
            
            if (this.TitleBarVisibility)
            {
                coreApplicationView.TitleBar.ExtendViewIntoTitleBar = true;
                VisualStateManager.GoToState(page, "TitleBarOpened", true);
                Window.Current.SetTitleBar(page.FindName("Flantter_TitleBar") as UIElement);
            }
            else
            {
                coreApplicationView.TitleBar.ExtendViewIntoTitleBar = false;
                VisualStateManager.GoToState(page, "TitleBarClosed", true);
            }

            return 1;
        }
    }
}
