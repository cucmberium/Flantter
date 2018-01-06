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
    public class MastodonUserListsSettingsFlyoutViewModel
    {
        private readonly ResourceLoader _resourceLoader;

        public MastodonUserListsSettingsFlyoutViewModel()
        {
            _resourceLoader = new ResourceLoader();

            Model = new MastodonUserListsSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");
            UserId = Model.ToReactivePropertyAsSynchronized(x => x.UserId);

            CreateListMenuOpen = new ReactiveProperty<bool>();
            UpdateListMenuOpen = new ReactiveProperty<bool>();
            EditingListMenuOpen = new ReactiveProperty<bool>();

            EditingListName = new ReactiveProperty<string>();
            EditingListId = new ReactiveProperty<long>();

            UserListsSelectedIndex = new ReactiveProperty<int>(-1);
            UpdateListButtonIsEnabled = UserListsSelectedIndex.Select(x => x != -1).ToReactiveProperty();

            UpdatingUserLists = Model.ObserveProperty(x => x.UpdatingUserLists).ToReactiveProperty();
            CreatingOrUpdatingList = Model.ObserveProperty(x => x.CreatingOrUpdatingList).ToReactiveProperty();

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    UpdateListMenuOpen.Value = false;
                    CreateListMenuOpen.Value = false;
                    EditingListMenuOpen.Value = false;

                    Model.UserLists.Clear();
                });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    await Model.UpdateUserLists();

                    if (Tokens.Value.UserId == UserId.Value)
                        EditingListMenuOpen.Value = true;
                });

            UserListsIncrementalLoadCommand = new ReactiveCommand();
            UserListsIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.UserLists.Count > 0)
                        await Model.UpdateUserLists(true);
                });

            OpenCreateListCommand = new ReactiveCommand();
            OpenCreateListCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    EditingListName.Value = "";
                    EditingListId.Value = 0;

                    EditingListMenuOpen.Value = false;
                    UpdateListMenuOpen.Value = false;
                    CreateListMenuOpen.Value = true;
                });

            CloseCreateListCommand = new ReactiveCommand();
            CloseCreateListCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    EditingListName.Value = "";
                    EditingListId.Value = 0;

                    EditingListMenuOpen.Value = true;
                    UpdateListMenuOpen.Value = false;
                    CreateListMenuOpen.Value = false;
                });

            OpenUpdateListCommand = new ReactiveCommand();
            OpenUpdateListCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (UserListsSelectedIndex.Value == -1)
                        return;

                    var list = Model.UserLists.ElementAt(UserListsSelectedIndex.Value);
                    EditingListName.Value = list.Name;
                    EditingListId.Value = list.Id;

                    EditingListMenuOpen.Value = false;
                    UpdateListMenuOpen.Value = true;
                    CreateListMenuOpen.Value = false;
                });

            CloseUpdateListCommand = new ReactiveCommand();
            CloseUpdateListCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    EditingListName.Value = "";
                    EditingListId.Value = 0;

                    EditingListMenuOpen.Value = true;
                    UpdateListMenuOpen.Value = false;
                    CreateListMenuOpen.Value = false;
                });

            CreateListCommand = new ReactiveCommand();
            CreateListCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (EditingListId.Value != 0)
                        return;

                    var result = await Model.CreateList(EditingListName.Value);

                    if (!result)
                        return;

                    EditingListMenuOpen.Value = true;
                    UpdateListMenuOpen.Value = false;
                    CreateListMenuOpen.Value = false;
                    await Model.UpdateUserLists();
                });

            UpdateListCommand = new ReactiveCommand();
            UpdateListCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (EditingListId.Value == 0)
                        return;

                    var result = await Model.UpdateList(EditingListId.Value, EditingListName.Value);

                    if (!result)
                        return;

                    EditingListMenuOpen.Value = true;
                    UpdateListMenuOpen.Value = false;
                    CreateListMenuOpen.Value = false;
                    await Model.UpdateUserLists();
                });

            DeleteListCommand = new ReactiveCommand();
            DeleteListCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (UserListsSelectedIndex.Value == -1)
                        return;

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_DeleteList"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    var list = Model.UserLists.ElementAt(UserListsSelectedIndex.Value);
                    var result = await Model.DeleteList(list.Id);

                    if (!result)
                        return;

                    EditingListMenuOpen.Value = true;
                    UpdateListMenuOpen.Value = false;
                    CreateListMenuOpen.Value = false;
                    await Model.UpdateUserLists();
                });

            UserLists = Model.UserLists.ToReadOnlyReactiveCollection(x => new ListViewModel(x));

            Notice = Notice.Instance;
        }

        public MastodonUserListsSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> UpdatingUserLists { get; set; }

        public ReactiveProperty<bool> CreatingOrUpdatingList { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<long> UserId { get; set; }

        public ReadOnlyReactiveCollection<ListViewModel> UserLists { get; }

        public ReactiveProperty<int> UserListsSelectedIndex { get; set; }

        public ReactiveProperty<bool> UpdateListButtonIsEnabled { get; set; }

        public ReactiveProperty<bool> CreateListMenuOpen { get; set; }

        public ReactiveProperty<bool> UpdateListMenuOpen { get; set; }

        public ReactiveProperty<bool> EditingListMenuOpen { get; set; }
        
        public ReactiveProperty<string> EditingListName { get; set; }

        public ReactiveProperty<long> EditingListId { get; set; }
        
        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand UserListsIncrementalLoadCommand { get; set; }

        public ReactiveCommand OpenCreateListCommand { get; set; }

        public ReactiveCommand CloseCreateListCommand { get; set; }

        public ReactiveCommand OpenUpdateListCommand { get; set; }

        public ReactiveCommand CloseUpdateListCommand { get; set; }

        public ReactiveCommand CreateListCommand { get; set; }

        public ReactiveCommand UpdateListCommand { get; set; }

        public ReactiveCommand DeleteListCommand { get; set; }

        public Notice Notice { get; set; }
    }
}