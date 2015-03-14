using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.ViewModels
{
    public class AccountViewModel
    {
        private AccountModel _AccountModel { get; set; }

        public ReadOnlyReactiveCollection<ColumnViewModel> Columns { get; private set; }

        public ReactiveProperty<string> ProfileImageUrl { get; private set; }
        public ReactiveProperty<string> ProfileBannerUrl { get; private set; }
        public ReactiveProperty<bool> IsEnabled { get; private set; }

#if WINDOWS_PHONE_APP
        public ReactiveProperty<double> PanelWidth { get; private set; }
        public ReactiveProperty<double> SnapPointsSpaceing { get; private set; }
#endif

        #region Constructor
        /*public AccountViewModel()
        {
        }*/

        public AccountViewModel(AccountModel account)
        {
            this._AccountModel = account;
            this.ProfileImageUrl = account.ObserveProperty(x => x.ProfileImageUrl).ToReactiveProperty();
            this.ProfileBannerUrl = account.ObserveProperty(x => x.ProfileBannerUrl).ToReactiveProperty();
            this.IsEnabled = account.ObserveProperty(x => x.IsEnabled).ToReactiveProperty();

            this.Columns = this._AccountModel.ReadOnlyColumns.ToReadOnlyReactiveCollection(x => new ColumnViewModel(x));

#if WINDOWS_PHONE_APP
            this.PanelWidth = Observable.CombineLatest<double, int, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                this.Columns.ObserveProperty(x => x.Count),
                (width, count) =>
                {
                    return width * count;
                }).ToReactiveProperty();

            this.SnapPointsSpaceing = WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth).ToReactiveProperty();
#endif

        }
        #endregion

        #region Destructor
        ~AccountViewModel()
        {
        }
        #endregion
    }
}
