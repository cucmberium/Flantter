using System;
using System.Linq;
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
    public class ListStatusesSettingsFlyoutViewModel
    {
        public ListStatusesSettingsFlyoutViewModel()
        {
            Model = new ListStatusesSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            Id = Model.ToReactivePropertyAsSynchronized(x => x.Id);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            FullName = new ReactiveProperty<string>();

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x => { Model.ListStatuses.Clear(); });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateListStatuses(); });

            RefreshCommand = new ReactiveCommand();
            RefreshCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateListStatuses(clear: false); });

            ListStatusesIncrementalLoadCommand = new ReactiveCommand();
            ListStatusesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.ListStatuses.Count <= 0)
                        return;

                    var id = Model.ListStatuses.Last().Id;
                    var status = Model.ListStatuses.Last();
                    if (status.HasRetweetInformation)
                        id = status.RetweetInformation.Id;

                    await Model.UpdateListStatuses(id);
                });

            ListStatuses =
                Model.ListStatuses.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, Tokens.Value.UserId));

            Updating = Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            Notice = Notice.Instance;
        }

        public ListStatusesSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<long> Id { get; set; }

        public ReactiveProperty<string> FullName { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReadOnlyReactiveCollection<StatusViewModel> ListStatuses { get; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand RefreshCommand { get; set; }

        public ReactiveCommand ListStatusesIncrementalLoadCommand { get; set; }

        public Notice Notice { get; set; }
    }
}