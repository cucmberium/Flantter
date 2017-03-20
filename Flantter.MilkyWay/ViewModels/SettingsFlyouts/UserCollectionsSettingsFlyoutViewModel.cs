using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class UserCollectionsSettingsFlyoutViewModel
    {
        public UserCollectionsSettingsFlyoutViewModel()
        {
            this.Model = new UserCollectionsSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.ScreenName = this.Model.ToReactivePropertyAsSynchronized(x => x.ScreenName);
            
            this.CreateCollectionOpen = new ReactiveProperty<bool>();
            this.UpdateCollectionOpen = new ReactiveProperty<bool>();
            this.CollectionMenuOpen = new ReactiveProperty<bool>();

            this.Name = new ReactiveProperty<string>();
            this.Description = new ReactiveProperty<string>();
            this.Url = new ReactiveProperty<string>();
            this.Id = new ReactiveProperty<string>();

            this.SelectedIndex = new ReactiveProperty<int>(-1);
            this.UpdateCollectionButtonIsEnabled = this.SelectedIndex.Select(x => x != -1).ToReactiveProperty();

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.UpdateCollectionOpen.Value = false;
                this.CreateCollectionOpen.Value = false;
                this.CollectionMenuOpen.Value = false;

                this.Model.UserCollections.Clear();
            });

            this.UpdateCommand = new ReactiveCommand();
            this.UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateUserCollections();

                if (this.Tokens.Value.ScreenName == this.ScreenName.Value)
                    this.CollectionMenuOpen.Value = true;
            });

            this.UserCollectionsIncrementalLoadCommand = new ReactiveCommand();
            this.UserCollectionsIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.UserCollections.Count > 0)
                    await this.Model.UpdateUserCollections(true);
            });

            this.OpenCreateCollectionCommand = new ReactiveCommand();
            this.OpenCreateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.Name.Value = "";
                this.Description.Value = "";
                this.Url.Value = "";
                this.Id.Value = "";

                this.CollectionMenuOpen.Value = false;
                this.UpdateCollectionOpen.Value = false;
                this.CreateCollectionOpen.Value = true;
            });

            this.CloseCreateCollectionCommand = new ReactiveCommand();
            this.CloseCreateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.Name.Value = "";
                this.Description.Value = "";
                this.Url.Value = "";
                this.Id.Value = "";

                this.CollectionMenuOpen.Value = true;
                this.UpdateCollectionOpen.Value = false;
                this.CreateCollectionOpen.Value = false;
            });

            this.OpenUpdateCollectionCommand = new ReactiveCommand();
            this.OpenUpdateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                if (this.SelectedIndex.Value == -1)
                    return;

                var collection = this.Model.UserCollections.ElementAt(this.SelectedIndex.Value);
                this.Name.Value = collection.Name;
                this.Description.Value = collection.Description;
                this.Url.Value = collection.Url;
                this.Id.Value = collection.Id;

                this.CollectionMenuOpen.Value = false;
                this.UpdateCollectionOpen.Value = true;
                this.CreateCollectionOpen.Value = false;
            });

            this.CloseUpdateCollectionCommand = new ReactiveCommand();
            this.CloseUpdateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.Name.Value = "";
                this.Description.Value = "";
                this.Url.Value = "";
                this.Id.Value = "";

                this.CollectionMenuOpen.Value = true;
                this.UpdateCollectionOpen.Value = false;
                this.CreateCollectionOpen.Value = false;
            });

            this.CreateCollectionCommand = new ReactiveCommand();
            this.CreateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (!string.IsNullOrWhiteSpace(this.Id.Value))
                    return;

                var result = await this.Model.CreateCollection(this.Name.Value, this.Description.Value, this.Url.Value);

                if (!result)
                    return;

                this.CollectionMenuOpen.Value = true;
                this.UpdateCollectionOpen.Value = false;
                this.CreateCollectionOpen.Value = false;
                await this.Model.UpdateUserCollections();
            });

            this.UpdateCollectionCommand = new ReactiveCommand();
            this.UpdateCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (string.IsNullOrWhiteSpace(this.Id.Value))
                    return;

                var result = await this.Model.UpdateCollection(this.Id.Value, this.Name.Value, this.Description.Value, this.Url.Value);

                if (!result)
                    return;

                this.CollectionMenuOpen.Value = true;
                this.UpdateCollectionOpen.Value = false;
                this.CreateCollectionOpen.Value = false;
                await this.Model.UpdateUserCollections();
            });

            this.DeleteCollectionCommand = new ReactiveCommand();
            this.DeleteCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.SelectedIndex.Value == -1)
                    return;

                var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_DeleteCollection"), Title = "Confirmation" };
                await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                if (!msgNotification.Result)
                    return;

                var collection = this.Model.UserCollections.ElementAt(this.SelectedIndex.Value);
                var result = await this.Model.DeleteCollection(collection.Id);

                if (!result)
                    return;

                this.CollectionMenuOpen.Value = true;
                this.UpdateCollectionOpen.Value = false;
                this.CreateCollectionOpen.Value = false;
                await this.Model.UpdateUserCollections();
            });

            this.UserCollections = this.Model.UserCollections.ToReadOnlyReactiveCollection(x => new CollectionViewModel(x));

            this.UpdatingUserCollections = this.Model.ObserveProperty(x => x.UpdatingUserCollections).ToReactiveProperty();
            this.CreatingCollection = this.Model.ObserveProperty(x => x.CreatingCollection).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;
        }

        public UserCollectionsSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> UpdatingUserCollections { get; set; }
        public ReactiveProperty<bool> CreatingCollection { get; set; }

        public ReactiveProperty<CoreTweet.Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<string> ScreenName { get; set; }

        public ReadOnlyReactiveCollection<CollectionViewModel> UserCollections { get; private set; }

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

        public Services.Notice Notice { get; set; }
    }
}
