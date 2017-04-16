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

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class CollectionStatusesSettingsFlyoutViewModel
    {
        public CollectionStatusesSettingsFlyoutViewModel()
        {
            Model = new CollectionStatusesSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            Id = Model.ToReactivePropertyAsSynchronized(x => x.Id);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            Name = new ReactiveProperty<string>();

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x => { Model.CollectionStatuses.Clear(); });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateCollectionStatuses(); });

            RefreshCommand = new ReactiveCommand();
            RefreshCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateCollectionStatuses(clear: false); });

            CollectionStatusesIncrementalLoadCommand = new ReactiveCommand();
            CollectionStatusesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.CollectionStatuses.Count <= 0)
                        return;

                    await Model.UpdateCollectionStatuses(Model.CollectionStatuses.Last().SortIndex);
                });

            CollectionStatuses =
                Model.CollectionStatuses.ToReadOnlyReactiveCollection(
                    x => new StatusViewModel(x.Status, Tokens.Value.UserId, Id.Value));

            Updating = Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            Notice = Notice.Instance;
        }

        public CollectionStatusesSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<string> Id { get; set; }

        public ReactiveProperty<string> Name { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReadOnlyReactiveCollection<StatusViewModel> CollectionStatuses { get; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand RefreshCommand { get; set; }

        public ReactiveCommand CollectionStatusesIncrementalLoadCommand { get; set; }

        public Notice Notice { get; set; }
    }
}