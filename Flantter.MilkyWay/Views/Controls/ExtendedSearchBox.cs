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
    public sealed class ExtendedSearchBox : SearchBox
    {
        public ExtendedSearchBox()
        {
            this.DefaultStyleKey = typeof(SearchBox);
        }

        private Popup _Popup;
        private ListView _ListView;

        public bool SuggestionToTop
        {
            get { return (bool)this.GetValue(SuggestionToTopProperty); }
            set { this.SetValue(SuggestionToTopProperty, value); }
        }
        public static readonly DependencyProperty SuggestionToTopProperty =
                        DependencyProperty.RegisterAttached("SuggestionToTop", typeof(bool),
                        typeof(ExtendedSearchBox), null);

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this._Popup = (Popup)GetTemplateChild("SearchSuggestionsPopup");
            this._ListView = (ListView)GetTemplateChild("SearchSuggestionsList");
            this._ListView.SizeChanged += SearchSuggestionsList_SizeChanged;
        }

        private void SearchSuggestionsList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_Popup == null || _ListView == null)
                return;

            if (SuggestionToTop)
            {
                if (_Popup.VerticalAlignment == Windows.UI.Xaml.VerticalAlignment.Bottom)
                    _Popup.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;

                _Popup.Margin = new Thickness(0.0, -e.NewSize.Height, 0.0, -e.NewSize.Height);
            }
            else
            {
                if (_Popup.VerticalAlignment == Windows.UI.Xaml.VerticalAlignment.Top)
                    _Popup.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom;

                _Popup.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
            }
        }
    }
}
