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
    public class RetweetersSettingsFlyoutViewModel
    {
        public RetweetersSettingsFlyoutViewModel()
        {
            Model = new RetweetersSettingsFlyoutModel();
            Id = Model.ToReactivePropertyAsSynchronized(x => x.Id);
            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x => { Model.Retweeters.Clear(); });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateRetweeters(); });

            RetweetersIncrementalLoadCommand = new ReactiveCommand();
            RetweetersIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateRetweeters(true); });

            Retweeters = Model.Retweeters.ToReadOnlyReactiveCollection(x => new UserViewModel(x));

            Updating = Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            Notice = Notice.Instance;
        }

        public RetweetersSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<long> Id { get; set; }

        public ReadOnlyReactiveCollection<UserViewModel> Retweeters { get; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand RetweetersIncrementalLoadCommand { get; set; }

        public Notice Notice { get; set; }
    }
}