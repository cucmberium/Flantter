using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class SymbolIconForegroundBrushSwitchBehavior
    {
        public static bool GetIsEnabled(DependencyObject obj) { return (bool)obj.GetValue(IsEnabledProperty); }
        public static void SetIsEnabled(DependencyObject obj, bool value) { obj.SetValue(IsEnabledProperty, value); }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(SymbolIconForegroundBrushSwitchBehavior), new PropertyMetadata(false, PropertyChanged));

        public static object GetEnabledBrush(DependencyObject obj) { return obj.GetValue(EnabledBrushProperty); }
        public static void SetEnabledBrush(DependencyObject obj, object value) { obj.SetValue(EnabledBrushProperty, value); }

        public static readonly DependencyProperty EnabledBrushProperty =
            DependencyProperty.Register("EnabledBrush", typeof(object), typeof(SymbolIconForegroundBrushSwitchBehavior), new PropertyMetadata(false, PropertyChanged));

        public static object GetDisabledBrush(DependencyObject obj) { return obj.GetValue(DisabledBrushProperty); }
        public static void SetDisabledBrush(DependencyObject obj, object value) { obj.SetValue(DisabledBrushProperty, value); }

        public static readonly DependencyProperty DisabledBrushProperty =
            DependencyProperty.Register("DisabledBrush", typeof(object), typeof(SymbolIconForegroundBrushSwitchBehavior), new PropertyMetadata(false, PropertyChanged));

        private static void PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var symbol = obj as SymbolIcon;

            if (symbol == null)
                return;

            if (GetIsEnabled(obj))
                symbol.Foreground = GetEnabledBrush(obj) as SolidColorBrush;
            else
                symbol.Foreground = GetDisabledBrush(obj) as SolidColorBrush;
        }
    }
}
