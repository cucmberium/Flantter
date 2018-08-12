using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.ViewModels;

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class BottomBarArea : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(AccountViewModel), typeof(BottomBarArea), null);

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(BottomBarArea),
                new PropertyMetadata(0, SelectedIndex_Changed));

        public BottomBarArea()
        {
            InitializeComponent();

            SizeChanged += BottomBarArea_SizeChanged;
            BottomBarAreaColumnSelecter.ItemClick += BottomBarAreaColumnSelecter_ItemClick;
            BottomBarAreaColumnSelecter.Loaded += (s, e) => BottomBarArea_SelectedIndexChanged();
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

        private void BottomBarAreaColumnSelecter_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedIndex = ViewModel.Columns.IndexOf((ColumnViewModel) e.ClickedItem);
        }

        private static void SelectedIndex_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bottomBarArea = d as BottomBarArea;
            bottomBarArea.BottomBarArea_SelectedIndexChanged();
        }

        public void BottomBarArea_SelectedIndexChanged()
        {
            foreach (var gridViewItem in BottomBarAreaColumnSelecter.FindVisualChildren<GridViewItem>())
            {
                var grid = gridViewItem.GetVisualChild<Grid>();
                var symbolIcon = gridViewItem.GetVisualChild<SymbolIcon>();
                if (ViewModel.Columns.IndexOf((ColumnViewModel) gridViewItem.Content) == SelectedIndex)
                {
                    grid.Background =
                        (SolidColorBrush) Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"];
                    symbolIcon.Foreground =
                        (SolidColorBrush) Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"];
                }
                else
                {
                    grid.Background =
                        (SolidColorBrush) Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"];
                    symbolIcon.Foreground =
                        (SolidColorBrush) Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"];
                }
            }
        }

        private void BottomBarArea_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BottomBarAreaProfileImageButton.Width = e.NewSize.Height;
            BottomBarAreaProfileImageButton.Height = e.NewSize.Height;
            BottomBarAreaColumnSelecter.ItemHeight = e.NewSize.Height;
        }
    }
}