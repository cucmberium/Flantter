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
    public class RetweetsOfMeSettingsFlyoutViewModel
    {
        public RetweetsOfMeSettingsFlyoutViewModel()
        {
            this.Model = new RetweetsOfMeSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");
            
            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.Model.RetweetsOfMe.Clear();
            });

            this.UpdateCommand = new ReactiveCommand();
            this.UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateRetweetsOfMe();
            });

            this.RetweetsOfMeIncrementalLoadCommand = new ReactiveCommand();
            this.RetweetsOfMeIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.RetweetsOfMe.Count <= 0)
                    return;

                var id = this.Model.RetweetsOfMe.Last().Id;
                var status = this.Model.RetweetsOfMe.Last();
                if (status.HasRetweetInformation)
                    id = status.RetweetInformation.Id;

                await this.Model.UpdateRetweetsOfMe(id);
            });

            this.RetweetsOfMe = this.Model.RetweetsOfMe.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, this.Tokens.Value.UserId));

            this.Updating = this.Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;
        }

        public RetweetsOfMeSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReadOnlyReactiveCollection<StatusViewModel> RetweetsOfMe { get; private set; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand RetweetsOfMeIncrementalLoadCommand { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
