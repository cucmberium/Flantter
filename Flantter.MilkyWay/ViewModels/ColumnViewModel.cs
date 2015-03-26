using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.ViewModels
{
    public class ColumnViewModel
    {
        private ColumnModel _ColumnModel { get; set; }

        public ReactiveProperty<double> Height { get; private set; }
        public ReactiveProperty<double> Width { get; private set; }
        public ReactiveProperty<double> Left { get; private set; }
        public IObservable<int> ColumnCount { get; set; }
        public ReactiveProperty<double> Opacity { get; private set; }

        public ReactiveProperty<SettingSupport.ColumnTypeEnum> Action { get; private set; }
        public ReactiveProperty<string> Name { get; private set; }
        public ReactiveProperty<string> OwnerScreenName { get; private set; }

        public ReactiveProperty<int> Index { get; private set; }

        #region Constructor
        /*public ColumnViewModel()
        {
        }*/

        public ColumnViewModel(ColumnModel column)
        {
            this._ColumnModel = column;

            this.Action = column.ObserveProperty(x => x.Action).ToReactiveProperty();
            this.Name = column.ObserveProperty(x => x.Name).ToReactiveProperty();
            this.OwnerScreenName = column.ObserveProperty(x => x.OwnerScreenName).ToReactiveProperty();
            this.Index = column.ObserveProperty(x => x.Index).ToReactiveProperty();


            #region Windows App
#if WINDOWS_APP
            this.ColumnCount = Observable.CombineLatest<double, double, int, int>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                SettingService.Setting.ObserveProperty(x => x.TweetFieldColumnSize),
                SettingService.Setting.ObserveProperty(x => x.TweetFieldColumnCount),
                (width, minWidth, maxCount) => 
                {
                    return (int)Math.Max(Math.Min(maxCount, (width - 5.0 * 2) / (minWidth + 5.0 * 2)), 1.0);
                }).ToReactiveProperty();

            this.Height = Observable.CombineLatest<double, double, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientHeight), 
                (width, height) => 
                {
                    if (width < 352.0)
                        return height - 64.0;
                    else if (width < 500.0)
                        return height - 64.0 - 20.0;
                    else
                        return height - 75.0 - 20.0;
                }).ToReactiveProperty();

            this.Width = Observable.CombineLatest<double, int, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                this.ColumnCount,
                (width, count) =>
                {
                    if (width < 352.0)
                        return width;
                    else
                        return (width - 5.0 * 2) / count - 10.0;
                }).ToReactiveProperty();

            this.Left = Observable.CombineLatest<double, int, double, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                this.Index,
                this.Width,
                (width, index, columnWidth) =>
                {
                    if (width < 352.0)
                        return index * (columnWidth + 10.0);
                    else
                        return 5.0 + index * (columnWidth + 10.0);
                }).ToReactiveProperty();

            this.Opacity = Observable.CombineLatest<int, int, double>(
                this.Index,
                this.ColumnCount,
                (index, count) =>
                {
                    return (index < count) ? 1.0 : 0.0;
                }).ToReactiveProperty();
#endif
            #endregion

            #region Windows Phone App
#if WINDOWS_PHONE_APP
            this.Height = WindowSizeHelper.Instance.ObserveProperty(x => x.ClientHeight).Select(x =>
            { return x - 54.0; }).ToReactiveProperty();

            this.Width = WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth).Select(x =>
            { return x; }).ToReactiveProperty();

            this.Left = Observable.CombineLatest<double, int, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                this.Index,
                (width, index) =>
                {
                    return index * width;
                }).ToReactiveProperty();
#endif
            #endregion

            this.ColumnCount = Observable.CombineLatest<double, double, int, int>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                SettingService.Setting.ObserveProperty(x => x.TweetFieldColumnSize),
                SettingService.Setting.ObserveProperty(x => x.TweetFieldColumnCount),
                (width, minWidth, maxCount) =>
                {
                    return (int)Math.Max(Math.Min(maxCount, (width - 5.0 * 2) / (minWidth + 5.0 * 2)), 1.0);
                }).ToReactiveProperty();

            this.Height = Observable.CombineLatest<double, double, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientHeight),
                (width, height) =>
                {
                    if (width < 352.0)
                        return height - 64.0;
                    else if (width < 500.0)
                        return height - 64.0 - 20.0;
                    else
                        return height - 75.0 - 20.0;
                }).ToReactiveProperty();

            this.Width = Observable.CombineLatest<double, int, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                this.ColumnCount,
                (width, count) =>
                {
                    if (width < 352.0)
                        return width;
                    else
                        return (width - 5.0 * 2) / count - 10.0;
                }).ToReactiveProperty();

            this.Left = Observable.CombineLatest<double, int, double, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                this.Index,
                this.Width,
                (width, index, columnWidth) =>
                {
                    if (width < 352.0)
                        return index * (columnWidth + 10.0);
                    else
                        return 5.0 + index * (columnWidth + 10.0);
                }).ToReactiveProperty();

            this.Opacity = Observable.CombineLatest<int, int, double>(
                this.Index,
                this.ColumnCount,
                (index, count) =>
                {
                    return (index < count) ? 1.0 : 0.0;
                }).ToReactiveProperty();
        }
        #endregion

        #region Destructor
        ~ColumnViewModel()
        {
        }
        #endregion
    }
}
