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

            this.ColumnHeight = Observable.CombineLatest<double, double, bool, UserInteractionMode, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientHeight),
                SettingService.Setting.ObserveProperty(x => x.ExtendTitleBar),
                WindowSizeHelper.Instance.ObserveProperty(x => x.UserInteractionMode),
                (width, height, extendTitleBar, userInteractionMode) =>
                {
                    var titleBarVisiblity = false;
                    if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                        titleBarVisiblity = false;
                    else if (userInteractionMode == UserInteractionMode.Mouse)
                        titleBarVisiblity = true;
                    else
                        titleBarVisiblity = extendTitleBar;

                    var retheight = 0.0;
                    if (width < 352.0)
                        retheight = height - 64.0 - (titleBarVisiblity ? 32.0 : 0.0);
                    else if (width < 500.0)
                        retheight = height - 64.0 - 20.0 - (titleBarVisiblity ? 32.0 : 0.0);
                    else
                        retheight = height - 75.0 - 20.0 - (titleBarVisiblity ? 32.0 : 0.0);

                    return retheight;
                }).ToReactiveProperty();
        }
        
        public ReactiveProperty<int> ColumnCount { get; private set; }

        public ReactiveProperty<double> ColumnWidth { get; private set; }
        public ReactiveProperty<double> ColumnHeight { get; private set; }
    }
}
