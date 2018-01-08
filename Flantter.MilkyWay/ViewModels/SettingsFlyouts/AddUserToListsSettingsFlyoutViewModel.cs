using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.ViewModels.Apis.Objects;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class AddUserToListsSettingsFlyoutViewModel
    {
        private readonly ResourceLoader _resourceLoader;

        public AddUserToListsSettingsFlyoutViewModel()
        {
            _resourceLoader = new ResourceLoader();

            Model = new AddUserToListsSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            UserId = Model.ToReactivePropertyAsSynchronized(x => x.UserId);

            TargetUserId = new ReactiveProperty<long>();

            SelectedIndex = new ReactiveProperty<int>(-1);

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    TargetUserId.Value = 0;

                    Model.UserId = 0;
                    Model.UserLists.Clear();
                });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateUserLists(); });

            UserListsIncrementalLoadCommand = new ReactiveCommand();
            UserListsIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.UserLists.Count > 0)
                        await Model.UpdateUserLists(true);
                });

            AddUserToListCommand = new ReactiveCommand();
            AddUserToListCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (TargetUserId.Value == 0)
                        return;

                    if (SelectedIndex.Value == -1)
                        return;

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_AddToList"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    await Model.AddUserToList(Model.UserLists[SelectedIndex.Value].Id, TargetUserId.Value);
                });

            UserLists = Model.UserLists.ToReadOnlyReactiveCollection(x => new ListViewModel(x));

            UpdatingUserLists = Model.ObserveProperty(x => x.UpdatingUserLists).ToReactiveProperty();

            Notice = Notice.Instance;
        }

        public AddUserToListsSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> UpdatingUserLists { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<long> UserId { get; set; }

        public ReactiveProperty<long> TargetUserId { get; set; }

        public ReadOnlyReactiveCollection<ListViewModel> UserLists { get; }

        public ReactiveProperty<int> SelectedIndex { get; set; }


        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand UserListsIncrementalLoadCommand { get; set; }

        public ReactiveCommand AddUserToListCommand { get; set; }

        public Notice Notice { get; set; }
    }
}