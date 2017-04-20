using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Flantter.MilkyWay.Views.Controls
{
    public sealed class ExtendedSearchBox : SearchBox
    {
        public static readonly DependencyProperty SuggestionToTopProperty =
            DependencyProperty.RegisterAttached("SuggestionToTop", typeof(bool),
                typeof(ExtendedSearchBox), null);

        private ListView _listView;

        private Popup _popup;

        public ExtendedSearchBox()
        {
            DefaultStyleKey = typeof(SearchBox);
        }

        public bool SuggestionToTop
        {
            get => (bool) GetValue(SuggestionToTopProperty);
            set => SetValue(SuggestionToTopProperty, value);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _popup = (Popup) GetTemplateChild("SearchSuggestionsPopup");
            _listView = (ListView) GetTemplateChild("SearchSuggestionsList");
            _listView.SizeChanged += SearchSuggestionsList_SizeChanged;
        }

        private void SearchSuggestionsList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_popup == null || _listView == null)
                return;

            if (SuggestionToTop)
            {
                if (_popup.VerticalAlignment == VerticalAlignment.Bottom)
                    _popup.VerticalAlignment = VerticalAlignment.Top;

                _popup.Margin = new Thickness(0.0, -e.NewSize.Height, 0.0, -e.NewSize.Height);
            }
            else
            {
                if (_popup.VerticalAlignment == VerticalAlignment.Top)
                    _popup.VerticalAlignment = VerticalAlignment.Bottom;

                _popup.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
            }
        }
    }
}