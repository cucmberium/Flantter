using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Setting;
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
    public class AccountViewModel : IDisposable
    {
        public AccountModel _AccountModel { get; set; }
        public Services.Notice Notice { get; set; }

        public ReadOnlyReactiveCollection<ColumnViewModel> Columns { get; private set; }
        public ReadOnlyReactiveCollection<string> AdditionalColumnsName { get; private set; }

        public ReactiveProperty<string> ProfileImageUrl { get; private set; }
        public ReactiveProperty<string> ProfileBannerUrl { get; private set; }
        public ReactiveProperty<string> ScreenName { get; private set; }
        public ReactiveProperty<bool> IsEnabled { get; private set; }
        public ReactiveProperty<double> PanelWidth { get; private set; }
        public ReactiveProperty<int> ColumnCount { get; private set; }
        public ReactiveProperty<double> ColumnWidth { get; private set; }
        public ReactiveProperty<double> SnapPointsSpaceing { get; private set; }
        public ReactiveProperty<double> MaxSnapPoint { get; private set; }
		public ReactiveProperty<double> MinSnapPoint { get; private set; }

        public ReactiveProperty<int> ColumnSelectedIndex { get; private set; }
        
        #region Constructor
        /*public AccountViewModel()
        {
        }*/

        public AccountViewModel(AccountModel account)
        {
            this._AccountModel = account;
            this.ScreenName = account.ObserveProperty(x => x.ScreenName).ToReactiveProperty();
            this.ProfileImageUrl = account.ObserveProperty(x => x.ProfileImageUrl).Select(x => !string.IsNullOrWhiteSpace(x) ? x : "http://localhost/").ToReactiveProperty();
            this.ProfileBannerUrl = account.ObserveProperty(x => x.ProfileBannerUrl).Select(x => !string.IsNullOrWhiteSpace(x) ? x : "http://localhost/").ToReactiveProperty();
            this.IsEnabled = account.ObserveProperty(x => x.IsEnabled).ToReactiveProperty();

            this.Columns = this._AccountModel.ReadOnlyColumns.ToReadOnlyReactiveCollection(x => new ColumnViewModel(x));
            this.AdditionalColumnsName = this._AccountModel.ReadOnlyColumns.ToObservable().Where(x => x.Name != "Home" && x.Name != "Mentions" && x.Name != "DirectMessages").Select(x => x.Name).ToReadOnlyReactiveCollection();
            
            this.ColumnCount = Observable.CombineLatest<double, double, int, int>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                SettingService.Setting.ObserveProperty(x => x.MinColumnSize),
                SettingService.Setting.ObserveProperty(x => x.MaxColumnCount),
                (width, minWidth, maxCount) =>
                {
                    return (int)Math.Max(Math.Min(maxCount, (width - 5.0 * 2) / (minWidth + 5.0 * 2)), 1.0);
                }).ToReactiveProperty();

            this.ColumnWidth = Observable.CombineLatest<double, int, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                this.ColumnCount,
                (width, count) =>
                {
                    if (width < 352.0)
                        return width;
                    else
                        return (width - 5.0 * 2) / count - 10.0;
                }).ToReactiveProperty();

            this.PanelWidth = Observable.CombineLatest<double, int, double>(
                this.ColumnWidth,
                this.Columns.ObserveProperty(x => x.Count),
                (width, count) =>
                {
                    if (width < 352.0)
                        return width * count + 352.0 * 2;
                    else
                        return (width + 10.0) * count + 352.0 * 2;
                }).ToReactiveProperty();

            this.SnapPointsSpaceing = this.ColumnWidth.Select(x => {
                return x + 10.0;
            }).ToReactiveProperty();

            this.MaxSnapPoint = Observable.CombineLatest<double, double, double>(this.PanelWidth, WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth), (panelWidth, windowWidth) => (panelWidth + 10.0) - windowWidth + 352.0).ToReactiveProperty();

			this.MinSnapPoint = new ReactiveProperty<double>(352.0);

            this.ColumnSelectedIndex = new ReactiveProperty<int>(0);

            this.Notice = Services.Notice.Instance;

            #region Command

            Services.Notice.Instance.ShowUserProfileCommand.Where(_ => this._AccountModel.IsEnabled).Subscribe(_ =>
            {
                System.Diagnostics.Debug.WriteLine("UserProfile Showed!");
            });

            #endregion
        }
        #endregion

        #region Destructor
        ~AccountViewModel()
        {
        }
        #endregion

        public void Dispose()
        {
            this.ScreenName.Dispose();
            this.ProfileImageUrl.Dispose();
            this.ProfileBannerUrl.Dispose();
            this.IsEnabled.Dispose();
            this.Columns.Dispose();
            this.AdditionalColumnsName.Dispose();
            this.ColumnCount.Dispose();
            this.ColumnWidth.Dispose();
            this.PanelWidth.Dispose();
            this.SnapPointsSpaceing.Dispose();
            this.MaxSnapPoint.Dispose();
            this.MinSnapPoint.Dispose();
            this.ColumnSelectedIndex.Dispose();
        }
    }
}
