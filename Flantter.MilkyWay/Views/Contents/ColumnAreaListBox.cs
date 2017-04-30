using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed class ColumnAreaListBox : ListBox, ISemanticZoomInformation
    {
        public ColumnAreaListBox()
        {
            DefaultStyleKey = typeof(ColumnAreaListBox);
        }

        bool ISemanticZoomInformation.IsActiveView { get; set; }

        bool ISemanticZoomInformation.IsZoomedInView { get; set; }

        SemanticZoom ISemanticZoomInformation.SemanticZoomOwner { get; set; }

        void ISemanticZoomInformation.InitializeViewChange()
        {
        }

        void ISemanticZoomInformation.CompleteViewChange()
        {
        }

        void ISemanticZoomInformation.MakeVisible(SemanticZoomLocation item)
        {
        }

        void ISemanticZoomInformation.StartViewChangeFrom(SemanticZoomLocation source, SemanticZoomLocation destination)
        {
        }

        void ISemanticZoomInformation.StartViewChangeTo(SemanticZoomLocation source, SemanticZoomLocation destination)
        {
        }

        void ISemanticZoomInformation.CompleteViewChangeFrom(SemanticZoomLocation source,
            SemanticZoomLocation destination)
        {
        }

        void ISemanticZoomInformation.CompleteViewChangeTo(SemanticZoomLocation source,
            SemanticZoomLocation destination)
        {
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var container = element as FrameworkElement;
            var binding = new Binding
            {
                Source = item,
                Path = new PropertyPath("Left.Value"),
                Mode = BindingMode.OneWay
            };
            container.SetBinding(Canvas.LeftProperty, binding);
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Left || e.Key == VirtualKey.Right ||
                e.Key == VirtualKey.Up || e.Key == VirtualKey.Down)
                e.Handled = true;

            base.OnKeyDown(e);
        }
    }
}