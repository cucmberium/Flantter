using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Setting;
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
using Windows.ApplicationModel.Search.Core;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class StatusDetailSettingsFlyoutViewModel
    {
        public StatusDetailSettingsFlyoutViewModel()
        {
            this.Model = new StatusDetailSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.StatusId = this.Model.ToReactivePropertyAsSynchronized(x => x.StatusId);
            this.Status = this.Model.ObserveProperty(x => x.Status).Select(x => x != null ? new StatusViewModel(x, this.Tokens.Value.UserId) : null).ToReactiveProperty();

            this.UpdatingStatus = this.Model.ToReactivePropertyAsSynchronized(x => x.UpdatingStatus);
            this.UpdatingActionStatuses = this.Model.ToReactivePropertyAsSynchronized(x => x.UpdatingActionStatuses);

            this.PivotSelectedIndex = new ReactiveProperty<int>(0);
            this.PivotSelectedIndex.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (x == 1)
                {
                    if (this.Model.UpdatingActionStatuses || this.Model.ActionStatuses.Count > 0)
                        return;

                    await this.Model.UpdateActionStatuses();
                }
            });
            
            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.PivotSelectedIndex.Value = 0;

                this.Model.ActionStatuses.Clear();
            });

            this.UpdateStatusCommand = new ReactiveCommand();
            this.UpdateStatusCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateStatus();
            });            

            this.ActionStatuses = this.Model.ActionStatuses.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, this.Tokens.Value.UserId));

            this.Notice = Services.Notice.Instance;
        }

        public StatusDetailSettingsFlyoutModel Model { get; set; }
        
        public ReactiveProperty<long> StatusId { get; set; }

        public ReactiveProperty<bool> UpdatingStatus { get; set; }

        public ReactiveProperty<bool> UpdatingActionStatuses { get; set; }

        public ReactiveProperty<CoreTweet.Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<int> PivotSelectedIndex { get; set; }
        

        public ReactiveProperty<StatusViewModel> Status { get; set; }


        public ReadOnlyReactiveCollection<StatusViewModel> ActionStatuses { get; private set; }
        
        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateStatusCommand { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
