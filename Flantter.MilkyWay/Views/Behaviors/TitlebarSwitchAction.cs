using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class TitleBarSwitchAction : DependencyObject, IAction
    {
        public static readonly DependencyProperty TitleBarVisibilityProperty
            = DependencyProperty.Register("TitleBarVisibility", typeof(bool), typeof(TitleBarSwitchAction), null);

        public bool TitleBarVisibility
        {
            get => (bool) GetValue(TitleBarVisibilityProperty);
            set => SetValue(TitleBarVisibilityProperty, value);
        }

        public object Execute(object sender, object parameter)
        {
            var page = sender as Page;
            if (page == null)
                return 0;

            var coreApplicationView = CoreApplication.GetCurrentView();

            if (TitleBarVisibility)
            {
                coreApplicationView.TitleBar.ExtendViewIntoTitleBar = true;
                VisualStateManager.GoToState(page, "TitleBarOpened", true);
                Window.Current.SetTitleBar(page.FindName("FlantterTitleBarMain") as UIElement);
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