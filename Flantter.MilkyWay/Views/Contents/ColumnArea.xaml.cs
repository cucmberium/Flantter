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
using Flantter.MilkyWay.ViewModels;
using Flantter.MilkyWay.Views.Util;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class ColumnArea : UserControl
    {
        public AccountViewModel ViewModel
        {
            get { return (AccountViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AccountViewModel), typeof(ColumnArea), null);
        

        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ColumnArea), new PropertyMetadata(0, SelectedIndex_Changed));

        public volatile int tempSelectedIndex = -1;
        public volatile bool isManualOperation = true;
        private static void SelectedIndex_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var columnArea = d as ColumnArea;

            if (columnArea.isManualOperation)
                columnArea.tempSelectedIndex = (int)e.NewValue;

            columnArea.ColumnArea_UpdateView(false);
			System.Diagnostics.Debug.WriteLine("SelectedIndex : " + e.NewValue);
        }

        private ScrollViewer _ScrollViewer = null;
        
        public ColumnArea()
        {
            this.InitializeComponent();
            
            this.SizeChanged += (s, e) =>
            {
                this.ColumnArea_UpdateView();
            };
            this.Loaded += (s, e) =>
            {
                var scrollViewer = this.ColumnArea_ColumnList.GetVisualChild<ScrollViewer>();
                if (scrollViewer == null)
                    return;

                _ScrollViewer = scrollViewer;

                scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged;

                extendedCanvas = this.ColumnArea_ColumnList.GetVisualChild<ExtendedCanvas>();
                extendedCanvas.SizeChanged += ExtendedCanvas_SizeChanged;

                this.ColumnArea_UpdateView();
            };

            this.LayoutUpdated += (s, e) =>
            {
                if (_ScrollViewer != null)
                    return;

                var scrollViewer = this.ColumnArea_ColumnList.GetVisualChild<ScrollViewer>();
                if (scrollViewer == null)
                    return;

                _ScrollViewer = scrollViewer;

                scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged;

                extendedCanvas = this.ColumnArea_ColumnList.GetVisualChild<ExtendedCanvas>();
                extendedCanvas.SizeChanged += ExtendedCanvas_SizeChanged;
            };
        }

        private void ExtendedCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_ScrollViewer == null)
                return;

            if (currentIndex < this.ViewModel.Columns.Count)
                return;

            var snapPointsList = extendedCanvas.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near);
            _ScrollViewer.ChangeView(snapPointsList.Last(), null, null, false);
        }

        private bool _InertialEvent;
        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            _InertialEvent = e.IsInertial;
            if (!e.IsInertial)
                return;

            _InertialEvent = e.IsInertial;
        }

        private ExtendedCanvas extendedCanvas = null;
        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (scrollViewerControlDisabled)
                return;

            if (e.IsIntermediate)
                return;

            if (!_InertialEvent)
                return;

            _InertialEvent = false;
            
            var snapPointsList = extendedCanvas.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near);

            try
            {
                currentIndex = snapPointsList.ToList().IndexOf(snapPointsList.Reverse().FirstOrDefault(x => x <= _ScrollViewer.HorizontalOffset));
                if (tempSelectedIndex != -1)
                    currentIndex = this.ViewModel.Columns[tempSelectedIndex].Index.Value;

                tempSelectedIndex = -1;

                this.isManualOperation = false;
                this.SelectedIndex = this.ViewModel.Columns.IndexOf(this.ViewModel.Columns.First(x => x.Index.Value == currentIndex));
                this.isManualOperation = true;
            }
            catch
            {
            }
            
        }

        private bool scrollViewerControlDisabled = false;

        private int currentIndex = 0;
        public async void ColumnArea_UpdateView(bool disableAnimation = true)
        {
            scrollViewerControlDisabled = true;
            
            if (_ScrollViewer == null)
                return;

            if (isManualOperation == true)
            {
                _ScrollViewer.HorizontalSnapPointsType = SnapPointsType.Mandatory;
                this.AnimationCooldown(500);
            }

            // GetIrregularSnapPointsの更新に少し時間がいる
            await Task.Delay(10);
            
            var snapPointsList = extendedCanvas.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near);

            // Taboo : 禁忌
            currentIndex = this.ViewModel.Columns[this.SelectedIndex].Index.Value;

            if (currentIndex < 0 || snapPointsList.Count == 0)
                _ScrollViewer.ChangeView(snapPointsList.First(), null, null, disableAnimation);
            else if (currentIndex >= snapPointsList.Count)
                _ScrollViewer.ChangeView(snapPointsList.Last(), null, null, disableAnimation);
            else
                _ScrollViewer.ChangeView(snapPointsList[currentIndex], null, null, disableAnimation);

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
                if (_ScrollViewer == null)
                    return;

                _ScrollViewer.HorizontalSnapPointsType = SnapPointsType.MandatorySingle;
            }).AsTask().Wait();
        }
    }
}
