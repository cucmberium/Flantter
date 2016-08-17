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
    public class AddStatusToCollectionSettingsFlyoutViewModel
    {
        public AddStatusToCollectionSettingsFlyoutViewModel()
        {
            this.Model = new AddStatusToCollectionSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.ScreenName = this.Model.ToReactivePropertyAsSynchronized(x => x.ScreenName);

            this.Status = new ReactiveProperty<Status>();

            this.SelectedIndex = new ReactiveProperty<int>(-1);

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.Status.Value = null;
                this.ScreenName.Value = "";
                this.Model.UserCollections.Clear();
            });

            this.UpdateCommand = new ReactiveCommand();
            this.UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateUserCollections();
            });

            this.UserCollectionsIncrementalLoadCommand = new ReactiveCommand();
            this.UserCollectionsIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.UserCollections.Count > 0)
                    await this.Model.UpdateUserCollections(true);
            });

            this.AddStatusToCollectionCommand = new ReactiveCommand();
            this.AddStatusToCollectionCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Status.Value == null)
                    return;

                if (this.SelectedIndex.Value == -1)
                    return;

                var msgNotification = new ConfirmMessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_AddToCollection"), Title = "Confirmation" };
                await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                if (!msgNotification.Result)
                    return;

                await this.Model.AddStatusToCollection(this.Model.UserCollections[this.SelectedIndex.Value].Id, this.Status.Value.Id);
            });

            this.UserCollections = this.Model.UserCollections.ToReadOnlyReactiveCollection(x => new CollectionViewModel(x));

            this.UpdatingUserCollections = this.Model.ObserveProperty(x => x.UpdatingUserCollections).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;
        }

        public AddStatusToCollectionSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> UpdatingUserCollections { get; set; }

        public ReactiveProperty<CoreTweet.Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<string> ScreenName { get; set; }
        
        public ReactiveProperty<Status> Status { get; set; }

        public ReadOnlyReactiveCollection<CollectionViewModel> UserCollections { get; private set; }

        public ReactiveProperty<int> SelectedIndex { get; set; }
        

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand UserCollectionsIncrementalLoadCommand { get; set; }

        public ReactiveCommand AddStatusToCollectionCommand { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
