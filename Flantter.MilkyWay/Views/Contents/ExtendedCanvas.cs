using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Flantter.MilkyWay.Views.Contents
{
    public class ExtendedCanvas : Canvas, IScrollSnapPointsInfo
    {
        public static readonly DependencyProperty SnapPointsSpaceingProperty =
            DependencyProperty.RegisterAttached("SnapPointsSpaceing", typeof(double),
                typeof(ExtendedCanvas), new PropertyMetadata(0.0, SnapPointsSpaceing_Changed));

        public static readonly DependencyProperty MaxSnapPointProperty =
            DependencyProperty.RegisterAttached("MaxSnapPoint", typeof(double),
                typeof(ExtendedCanvas), new PropertyMetadata(0.0, MaxSnapPoint_Changed));

        public static readonly DependencyProperty MinSnapPointProperty =
            DependencyProperty.RegisterAttached("MinSnapPoint", typeof(double),
                typeof(ExtendedCanvas), new PropertyMetadata(0.0, MinSnapPoint_Changed));

        public double SnapPointsSpaceing
        {
            get => (double) GetValue(SnapPointsSpaceingProperty);
            set => SetValue(SnapPointsSpaceingProperty, value);
        }

        public double MaxSnapPoint
        {
            get => (double) GetValue(MaxSnapPointProperty);
            set => SetValue(MaxSnapPointProperty, value);
        }

        public double MinSnapPoint
        {
            get => (double) GetValue(MinSnapPointProperty);
            set => SetValue(MinSnapPointProperty, value);
        }

        public bool AreHorizontalSnapPointsRegular => false;

        public bool AreVerticalSnapPointsRegular => false;

        public IReadOnlyList<float> GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment)
        {
            var snapPointsList = new List<float>();

            if (SnapPointsSpaceing <= 0.0 || MaxSnapPoint - MinSnapPoint <= 0.0)
                return snapPointsList;

            for (var i = MinSnapPoint; i <= MaxSnapPoint; i += SnapPointsSpaceing)
                snapPointsList.Add((float) i);

            return snapPointsList;
        }

        public float GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment alignment, out float offset)
        {
            offset = 0.0F;
            return (float) SnapPointsSpaceing;
        }

        public event EventHandler<object> HorizontalSnapPointsChanged;
        public event EventHandler<object> VerticalSnapPointsChanged;

        private static void SnapPointsSpaceing_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var extendedCanvas = d as ExtendedCanvas;

            extendedCanvas.HorizontalSnapPointsChanged_OnCommandExecute();
            extendedCanvas.VerticalSnapPointsChanged_OnCommandExecute();
        }

        private static void MaxSnapPoint_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var extendedCanvas = d as ExtendedCanvas;

            extendedCanvas.HorizontalSnapPointsChanged_OnCommandExecute();
            extendedCanvas.VerticalSnapPointsChanged_OnCommandExecute();
        }

        private static void MinSnapPoint_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var extendedCanvas = d as ExtendedCanvas;

            extendedCanvas.HorizontalSnapPointsChanged_OnCommandExecute();
            extendedCanvas.VerticalSnapPointsChanged_OnCommandExecute();
        }

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
    }
}