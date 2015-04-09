using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Flantter.MilkyWay.Common;

namespace Flantter.MilkyWay.Views.Controls
{
    public class ExtendedCanvas : Canvas, IScrollSnapPointsInfo
    {
        public double SnapPointsSpaceing
        {
            get { return (double)this.GetValue(SnapPointsSpaceingProperty); }
            set { this.SetValue(SnapPointsSpaceingProperty, value); }
        }
        public static readonly DependencyProperty SnapPointsSpaceingProperty =
                        DependencyProperty.RegisterAttached("SnapPointsSpaceing", typeof(double),
                        typeof(ExtendedCanvas), new PropertyMetadata(0.0, SnapPointsSpaceing_Changed));

        private static void SnapPointsSpaceing_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var extendedCanvas = d as ExtendedCanvas;

            extendedCanvas.HorizontalSnapPointsChanged_OnCommandExecute();
            extendedCanvas.VerticalSnapPointsChanged_OnCommandExecute();
        }

        public double MaxSnapPoint
        {
            get { return (double)this.GetValue(MaxSnapPointProperty); }
            set { this.SetValue(MaxSnapPointProperty, value); }
        }
        public static readonly DependencyProperty MaxSnapPointProperty =
                        DependencyProperty.RegisterAttached("MaxSnapPoint", typeof(double),
                        typeof(ExtendedCanvas), new PropertyMetadata(0.0, MaxSnapPoint_Changed));

        private static void MaxSnapPoint_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var extendedCanvas = d as ExtendedCanvas;

            extendedCanvas.HorizontalSnapPointsChanged_OnCommandExecute();
            extendedCanvas.VerticalSnapPointsChanged_OnCommandExecute();
        }

        public bool AreHorizontalSnapPointsRegular
        {
            get { return false; }
        }

        public bool AreVerticalSnapPointsRegular
        {
            get { return false; }
        }

        public IReadOnlyList<float> GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment)
        {
            var snapPointsList = new List<float>();

            if (SnapPointsSpaceing <= 0.0 || MaxSnapPoint <= 0.0)
                return snapPointsList;

            for (var i = 0.0; i <= MaxSnapPoint; i += SnapPointsSpaceing)
                snapPointsList.Add((float)i);

            return snapPointsList;
        }

        public float GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment alignment, out float offset)
        {
            offset = 0.0F;
            return (float)SnapPointsSpaceing;
        }

        public event EventHandler<object> HorizontalSnapPointsChanged;
        public event EventHandler<object> VerticalSnapPointsChanged;

        public void HorizontalSnapPointsChanged_OnCommandExecute()
        {
            if (HorizontalSnapPointsChanged != null)
                HorizontalSnapPointsChanged(this, EventArgs.Empty);
        }

        public void VerticalSnapPointsChanged_OnCommandExecute()
        {
            if (VerticalSnapPointsChanged != null)
                VerticalSnapPointsChanged(this, EventArgs.Empty);
        }

        public ExtendedCanvas()
        {
            this.PointerWheelChanged += ExtendedCanvas_PointerWheelChanged;
        }

        ~ExtendedCanvas()
        {
            this.PointerWheelChanged -= ExtendedCanvas_PointerWheelChanged;
        }
            
        private void ExtendedCanvas_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            //e.Handled = true;
        }
    }
}
