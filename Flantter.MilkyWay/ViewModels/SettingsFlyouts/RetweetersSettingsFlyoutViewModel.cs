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
    public class RetweetersSettingsFlyoutViewModel
    {
        public RetweetersSettingsFlyoutViewModel()
        {
            this.Model = new RetweetersSettingsFlyoutModel();
            this.Id = this.Model.ToReactivePropertyAsSynchronized(x => x.Id);
            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.Model.Retweeters.Clear();
            });

            this.UpdateCommand = new ReactiveCommand();
            this.UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateRetweeters();
            });

            this.RetweetersIncrementalLoadCommand = new ReactiveCommand();
            this.RetweetersIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateRetweeters(true);
            });

            this.Retweeters = this.Model.Retweeters.ToReadOnlyReactiveCollection(x => new UserViewModel(x));

            this.Updating = this.Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;
        }

        public RetweetersSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<long> Id { get; set; }

        public ReadOnlyReactiveCollection<UserViewModel> Retweeters { get; private set; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand RetweetersIncrementalLoadCommand { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
