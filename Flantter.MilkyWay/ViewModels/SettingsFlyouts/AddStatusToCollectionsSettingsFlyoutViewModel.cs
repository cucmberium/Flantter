using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class AddStatusToCollectionSettingsFlyoutViewModel
    {
        private ResourceLoader _resourceLoader;

        public AddStatusToCollectionSettingsFlyoutViewModel()
        {
            _resourceLoader = new ResourceLoader();

            Model = new AddStatusToCollectionSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            ScreenName = Model.ToReactivePropertyAsSynchronized(x => x.ScreenName);

            Status = new ReactiveProperty<Status>();

            SelectedIndex = new ReactiveProperty<int>(-1);

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    Status.Value = null;
                    ScreenName.Value = "";
                    Model.UserCollections.Clear();
                });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateUserCollections(); });

            UserCollectionsIncrementalLoadCommand = new ReactiveCommand();
            UserCollectionsIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.UserCollections.Count > 0)
                        await Model.UpdateUserCollections(true);
                });

            AddStatusToCollectionCommand = new ReactiveCommand();
            AddStatusToCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Status.Value == null)
                        return;

                    if (SelectedIndex.Value == -1)
                        return;

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_AddToCollection"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    await Model.AddStatusToCollection(Model.UserCollections[SelectedIndex.Value].Id, Status.Value.Id);
                });

            UserCollections = Model.UserCollections.ToReadOnlyReactiveCollection(x => new CollectionViewModel(x));

            UpdatingUserCollections = Model.ObserveProperty(x => x.UpdatingUserCollections).ToReactiveProperty();

            Notice = Notice.Instance;
        }

        public AddStatusToCollectionSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> UpdatingUserCollections { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<string> ScreenName { get; set; }

        public ReactiveProperty<Status> Status { get; set; }

        public ReadOnlyReactiveCollection<CollectionViewModel> UserCollections { get; }

        public ReactiveProperty<int> SelectedIndex { get; set; }


        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand UserCollectionsIncrementalLoadCommand { get; set; }

        public ReactiveCommand AddStatusToCollectionCommand { get; set; }

        public Notice Notice { get; set; }
    }
}