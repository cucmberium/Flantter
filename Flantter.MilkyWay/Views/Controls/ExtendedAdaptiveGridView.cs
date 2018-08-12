using System;
using System.Linq;
using System.Windows.Input;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Flantter.MilkyWay.Views.Controls
{
    public class ExtendedAdaptiveGridView : GridView
    {
        public static readonly DependencyProperty ItemClickCommandProperty =
            DependencyProperty.Register(nameof(ItemClickCommand), typeof(ICommand), typeof(ExtendedAdaptiveGridView),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(ExtendedAdaptiveGridView),
                new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty OneRowModeEnabledProperty =
            DependencyProperty.Register(nameof(OneRowModeEnabled), typeof(bool), typeof(ExtendedAdaptiveGridView),
                new PropertyMetadata(false, (o, e) => { OnOneRowModeEnabledChanged(o, e.NewValue); }));

        private static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(ExtendedAdaptiveGridView),
                new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty DesiredWidthProperty =
            DependencyProperty.Register(nameof(DesiredWidth), typeof(double), typeof(ExtendedAdaptiveGridView),
                new PropertyMetadata(double.NaN, DesiredWidthChanged));

        public static readonly DependencyProperty StretchContentForSingleRowProperty =
            DependencyProperty.Register(nameof(StretchContentForSingleRow), typeof(bool),
                typeof(ExtendedAdaptiveGridView),
                new PropertyMetadata(true, OnStretchContentForSingleRowPropertyChanged));

        private bool _isLoaded;
        private bool _needContainerMarginForLayout;
        private bool _needToRestoreScrollStates;
        private ScrollBarVisibility _savedHorizontalScrollBarVisibility;
        private ScrollMode _savedHorizontalScrollMode;
        private Orientation _savedOrientation;
        private ScrollBarVisibility _savedVerticalScrollBarVisibility;
        private ScrollMode _savedVerticalScrollMode;

        public ExtendedAdaptiveGridView()
        {
            IsTabStop = false;
            SizeChanged += OnSizeChanged;
            ItemClick += OnItemClick;
            Items.VectorChanged += ItemsOnVectorChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            // Prevent issues with higher DPIs and underlying panel. #1803
            UseLayoutRounding = false;
        }

        public double DesiredWidth
        {
            get => (double) GetValue(DesiredWidthProperty);
            set => SetValue(DesiredWidthProperty, value);
        }

        public bool StretchContentForSingleRow
        {
            get => (bool) GetValue(StretchContentForSingleRowProperty);
            set => SetValue(StretchContentForSingleRowProperty, value);
        }

        public ICommand ItemClickCommand
        {
            get => (ICommand) GetValue(ItemClickCommandProperty);
            set => SetValue(ItemClickCommandProperty, value);
        }

        public double ItemHeight
        {
            get => (double) GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public bool OneRowModeEnabled
        {
            get => (bool) GetValue(OneRowModeEnabledProperty);
            set => SetValue(OneRowModeEnabledProperty, value);
        }

        public new ItemsPanelTemplate ItemsPanel => base.ItemsPanel;

        private double ItemWidth
        {
            get => (double) GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        private static void OnOneRowModeEnabledChanged(DependencyObject d, object newValue)
        {
            var self = d as ExtendedAdaptiveGridView;
            self.DetermineOneRowMode();
        }

        private static void DesiredWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as ExtendedAdaptiveGridView;
            self.RecalculateLayout(self.ActualWidth);
        }

        private static void OnStretchContentForSingleRowPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var self = d as ExtendedAdaptiveGridView;
            self.RecalculateLayout(self.ActualWidth);
        }

        private static int CalculateColumns(double containerWidth, double itemWidth)
        {
            var columns = (int) Math.Round(containerWidth / itemWidth);
            if (columns == 0) columns = 1;

            return columns;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject obj, object item)
        {
            base.PrepareContainerForItemOverride(obj, item);
            if (obj is FrameworkElement element)
            {
                var heightBinding = new Binding
                {
                    Source = this,
                    Path = new PropertyPath("ItemHeight"),
                    Mode = BindingMode.TwoWay
                };

                var widthBinding = new Binding
                {
                    Source = this,
                    Path = new PropertyPath("ItemWidth"),
                    Mode = BindingMode.TwoWay
                };

                element.SetBinding(HeightProperty, heightBinding);
                element.SetBinding(WidthProperty, widthBinding);
            }

            if (obj is ContentControl contentControl)
            {
                contentControl.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                contentControl.VerticalContentAlignment = VerticalAlignment.Stretch;
            }

            if (_needContainerMarginForLayout)
            {
                _needContainerMarginForLayout = false;
                RecalculateLayout(ActualWidth);
            }
        }

        protected virtual double CalculateItemWidth(double containerWidth)
        {
            if (double.IsNaN(DesiredWidth)) return DesiredWidth;

            var columns = CalculateColumns(containerWidth, DesiredWidth);

            // If there's less items than there's columns, reduce the column count (if requested);
            if (Items != null && Items.Count > 0 && Items.Count < columns && StretchContentForSingleRow)
                columns = Items.Count;

            // subtract the margin from the width so we place the correct width for placement
            var fallbackThickness = default(Thickness);
            var itemMargin = AdaptiveHeightValueConverter.GetItemMargin(this, fallbackThickness);
            if (itemMargin == fallbackThickness) _needContainerMarginForLayout = true;

            return containerWidth / columns - itemMargin.Left - itemMargin.Right;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            OnOneRowModeEnabledChanged(this, OneRowModeEnabled);
        }

        private void ItemsOnVectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs @event)
        {
            if (!double.IsNaN(ActualWidth)) RecalculateLayout(ActualWidth);
        }

        private void OnItemClick(object sender, ItemClickEventArgs e)
        {
            var cmd = ItemClickCommand;
            if (cmd != null)
                if (cmd.CanExecute(e.ClickedItem))
                    cmd.Execute(e.ClickedItem);
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // If we are in center alignment, we only care about relayout if the number of columns we can display changes
            // Fixes #1737
            if (HorizontalAlignment != HorizontalAlignment.Stretch)
            {
                var prevColumns = CalculateColumns(e.PreviousSize.Width, DesiredWidth);
                var newColumns = CalculateColumns(e.NewSize.Width, DesiredWidth);

                // If the width of the internal list view changes, check if more or less columns needs to be rendered.
                if (prevColumns != newColumns) RecalculateLayout(e.NewSize.Width);
            }
            else if (e.PreviousSize.Width != e.NewSize.Width)
            {
                // We need to recalculate width as our size changes to adjust internal items.
                RecalculateLayout(e.NewSize.Width);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = true;
            DetermineOneRowMode();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _isLoaded = false;
        }

        private void DetermineOneRowMode()
        {
            if (_isLoaded)
            {
                var itemsWrapGridPanel = ItemsPanelRoot as ItemsWrapGrid;

                if (OneRowModeEnabled)
                {
                    var b = new Binding
                    {
                        Source = this,
                        Path = new PropertyPath("ItemHeight"),
                        Converter = new AdaptiveHeightValueConverter(),
                        ConverterParameter = this
                    };

                    if (itemsWrapGridPanel != null)
                    {
                        _savedOrientation = itemsWrapGridPanel.Orientation;
                        itemsWrapGridPanel.Orientation = Orientation.Vertical;
                    }

                    SetBinding(MaxHeightProperty, b);

                    _savedHorizontalScrollMode = ScrollViewer.GetHorizontalScrollMode(this);
                    _savedVerticalScrollMode = ScrollViewer.GetVerticalScrollMode(this);
                    _savedHorizontalScrollBarVisibility = ScrollViewer.GetHorizontalScrollBarVisibility(this);
                    _savedVerticalScrollBarVisibility = ScrollViewer.GetVerticalScrollBarVisibility(this);
                    _needToRestoreScrollStates = true;

                    ScrollViewer.SetVerticalScrollMode(this, ScrollMode.Disabled);
                    ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Hidden);
                    ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Auto);
                    ScrollViewer.SetHorizontalScrollMode(this, ScrollMode.Enabled);
                }
                else
                {
                    ClearValue(MaxHeightProperty);

                    if (!_needToRestoreScrollStates) return;

                    _needToRestoreScrollStates = false;

                    if (itemsWrapGridPanel != null) itemsWrapGridPanel.Orientation = _savedOrientation;

                    ScrollViewer.SetVerticalScrollMode(this, _savedVerticalScrollMode);
                    ScrollViewer.SetVerticalScrollBarVisibility(this, _savedVerticalScrollBarVisibility);
                    ScrollViewer.SetHorizontalScrollBarVisibility(this, _savedHorizontalScrollBarVisibility);
                    ScrollViewer.SetHorizontalScrollMode(this, _savedHorizontalScrollMode);
                }
            }
        }

        private void RecalculateLayout(double containerWidth)
        {
            var itemsPanel = ItemsPanelRoot;
            var panelMargin = itemsPanel != null ? itemsPanel.Margin.Left + itemsPanel.Margin.Right : 0;

            // width should be the displayable width
            containerWidth = containerWidth - Padding.Left - Padding.Right - panelMargin;
            if (containerWidth > 0)
            {
                var newWidth = CalculateItemWidth(containerWidth);
                ItemWidth = Math.Floor(newWidth);
            }
        }
    }

    internal class AdaptiveHeightValueConverter : IValueConverter
    {
        public Thickness DefaultItemMargin { get; set; } = new Thickness(0, 0, 4, 4);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null)
            {
                var gridView = (GridView) parameter;
                if (gridView == null) return value;

                double.TryParse(value.ToString(), out var height);

                var padding = gridView.Padding;
                var margin = GetItemMargin(gridView, DefaultItemMargin);
                height = height + margin.Top + margin.Bottom + padding.Top + padding.Bottom;

                return height;
            }

            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        internal static Thickness GetItemMargin(GridView view, Thickness fallback = default(Thickness))
        {
            var setter = view.ItemContainerStyle?.Setters.OfType<Setter>()
                .FirstOrDefault(s => s.Property == FrameworkElement.MarginProperty);
            if (setter != null) return (Thickness) setter.Value;

            if (view.Items.Count > 0)
            {
                var container = (GridViewItem) view.ContainerFromIndex(0);
                if (container != null) return container.Margin;
            }

            // Use the default thickness for a GridViewItem
            return fallback;
        }
    }
}