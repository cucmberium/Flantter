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
    public class RetweetsOfMeSettingsFlyoutViewModel
    {
        public RetweetsOfMeSettingsFlyoutViewModel()
        {
            Model = new RetweetsOfMeSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x => { Model.RetweetsOfMe.Clear(); });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateRetweetsOfMe(); });

            RetweetsOfMeIncrementalLoadCommand = new ReactiveCommand();
            RetweetsOfMeIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.RetweetsOfMe.Count <= 0)
                        return;

                    var id = Model.RetweetsOfMe.Last().Id;
                    var status = Model.RetweetsOfMe.Last();
                    if (status.HasRetweetInformation)
                        id = status.RetweetInformation.Id;

                    await Model.UpdateRetweetsOfMe(id);
                });

            RetweetsOfMe =
                Model.RetweetsOfMe.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, Tokens.Value.UserId));

            Updating = Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            Notice = Notice.Instance;
        }

        public RetweetsOfMeSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReadOnlyReactiveCollection<StatusViewModel> RetweetsOfMe { get; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand RetweetsOfMeIncrementalLoadCommand { get; set; }

        public Notice Notice { get; set; }
    }
}