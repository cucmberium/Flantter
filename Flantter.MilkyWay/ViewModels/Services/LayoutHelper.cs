using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;

namespace Flantter.MilkyWay.ViewModels.Services
{
    public class LayoutHelper
    {
        private static LayoutHelper _Instance = new LayoutHelper();
        public static LayoutHelper Instance
        {
            get { return _Instance; }
        }

        private LayoutHelper()
        {
            this.ColumnCount = Observable.CombineLatest<double, double, double, int, int>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                WindowSizeHelper.Instance.ObserveProperty(x => x.WindowHeight),
                SettingService.Setting.ObserveProperty(x => x.MinColumnSize),
                SettingService.Setting.ObserveProperty(x => x.MaxColumnCount),
                (width, winHeight, minWidth, maxCount) =>
                {
                    if (winHeight >= 500)
                        return (int)Math.Max(Math.Min(maxCount, (width - 5.0 * 2) / (minWidth + 5.0 * 2)), 1.0);
                    else
                        return 1;
                }).ToReactiveProperty();

            this.ColumnWidth = Observable.CombineLatest<double, double, int, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                WindowSizeHelper.Instance.ObserveProperty(x => x.WindowHeight),
                this.ColumnCount,
                (width, winHeight, count) =>
                {
                    if (winHeight >= 500)
                    {
                        if (width < 384.0)
                            return width;
                        else
                            return (width - 5.0 * 2) / count - 10.0;
                    }
                    else
                    {
                        return width;
                    }
                }).ToReactiveProperty();

            this.ColumnHeight = Observable.CombineLatest<double, double, double, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientHeight),
                WindowSizeHelper.Instance.ObserveProperty(x => x.WindowHeight),
                (width, height, winHeight) =>
                {
                    if (winHeight >= 500)
                    {
                        var retheight = 0.0;
                        if (width < 384.0)
                            retheight = height - 64.0;
                        else if (width < 500.0)
                            retheight = height - 64.0 - 20.0;
                        else
                            retheight = height - 75.0 - 20.0;

                        return retheight;
                    }
                    else
                    {
                        return height - 64.0;
                    }
                }).ToReactiveProperty();
        }
        
        public ReactiveProperty<int> ColumnCount { get; private set; }

        public ReactiveProperty<double> ColumnWidth { get; private set; }
        public ReactiveProperty<double> ColumnHeight { get; private set; }
    }
}
