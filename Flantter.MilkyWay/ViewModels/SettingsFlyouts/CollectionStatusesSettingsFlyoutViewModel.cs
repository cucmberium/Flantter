using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class CollectionStatusesSettingsFlyoutViewModel
    {
        public CollectionStatusesSettingsFlyoutViewModel()
        {
            this.Model = new CollectionStatusesSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.Id = this.Model.ToReactivePropertyAsSynchronized(x => x.Id);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.Name = new ReactiveProperty<string>();

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.Model.CollectionStatuses.Clear();
            });

            this.UpdateCommand = new ReactiveCommand();
            this.UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateCollectionStatuses();
            });

            this.RefreshCommand = new ReactiveCommand();
            this.RefreshCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateCollectionStatuses(clear: false);
            });

            this.CollectionStatusesIncrementalLoadCommand = new ReactiveCommand();
            this.CollectionStatusesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.CollectionStatuses.Count <= 0)
                    return;

                await this.Model.UpdateCollectionStatuses(this.Model.CollectionStatuses.Last().SortIndex);
            });

            this.CollectionStatuses = this.Model.CollectionStatuses.ToReadOnlyReactiveCollection(x => new StatusViewModel(x.Status, this.Tokens.Value.UserId, this.Id.Value));

            this.Updating = this.Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;
        }

        public CollectionStatusesSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<string> Id { get; set; }

        public ReactiveProperty<string> Name { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<CoreTweet.Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReadOnlyReactiveCollection<StatusViewModel> CollectionStatuses { get; private set; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }
        
        public ReactiveCommand RefreshCommand { get; set; }

        public ReactiveCommand CollectionStatusesIncrementalLoadCommand { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
