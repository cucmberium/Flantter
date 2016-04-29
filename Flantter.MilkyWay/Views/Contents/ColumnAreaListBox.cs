using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
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
            this.DefaultStyleKey = typeof(ColumnAreaListBox);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var container = element as FrameworkElement;
            var binding = new Windows.UI.Xaml.Data.Binding
            {
                Source = item,
                Path = new PropertyPath("Left.Value"),
                Mode = BindingMode.OneWay
            };

            container.SetBinding(Canvas.LeftProperty, binding);
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Left || e.Key == Windows.System.VirtualKey.Right || 
                e.Key == Windows.System.VirtualKey.Up || e.Key == Windows.System.VirtualKey.Down)
                e.Handled = true;

            base.OnKeyDown(e);
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

        void ISemanticZoomInformation.CompleteViewChangeFrom(SemanticZoomLocation source, SemanticZoomLocation destination)
        {
        }

        void ISemanticZoomInformation.CompleteViewChangeTo(SemanticZoomLocation source, SemanticZoomLocation destination)
        {
        }
    }
}
