using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

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
                        typeof(ExtendedCanvas), new PropertyMetadata(0.0));

        public bool AreHorizontalSnapPointsRegular
        {
            get { return true; }
        }

        public bool AreVerticalSnapPointsRegular
        {
            get { return true; }
        }

        public IReadOnlyList<float> GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment)
        {
            return new List<float>();
        }

        public float GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment alignment, out float offset)
        {
            offset = 0.0F;
            return (float)SnapPointsSpaceing;
        }

        public event EventHandler<object> HorizontalSnapPointsChanged;
        public event EventHandler<object> VerticalSnapPointsChanged;

        public ExtendedCanvas()
        {
        }
    }
}
