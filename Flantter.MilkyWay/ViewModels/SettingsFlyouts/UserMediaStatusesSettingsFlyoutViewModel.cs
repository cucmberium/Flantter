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
    public class UserMediaStatusesSettingsFlyoutViewModel
    {
        public UserMediaStatusesSettingsFlyoutViewModel()
        {
            Model = new UserMediaStatusesSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            UserId = Model.ToReactivePropertyAsSynchronized(x => x.UserId);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x => { Model.UserMediaStatuses.Clear(); });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateUserMediaStatuses(); });

            RefreshCommand = new ReactiveCommand();
            RefreshCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateUserMediaStatuses(clear: false); });

            UserMediaStatusesIncrementalLoadCommand = new ReactiveCommand();
            UserMediaStatusesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateUserMediaStatuses(true); });

            UserMediaStatuses =
                Model.UserMediaStatuses.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, Tokens.Value.UserId));

            Updating = Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            Notice = Notice.Instance;
        }

        public UserMediaStatusesSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<long> UserId { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReadOnlyReactiveCollection<StatusViewModel> UserMediaStatuses { get; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand RefreshCommand { get; set; }

        public ReactiveCommand UserMediaStatusesIncrementalLoadCommand { get; set; }

        public Notice Notice { get; set; }
    }
}