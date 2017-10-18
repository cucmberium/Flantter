using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.ViewModels.Apis.Objects;
using Flantter.MilkyWay.ViewModels.Services;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class StatusDetailSettingsFlyoutViewModel
    {
        public StatusDetailSettingsFlyoutViewModel()
        {
            Model = new StatusDetailSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            StatusId = Model.ToReactivePropertyAsSynchronized(x => x.StatusId);
            Status = Model.ObserveProperty(x => x.Status)
                .Select(x => x != null ? new StatusViewModel(x, Tokens.Value.UserId) : new StatusViewModel())
                .ToReactiveProperty();

            UpdatingStatus = Model.ToReactivePropertyAsSynchronized(x => x.UpdatingStatus);
            UpdatingActionStatuses = Model.ToReactivePropertyAsSynchronized(x => x.UpdatingActionStatuses);

            PivotSelectedIndex = new ReactiveProperty<int>(0);
            PivotSelectedIndex.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (x == 1)
                    {
                        if (Model.UpdatingActionStatuses || Model.ActionStatuses.Count > 0)
                            return;

                        await Model.UpdateActionStatuses();
                    }
                });

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    PivotSelectedIndex.Value = 0;

                    Model.ActionStatuses.Clear();
                });

            UpdateStatusCommand = new ReactiveCommand();
            UpdateStatusCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateStatus(); });

            ActionStatuses =
                Model.ActionStatuses.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, Tokens.Value.UserId));

            Notice = Notice.Instance;
        }

        public StatusDetailSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<long> StatusId { get; set; }

        public ReactiveProperty<bool> UpdatingStatus { get; set; }

        public ReactiveProperty<bool> UpdatingActionStatuses { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<int> PivotSelectedIndex { get; set; }


        public ReactiveProperty<StatusViewModel> Status { get; set; }


        public ReadOnlyReactiveCollection<StatusViewModel> ActionStatuses { get; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateStatusCommand { get; set; }

        public Notice Notice { get; set; }
    }
}