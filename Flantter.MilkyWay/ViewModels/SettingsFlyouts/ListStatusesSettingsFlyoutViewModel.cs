using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
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
    public class ListStatusesSettingsFlyoutViewModel
    {
        public ListStatusesSettingsFlyoutViewModel()
        {
            this.Model = new ListStatusesSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.Id = this.Model.ToReactivePropertyAsSynchronized(x => x.Id);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.FullName = new ReactiveProperty<string>();

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.Model.ListStatuses.Clear();
            });

            this.UpdateCommand = new ReactiveCommand();
            this.UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateListStatuses();
            });

            this.RefreshCommand = new ReactiveCommand();
            this.RefreshCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateListStatuses(clear: false);
            });

            this.ListStatusesIncrementalLoadCommand = new ReactiveCommand();
            this.ListStatusesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.ListStatuses.Count <= 0)
                    return;

                var id = this.Model.ListStatuses.Last().Id;
                var status = this.Model.ListStatuses.Last();
                if (status.HasRetweetInformation)
                    id = status.RetweetInformation.Id;

                await this.Model.UpdateListStatuses(id);
            });

            this.ListStatuses = this.Model.ListStatuses.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, this.Tokens.Value.UserId));

            this.Updating = this.Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;
        }

        public ListStatusesSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<long> Id { get; set; }

        public ReactiveProperty<string> FullName { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReadOnlyReactiveCollection<StatusViewModel> ListStatuses { get; private set; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }
        
        public ReactiveCommand RefreshCommand { get; set; }

        public ReactiveCommand ListStatusesIncrementalLoadCommand { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
