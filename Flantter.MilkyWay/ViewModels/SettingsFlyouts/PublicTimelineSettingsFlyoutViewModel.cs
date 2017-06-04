using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Flantter.MilkyWay.Setting;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class PublicTimelineSettingsFlyoutViewModel
    {
        public PublicTimelineSettingsFlyoutViewModel()
        {
            Model = new PublicTimelineSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            Type = Model.ToReactivePropertyAsSynchronized(x => x.Type);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x => { Model.PublicTimelineStatuses.Clear(); });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdatePublicTimeline(); });

            RefreshCommand = new ReactiveCommand();
            RefreshCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdatePublicTimeline(clear: false); });

            PublicTimelineIncrementalLoadCommand = new ReactiveCommand();
            PublicTimelineIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.PublicTimelineStatuses.Count <= 0)
                        return;

                    var id = Model.PublicTimelineStatuses.Last().Id;
                    var status = Model.PublicTimelineStatuses.Last();
                    if (status.HasRetweetInformation)
                        id = status.RetweetInformation.Id;

                    await Model.UpdatePublicTimeline(id);
                });

            PublicTimelineStatuses =
                Model.PublicTimelineStatuses.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, Tokens.Value.UserId));

            AddColumnCommand = new ReactiveCommand();
            AddColumnCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var columnSetting = new ColumnSetting
                    {
                        Action = Model.Type == "Local" ? SettingSupport.ColumnTypeEnum.Local : SettingSupport.ColumnTypeEnum.Federated,
                        AutoRefresh = false,
                        AutoRefreshTimerInterval = 180.0,
                        Filter = "()",
                        Name = Model.Type,
                        Parameter = null,
                        Streaming = true,
                        Index = -1,
                        DisableStartupRefresh = false,
                        FetchingNumberOfTweet = 40
                    };
                    Notice.Instance.AddColumnCommand.Execute(columnSetting);
                });

            Updating = Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            Notice = Notice.Instance;
        }

        public PublicTimelineSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<string> Type { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReadOnlyReactiveCollection<StatusViewModel> PublicTimelineStatuses { get; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand RefreshCommand { get; set; }

        public ReactiveCommand PublicTimelineIncrementalLoadCommand { get; set; }

        public ReactiveCommand AddColumnCommand { get; set; }

        public Notice Notice { get; set; }
    }
}