using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.ViewModels;

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class ColumnArea : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AccountViewModel), typeof(ColumnArea), null);

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(ColumnArea),
                new PropertyMetadata(0, SelectedIndex_Changed));

        private bool _inertialEvent;

        private ScrollViewer _scrollViewer;

        private volatile int _cooltime;

        private int _currentIndex;

        private ExtendedCanvas _extendedCanvas;
        private volatile bool _isAnimationCooldown;
        private volatile bool _isManualOperation = true;

        private bool _scrollViewerControlDisabled;

        private volatile int _tempSelectedIndex = -1;

        public ColumnArea()
        {
            InitializeComponent();

            SizeChanged += (s, e) => { ColumnArea_UpdateView(); };

            Loaded += (s, e) =>
            {
                var scrollViewer = ColumnAreaColumnList.GetVisualChild<ScrollViewer>();
                if (scrollViewer == null)
                    return;

                _scrollViewer = scrollViewer;

                scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged;

                _extendedCanvas = ColumnAreaColumnList.GetVisualChild<ExtendedCanvas>();
                _extendedCanvas.SizeChanged += ExtendedCanvas_SizeChanged;

                ColumnArea_UpdateView();
            };

            LayoutUpdated += (s, e) =>
            {
                if (_scrollViewer != null)
                    return;

                var scrollViewer = ColumnAreaColumnList.GetVisualChild<ScrollViewer>();
                if (scrollViewer == null)
                    return;

                _scrollViewer = scrollViewer;

                scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
                scrollViewer.ViewChanged += ScrollViewer_ViewChanged;

                _extendedCanvas = ColumnAreaColumnList.GetVisualChild<ExtendedCanvas>();
                _extendedCanvas.SizeChanged += ExtendedCanvas_SizeChanged;

                ColumnArea_UpdateView();
            };
        }

        public AccountViewModel ViewModel
        {
            get => (AccountViewModel) GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }


        public int SelectedIndex
        {
            get => (int) GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        private static void SelectedIndex_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var columnArea = d as ColumnArea;

            if (columnArea._isManualOperation)
                columnArea._tempSelectedIndex = (int) e.NewValue;

            columnArea.ColumnArea_UpdateView(false);
            Debug.WriteLine("SelectedIndex : " + e.NewValue);
        }

        private void ExtendedCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_scrollViewer == null)
                return;

            if (_currentIndex < ViewModel.Columns.Count)
                return;

            var snapPointsList =
                _extendedCanvas.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near);
            _scrollViewer.ChangeView(snapPointsList.Last(), null, null, false);
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            _inertialEvent = e.IsInertial;
            if (!e.IsInertial)
                return;

            _inertialEvent = e.IsInertial;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (_scrollViewerControlDisabled)
                return;

            if (e.IsIntermediate)
                return;

            if (!_inertialEvent)
                return;

            _inertialEvent = false;

            var snapPointsList =
                _extendedCanvas.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near);

            try
            {
                _currentIndex = snapPointsList.ToList()
                    .IndexOf(snapPointsList.Reverse().FirstOrDefault(x => x <= _scrollViewer.HorizontalOffset));
                if (_tempSelectedIndex != -1)
                    _currentIndex = ViewModel.Columns[_tempSelectedIndex].Index.Value;

                _tempSelectedIndex = -1;

                _isManualOperation = false;
                SelectedIndex = ViewModel.Columns.IndexOf(ViewModel.Columns.First(x => x.Index.Value == _currentIndex));
                _isManualOperation = true;
            }
            catch
            {
            }
        }

        public async void ColumnArea_UpdateView(bool disableAnimation = true)
        {
            _scrollViewerControlDisabled = true;

            if (_scrollViewer == null)
                return;

            if (_isManualOperation)
            {
                _scrollViewer.HorizontalSnapPointsType = SnapPointsType.Mandatory;
                AnimationCooldown(500);
            }

            // GetIrregularSnapPointsの更新に少し時間がいる
            await Task.Delay(10);

            var snapPointsList =
                _extendedCanvas.GetIrregularSnapPoints(Orientation.Horizontal, SnapPointsAlignment.Near);

            // Taboo : 禁忌
            _currentIndex = ViewModel.Columns[SelectedIndex].Index.Value;

            if (_currentIndex < 0 || snapPointsList.Count == 0)
                _scrollViewer.ChangeView(snapPointsList.First(), null, null, disableAnimation);
            else if (_currentIndex >= snapPointsList.Count)
                _scrollViewer.ChangeView(snapPointsList.Last(), null, null, disableAnimation);
            else
                _scrollViewer.ChangeView(snapPointsList[_currentIndex], null, null, disableAnimation);

            _scrollViewerControlDisabled = false;

            //System.Diagnostics.Debug.WriteLine("SelectedIndex : " + this.SelectedIndex);
        }

        private async void AnimationCooldown(int time)
        {
            _cooltime += time;
            if (_cooltime > 500)
                _cooltime = 500;

            if (_isAnimationCooldown) return;

            _isAnimationCooldown = true;

            await Task.Run(() => AnimationCooldownTask());
        }

        private void AnimationCooldownTask()
        {
            while (true)
            {
                if (!_isAnimationCooldown)
                    break;

                if (_cooltime < 0)
                    break;

                _cooltime -= 10;
                new Task(() => { }).Wait(10);
            }
            _cooltime = 0;
            _isAnimationCooldown = false;

            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (_scrollViewer == null)
                        return;

                    _scrollViewer.HorizontalSnapPointsType = SnapPointsType.MandatorySingle;
                })
                .AsTask()
                .Wait();
        }
    }
}