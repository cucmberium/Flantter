using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class SymbolIconColorSwitchBehavior
	{
        public static bool GetIsSwitched(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsSwitchedProperty);
        }
        public static void SetIsSwitched(DependencyObject obj, bool value)
        {
            obj.SetValue(IsSwitchedProperty, value);
        }
        public static SolidColorBrush GetDefaultSymbolIconForegroundBrush(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(DefaultSymbolIconForegroundBrushProperty);
        }
        public static void SetDefaultSymbolIconForegroundBrush(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(DefaultSymbolIconForegroundBrushProperty, value);
        }
        public static SolidColorBrush GetCustomSymbolIconForegroundBrush(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(CustomSymbolIconForegroundBrushProperty);
        }
        public static void SetCustomSymbolIconForegroundBrush(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(CustomSymbolIconForegroundBrushProperty, value);
        }

        public static readonly DependencyProperty IsSwitchedProperty =
            DependencyProperty.RegisterAttached("IsSwitched", typeof(bool), typeof(SymbolIconColorSwitchBehavior), new PropertyMetadata(false, PropertyChanged));
        public static readonly DependencyProperty DefaultSymbolIconForegroundBrushProperty =
            DependencyProperty.RegisterAttached("DefaultSymbolIconForegroundBrush", typeof(SolidColorBrush), typeof(SymbolIconColorSwitchBehavior), new PropertyMetadata(new SolidColorBrush(), PropertyChanged));
        public static readonly DependencyProperty CustomSymbolIconForegroundBrushProperty =
            DependencyProperty.RegisterAttached("CustomSymbolIconForegroundBrush", typeof(SolidColorBrush), typeof(SymbolIconColorSwitchBehavior), new PropertyMetadata(new SolidColorBrush(), PropertyChanged));

        public static void PropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var switched = GetIsSwitched(obj);
            var symbolicon = obj as SymbolIcon;
            
            if (symbolicon == null)
                return;

            if (switched)
                symbolicon.Foreground = GetCustomSymbolIconForegroundBrush(obj);
            else
                symbolicon.Foreground = GetDefaultSymbolIconForegroundBrush(obj);
        }
    }
}
