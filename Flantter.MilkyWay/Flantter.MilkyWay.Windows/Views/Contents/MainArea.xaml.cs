using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// ユーザー コントロールのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234236 を参照してください

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class MainArea : UserControl
    {
        public MainArea()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += Window_SizeChanged;

            if (Window.Current.Bounds.Width < 352)
                VisualStateManager.GoToState(this, "Under352px", true);
            else if (Window.Current.Bounds.Width < 384)
                VisualStateManager.GoToState(this, "Under384px", true);
            else if (Window.Current.Bounds.Width < 500)
                VisualStateManager.GoToState(this, "Under500px", true);
            else
                VisualStateManager.GoToState(this, "Default", true);
        }

        ~MainArea()
        {
            Window.Current.SizeChanged -= Window_SizeChanged;
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (e.Size.Width < 352)
                VisualStateManager.GoToState(this, "Under352px", true);
            else if (e.Size.Width < 384)
                VisualStateManager.GoToState(this, "Under384px", true);
            else if (e.Size.Width < 500)
                VisualStateManager.GoToState(this, "Under500px", true);
            else
                VisualStateManager.GoToState(this, "Default", true);
        }
    }
}
