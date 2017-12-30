using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.ViewModels.Apis.Objects;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class UserCollectionsSettingsFlyoutViewModel
    {
        private readonly ResourceLoader _resourceLoader;

        public UserCollectionsSettingsFlyoutViewModel()
        {
            _resourceLoader = new ResourceLoader();

            Model = new UserCollectionsSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");
            UserId = Model.ToReactivePropertyAsSynchronized(x => x.UserId);

            CreateCollectionMenuOpen = new ReactiveProperty<bool>();
            UpdateCollectionMenuOpen = new ReactiveProperty<bool>();
            EditingCollectionMenuOpen = new ReactiveProperty<bool>();

            EditingCollectionName = new ReactiveProperty<string>();
            EditingCollectionDescription = new ReactiveProperty<string>();
            EditingCollectionUrl = new ReactiveProperty<string>();
            EditingCollectionId = new ReactiveProperty<string>();

            UserCollectionsSelectedIndex = new ReactiveProperty<int>(-1);
            UpdateCollectionButtonIsEnabled = UserCollectionsSelectedIndex.Select(x => x != -1).ToReactiveProperty();

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    UpdateCollectionMenuOpen.Value = false;
                    CreateCollectionMenuOpen.Value = false;
                    EditingCollectionMenuOpen.Value = false;

                    Model.UserCollections.Clear();
                });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    await Model.UpdateUserCollections();

                    if (Tokens.Value.UserId == UserId.Value)
                        EditingCollectionMenuOpen.Value = true;
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
                    EditingCollectionName.Value = "";
                    EditingCollectionDescription.Value = "";
                    EditingCollectionUrl.Value = "";
                    EditingCollectionId.Value = "";

                    EditingCollectionMenuOpen.Value = false;
                    UpdateCollectionMenuOpen.Value = false;
                    CreateCollectionMenuOpen.Value = true;
                });

            CloseCreateCollectionCommand = new ReactiveCommand();
            CloseCreateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    EditingCollectionName.Value = "";
                    EditingCollectionDescription.Value = "";
                    EditingCollectionUrl.Value = "";
                    EditingCollectionId.Value = "";

                    EditingCollectionMenuOpen.Value = true;
                    UpdateCollectionMenuOpen.Value = false;
                    CreateCollectionMenuOpen.Value = false;
                });

            OpenUpdateCollectionCommand = new ReactiveCommand();
            OpenUpdateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (UserCollectionsSelectedIndex.Value == -1)
                        return;

                    var collection = Model.UserCollections.ElementAt(UserCollectionsSelectedIndex.Value);
                    EditingCollectionName.Value = collection.Name;
                    EditingCollectionDescription.Value = collection.Description;
                    EditingCollectionUrl.Value = collection.Url;
                    EditingCollectionId.Value = collection.Id;

                    EditingCollectionMenuOpen.Value = false;
                    UpdateCollectionMenuOpen.Value = true;
                    CreateCollectionMenuOpen.Value = false;
                });

            CloseUpdateCollectionCommand = new ReactiveCommand();
            CloseUpdateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    EditingCollectionName.Value = "";
                    EditingCollectionDescription.Value = "";
                    EditingCollectionUrl.Value = "";
                    EditingCollectionId.Value = "";

                    EditingCollectionMenuOpen.Value = true;
                    UpdateCollectionMenuOpen.Value = false;
                    CreateCollectionMenuOpen.Value = false;
                });

            CreateCollectionCommand = new ReactiveCommand();
            CreateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (!string.IsNullOrWhiteSpace(EditingCollectionId.Value))
                        return;

                    var result = await Model.CreateCollection(EditingCollectionName.Value, EditingCollectionDescription.Value, EditingCollectionUrl.Value);

                    if (!result)
                        return;

                    EditingCollectionMenuOpen.Value = true;
                    UpdateCollectionMenuOpen.Value = false;
                    CreateCollectionMenuOpen.Value = false;
                    await Model.UpdateUserCollections();
                });

            UpdateCollectionCommand = new ReactiveCommand();
            UpdateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (string.IsNullOrWhiteSpace(EditingCollectionId.Value))
                        return;

                    var result = await Model.UpdateCollection(EditingCollectionId.Value, EditingCollectionName.Value, EditingCollectionDescription.Value, EditingCollectionUrl.Value);

                    if (!result)
                        return;

                    EditingCollectionMenuOpen.Value = true;
                    UpdateCollectionMenuOpen.Value = false;
                    CreateCollectionMenuOpen.Value = false;
                    await Model.UpdateUserCollections();
                });

            DeleteCollectionCommand = new ReactiveCommand();
            DeleteCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (UserCollectionsSelectedIndex.Value == -1)
                        return;

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_DeleteCollection"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    var collection = Model.UserCollections.ElementAt(UserCollectionsSelectedIndex.Value);
                    var result = await Model.DeleteCollection(collection.Id);

                    if (!result)
                        return;

                    EditingCollectionMenuOpen.Value = true;
                    UpdateCollectionMenuOpen.Value = false;
                    CreateCollectionMenuOpen.Value = false;
                    await Model.UpdateUserCollections();
                });

            UserCollections = Model.UserCollections.ToReadOnlyReactiveCollection(x => new CollectionViewModel(x));

            UpdatingUserCollections = Model.ObserveProperty(x => x.UpdatingUserCollections).ToReactiveProperty();
            CreatingOrUpdatingCollection = Model.ObserveProperty(x => x.CreatingOrUpdatingCollection).ToReactiveProperty();

            Notice = Notice.Instance;
        }

        public UserCollectionsSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> UpdatingUserCollections { get; set; }

        public ReactiveProperty<bool> CreatingOrUpdatingCollection { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<long> UserId { get; set; }

        public ReadOnlyReactiveCollection<CollectionViewModel> UserCollections { get; }

        public ReactiveProperty<int> UserCollectionsSelectedIndex { get; set; }

        public ReactiveProperty<bool> UpdateCollectionButtonIsEnabled { get; set; }

        public ReactiveProperty<bool> CreateCollectionMenuOpen { get; set; }

        public ReactiveProperty<bool> UpdateCollectionMenuOpen { get; set; }

        public ReactiveProperty<bool> EditingCollectionMenuOpen { get; set; }
        
        public ReactiveProperty<string> EditingCollectionName { get; set; }

        public ReactiveProperty<string> EditingCollectionDescription { get; set; }

        public ReactiveProperty<string> EditingCollectionUrl { get; set; }

        public ReactiveProperty<string> EditingCollectionId { get; set; }
        
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