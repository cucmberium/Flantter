using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class UserCollectionsSettingsFlyoutViewModel
    {
        private ResourceLoader _resourceLoader;

        public UserCollectionsSettingsFlyoutViewModel()
        {
            _resourceLoader = new ResourceLoader();

            Model = new UserCollectionsSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");
            UserId = Model.ToReactivePropertyAsSynchronized(x => x.UserId);

            CreateCollectionOpen = new ReactiveProperty<bool>();
            UpdateCollectionOpen = new ReactiveProperty<bool>();
            CollectionMenuOpen = new ReactiveProperty<bool>();

            Name = new ReactiveProperty<string>();
            Description = new ReactiveProperty<string>();
            Url = new ReactiveProperty<string>();
            Id = new ReactiveProperty<string>();

            SelectedIndex = new ReactiveProperty<int>(-1);
            UpdateCollectionButtonIsEnabled = SelectedIndex.Select(x => x != -1).ToReactiveProperty();

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    UpdateCollectionOpen.Value = false;
                    CreateCollectionOpen.Value = false;
                    CollectionMenuOpen.Value = false;

                    Model.UserCollections.Clear();
                });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    await Model.UpdateUserCollections();

                    if (Tokens.Value.UserId == UserId.Value)
                        CollectionMenuOpen.Value = true;
                });

            UserCollectionsIncrementalLoadCommand = new ReactiveCommand();
            UserCollectionsIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.UserCollections.Count > 0)
                        await Model.UpdateUserCollections(true);
                });

            OpenCreateCollectionCommand = new ReactiveCommand();
            OpenCreateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    Name.Value = "";
                    Description.Value = "";
                    Url.Value = "";
                    Id.Value = "";

                    CollectionMenuOpen.Value = false;
                    UpdateCollectionOpen.Value = false;
                    CreateCollectionOpen.Value = true;
                });

            CloseCreateCollectionCommand = new ReactiveCommand();
            CloseCreateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    Name.Value = "";
                    Description.Value = "";
                    Url.Value = "";
                    Id.Value = "";

                    CollectionMenuOpen.Value = true;
                    UpdateCollectionOpen.Value = false;
                    CreateCollectionOpen.Value = false;
                });

            OpenUpdateCollectionCommand = new ReactiveCommand();
            OpenUpdateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (SelectedIndex.Value == -1)
                        return;

                    var collection = Model.UserCollections.ElementAt(SelectedIndex.Value);
                    Name.Value = collection.Name;
                    Description.Value = collection.Description;
                    Url.Value = collection.Url;
                    Id.Value = collection.Id;

                    CollectionMenuOpen.Value = false;
                    UpdateCollectionOpen.Value = true;
                    CreateCollectionOpen.Value = false;
                });

            CloseUpdateCollectionCommand = new ReactiveCommand();
            CloseUpdateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    Name.Value = "";
                    Description.Value = "";
                    Url.Value = "";
                    Id.Value = "";

                    CollectionMenuOpen.Value = true;
                    UpdateCollectionOpen.Value = false;
                    CreateCollectionOpen.Value = false;
                });

            CreateCollectionCommand = new ReactiveCommand();
            CreateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (!string.IsNullOrWhiteSpace(Id.Value))
                        return;

                    var result = await Model.CreateCollection(Name.Value, Description.Value, Url.Value);

                    if (!result)
                        return;

                    CollectionMenuOpen.Value = true;
                    UpdateCollectionOpen.Value = false;
                    CreateCollectionOpen.Value = false;
                    await Model.UpdateUserCollections();
                });

            UpdateCollectionCommand = new ReactiveCommand();
            UpdateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (string.IsNullOrWhiteSpace(Id.Value))
                        return;

                    var result = await Model.UpdateCollection(Id.Value, Name.Value, Description.Value, Url.Value);

                    if (!result)
                        return;

                    CollectionMenuOpen.Value = true;
                    UpdateCollectionOpen.Value = false;
                    CreateCollectionOpen.Value = false;
                    await Model.UpdateUserCollections();
                });

            DeleteCollectionCommand = new ReactiveCommand();
            DeleteCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (SelectedIndex.Value == -1)
                        return;

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_DeleteCollection"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    var collection = Model.UserCollections.ElementAt(SelectedIndex.Value);
                    var result = await Model.DeleteCollection(collection.Id);

                    if (!result)
                        return;

                    CollectionMenuOpen.Value = true;
                    UpdateCollectionOpen.Value = false;
                    CreateCollectionOpen.Value = false;
                    await Model.UpdateUserCollections();
                });

            UserCollections = Model.UserCollections.ToReadOnlyReactiveCollection(x => new CollectionViewModel(x));

            UpdatingUserCollections = Model.ObserveProperty(x => x.UpdatingUserCollections).ToReactiveProperty();
            CreatingCollection = Model.ObserveProperty(x => x.CreatingCollection).ToReactiveProperty();

            Notice = Notice.Instance;
        }

        public UserCollectionsSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> UpdatingUserCollections { get; set; }
        public ReactiveProperty<bool> CreatingCollection { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<long> UserId { get; set; }

        public ReadOnlyReactiveCollection<CollectionViewModel> UserCollections { get; }

        public ReactiveProperty<int> SelectedIndex { get; set; }
        public ReactiveProperty<bool> UpdateCollectionButtonIsEnabled { get; set; }

        public ReactiveProperty<bool> CreateCollectionOpen { get; set; }

        public ReactiveProperty<bool> UpdateCollectionOpen { get; set; }

        public ReactiveProperty<bool> CollectionMenuOpen { get; set; }


        public ReactiveProperty<string> Name { get; set; }
        public ReactiveProperty<string> Description { get; set; }
        public ReactiveProperty<string> Url { get; set; }
        public ReactiveProperty<string> Id { get; set; }


        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand UserCollectionsIncrementalLoadCommand { get; set; }

        public ReactiveCommand OpenCreateCollectionCommand { get; set; }
        public ReactiveCommand CloseCreateCollectionCommand { get; set; }
        public ReactiveCommand OpenUpdateCollectionCommand { get; set; }
        public ReactiveCommand CloseUpdateCollectionCommand { get; set; }

        public ReactiveCommand CreateCollectionCommand { get; set; }
        public ReactiveCommand UpdateCollectionCommand { get; set; }
        public ReactiveCommand DeleteCollectionCommand { get; set; }

        public Notice Notice { get; set; }
    }
}