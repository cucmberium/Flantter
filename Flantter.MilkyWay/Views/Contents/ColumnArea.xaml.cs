using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Views.Controls;
using System.Threading.Tasks;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class ColumnArea : UserControl
    {
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ColumnArea), new PropertyMetadata(0, SelectedIndex_Changed));

        public volatile bool isManualOperation = true;
        private static void SelectedIndex_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var columnArea = d as ColumnArea;
            columnArea.ColumnArea_UpdateView(false);
			System.Diagnostics.Debug.WriteLine("SelectedIndex : " + e.NewValue);
		}

        public ColumnArea()
        {
            this.InitializeComponent();
            this.SizeChanged += (s, e) => { this.ColumnArea_UpdateView(); };
            this.Loaded += (s, e) =>
            {
                var scrollViewer = this.ColumnArea_ColumnList.GetVisualChild<ScrollViewer>();
                scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            };
        }

        private bool _InertialEvent;
        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            _InertialEvent = e.IsInertial;
            if (!e.IsInertial)
                return;

            _InertialEvent = e.IsInertial;

            /*
            var scrollViewer = sender as ScrollViewer;
            var extendedCanvas = this.ColumnArea_ColumnList.GetVisualChild<ExtendedCanvas>();
            var snapPointsList = extendedCanvas.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near);

            try
            {
                var index = snapPointsList.ToList().IndexOf(snapPointsList.Reverse().FirstOrDefault(x => x <= e.FinalView.HorizontalOffset));
                this.SelectedIndex = index;
            }
            catch
            {
            }
            */
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (scrollViewerControlDisabled)
                return;

            if (e.IsIntermediate)
                return;

            if (!_InertialEvent)
                return;

            _InertialEvent = false;

            var scrollViewer = sender as ScrollViewer;
            var extendedCanvas = this.ColumnArea_ColumnList.GetVisualChild<ExtendedCanvas>();
            var snapPointsList = extendedCanvas.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near);

            try
            {
                var index = snapPointsList.ToList().IndexOf(snapPointsList.Reverse().FirstOrDefault(x => x <= scrollViewer.HorizontalOffset));
                this.isManualOperation = false;
                this.SelectedIndex = index;
                this.isManualOperation = true;
            }
            catch
            {
            }
            
        }

        bool scrollViewerControlDisabled = false;
        public void ColumnArea_UpdateView(bool disableAnimation = true)
        {
            scrollViewerControlDisabled = true;

            var scrollViewer = this.ColumnArea_ColumnList.GetVisualChild<ScrollViewer>();
            if (scrollViewer == null)
                return;

            if (isManualOperation == true)
            {
                scrollViewer.HorizontalSnapPointsType = SnapPointsType.Mandatory;
                this.AnimationCooldown(500);
            }

            var extendedCanvas = this.ColumnArea_ColumnList.GetVisualChild<ExtendedCanvas>();
            var snapPointsList = extendedCanvas.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near);
            var selectedIndex = this.SelectedIndex;


            if (selectedIndex < 0 || snapPointsList.Count == 0)
                scrollViewer.ChangeView(snapPointsList.First(), null, null, disableAnimation);
            else if (selectedIndex >= snapPointsList.Count)
                scrollViewer.ChangeView(snapPointsList.Last(), null, null, disableAnimation);
            else
                scrollViewer.ChangeView(snapPointsList[selectedIndex], null, null, disableAnimation);

            scrollViewerControlDisabled = false;

            //System.Diagnostics.Debug.WriteLine("SelectedIndex : " + this.SelectedIndex);
        }

        public volatile int cooltime = 0;
        public volatile bool isAnimationCooldown = false;
        private async void AnimationCooldown(int time)
        {
            cooltime += time;
            if (cooltime > 500)
                cooltime = 500;

            if (isAnimationCooldown) return;

            isAnimationCooldown = true;

            await Task.Run(() => AnimationCooldownTask());
        }

        private void AnimationCooldownTask()
        {
            while (true)
            {
                if (!isAnimationCooldown)
                    break;

                if (cooltime < 0)
                    break;

                cooltime -= 10;
                new Task(() => { }).Wait(10);
            }
            cooltime = 0;
            isAnimationCooldown = false;

            this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => 
            {
                var scrollViewer = this.ColumnArea_ColumnList.GetVisualChild<ScrollViewer>();
                if (scrollViewer == null)
                    return;
                scrollViewer.HorizontalSnapPointsType = SnapPointsType.MandatorySingle;
            }).AsTask().Wait();
        }
    }
}
