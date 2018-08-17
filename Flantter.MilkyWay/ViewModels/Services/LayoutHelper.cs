using System;
using System.Reactive.Linq;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels.Services
{
    public class LayoutHelper
    {
        private LayoutHelper()
        {
            ColumnCount = WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth)
                .CombineLatest(WindowSizeHelper.Instance.ObserveProperty(x => x.WindowHeight),
                    SettingService.Setting.ObserveProperty(x => x.MinColumnSize),
                    SettingService.Setting.ObserveProperty(x => x.MaxColumnCount),
                    (width, winHeight, minWidth, maxCount) =>
                    {
                        if (winHeight >= 500)
                            return (int) Math.Max(Math.Min(maxCount, (width - 5.0 * 2) / (minWidth + 5.0 * 2)), 1.0);
                        return 1;
                    })
                .ToReactiveProperty();

            ColumnWidth = WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth)
                .CombineLatest(WindowSizeHelper.Instance.ObserveProperty(x => x.WindowHeight),
                    ColumnCount,
                    (width, winHeight, count) =>
                    {
                        if (winHeight >= 500)
                            if (width < 384.0)
                                return width;
                            else
                                return (width - 5.0 * 2) / count - 10.0;
                        return width;
                    })
                .ToReactiveProperty();

            ColumnHeight = WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth)
                .CombineLatest(WindowSizeHelper.Instance.ObserveProperty(x => x.ClientHeight),
                    WindowSizeHelper.Instance.ObserveProperty(x => x.WindowHeight),
                    (width, height, winHeight) =>
                    {
                        if (winHeight >= 500)
                        {
                            double retheight;
                            if (width < 384.0)
                                retheight = height - 64.0;
                            else if (width < 500.0)
                                retheight = height - 64.0 - 20.0;
                            else
                                retheight = height - 64.0 - 20.0;

                            return retheight;
                        }
                        return height - 64.0;
                    })
                .ToReactiveProperty();
        }

        public static LayoutHelper Instance { get; } = new LayoutHelper();

        public ReactiveProperty<int> ColumnCount { get; }

        public ReactiveProperty<double> ColumnWidth { get; }
        public ReactiveProperty<double> ColumnHeight { get; }
    }
}