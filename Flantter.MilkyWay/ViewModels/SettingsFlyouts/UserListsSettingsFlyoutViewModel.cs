using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.ViewModels.Apis.Objects;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Windows.ApplicationModel.Resources;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class UserListsSettingsFlyoutViewModel
    {
        private readonly ResourceLoader _resourceLoader;

        public UserListsSettingsFlyoutViewModel()
        {
            _resourceLoader = new ResourceLoader();

            Model = new UserListsSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");
            UserId = Model.ToReactivePropertyAsSynchronized(x => x.UserId);

            CreateListMenuOpen = new ReactiveProperty<bool>();
            UpdateListMenuOpen = new ReactiveProperty<bool>();
            EditingListMenuOpen = new ReactiveProperty<bool>();

            EditingListName = new ReactiveProperty<string>();
            EditingListDescription = new ReactiveProperty<string>();
            EditingListIsPrivate = new ReactiveProperty<bool>();
            EditingListId = new ReactiveProperty<long>();

            UserListsSelectedIndex = new ReactiveProperty<int>(-1);
            UpdateListButtonIsEnabled = UserListsSelectedIndex.Select(x => x != -1).ToReactiveProperty();

            PivotSelectedIndex = new ReactiveProperty<int>(0);
            PivotSelectedIndex.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    switch (x)
                    {
                        case 1:
                            if (!Model.OpenSubscribeLists)
                            {
                                await Model.UpdateSubscribeLists();
                                Model.OpenSubscribeLists = true;
                            }
                            break;
                        case 2:
                            if (!Model.OpenMembershipLists)
                            {
                                await Model.UpdateMembershipLists();
                                Model.OpenMembershipLists = true;
                            }
                            break;
                    }
                });

            UpdatingUserLists = Model.ObserveProperty(x => x.UpdatingUserLists).ToReactiveProperty();
            UpdatingSubscribeLists = Model.ObserveProperty(x => x.UpdatingSubscribeLists).ToReactiveProperty();
            UpdatingMembershipLists = Model.ObserveProperty(x => x.UpdatingMembershipLists).ToReactiveProperty();
            CreatingOrUpdatingList = Model.ObserveProperty(x => x.CreatingOrUpdatingList).ToReactiveProperty();

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    PivotSelectedIndex.Value = 0;

                    Model.UserId = 0;

                    Model.OpenSubscribeLists = false;
                    Model.OpenMembershipLists = false;

                    Model.UserLists.Clear();
                    Model.SubscribeLists.Clear();
                    Model.MembershipLists.Clear();

                    UpdateListMenuOpen.Value = false;
                    CreateListMenuOpen.Value = false;
                    EditingListMenuOpen.Value = false;
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

            SubscribeListsIncrementalLoadCommand = new ReactiveCommand();
            SubscribeListsIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.SubscribeLists.Count > 0)
                        await Model.UpdateSubscribeLists(true);
                });

            MembershipListsIncrementalLoadCommand = new ReactiveCommand();
            MembershipListsIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.MembershipLists.Count > 0)
                        await Model.UpdateMembershipLists(true);
                });

            OpenCreateListMenuCommand = new ReactiveCommand();
            OpenCreateListMenuCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    EditingListName.Value = "";
                    EditingListDescription.Value = "";
                    EditingListIsPrivate.Value = false;
                    EditingListId.Value = 0;

                    EditingListMenuOpen.Value = false;
                    UpdateListMenuOpen.Value = false;
                    CreateListMenuOpen.Value = true;
                });

            CloseCreateListMenuCommand = new ReactiveCommand();
            CloseCreateListMenuCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    EditingListName.Value = "";
                    EditingListDescription.Value = "";
                    EditingListIsPrivate.Value = false;
                    EditingListId.Value = 0;

                    EditingListMenuOpen.Value = true;
                    UpdateListMenuOpen.Value = false;
                    CreateListMenuOpen.Value = false;
                });

            OpenUpdateListMenuCommand = new ReactiveCommand();
            OpenUpdateListMenuCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (UserListsSelectedIndex.Value == -1)
                        return;

                    var list = Model.UserLists.ElementAt(UserListsSelectedIndex.Value);
                    EditingListName.Value = list.Name;
                    EditingListDescription.Value = list.Description;
                    EditingListIsPrivate.Value = list.Mode == "private";
                    EditingListId.Value = list.Id;

                    EditingListMenuOpen.Value = false;
                    UpdateListMenuOpen.Value = true;
                    CreateListMenuOpen.Value = false;
                });

            CloseUpdateListMenuCommand = new ReactiveCommand();
            CloseUpdateListMenuCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    EditingListName.Value = "";
                    EditingListDescription.Value = "";
                    EditingListIsPrivate.Value = false;
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

                    var result = await Model.CreateList(EditingListName.Value, EditingListDescription.Value, EditingListIsPrivate.Value ? "private" : "public");

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

                    var result = await Model.UpdateList(EditingListId.Value, EditingListName.Value, EditingListDescription.Value, EditingListIsPrivate.Value ? "private" : "public");

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
            SubscribeLists = Model.SubscribeLists.ToReadOnlyReactiveCollection(x => new ListViewModel(x));
            MembershipLists = Model.MembershipLists.ToReadOnlyReactiveCollection(x => new ListViewModel(x));

            Notice = Notice.Instance;
        }

        public UserListsSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<long> UserId { get; set; }

        public ReactiveProperty<int> PivotSelectedIndex { get; set; }

        public ReactiveProperty<bool> UpdatingUserLists { get; set; }

        public ReactiveProperty<bool> UpdatingSubscribeLists { get; set; }

        public ReactiveProperty<bool> UpdatingMembershipLists { get; set; }

        public ReactiveProperty<bool> CreatingOrUpdatingList { get; set; }

        public ReactiveProperty<bool> UpdateListButtonIsEnabled { get; set; }

        public ReactiveProperty<bool> CreateListMenuOpen { get; set; }

        public ReactiveProperty<bool> UpdateListMenuOpen { get; set; }

        public ReactiveProperty<bool> EditingListMenuOpen { get; set; }

        public ReactiveProperty<string> EditingListName { get; set; }

        public ReactiveProperty<string> EditingListDescription { get; set; }

        public ReactiveProperty<bool> EditingListIsPrivate { get; set; }

        public ReactiveProperty<long> EditingListId { get; set; }

        public ReactiveProperty<int> UserListsSelectedIndex { get; set; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand UserListsIncrementalLoadCommand { get; set; }

        public ReactiveCommand SubscribeListsIncrementalLoadCommand { get; set; }

        public ReactiveCommand MembershipListsIncrementalLoadCommand { get; set; }

        public ReactiveCommand OpenCreateListMenuCommand { get; set; }

        public ReactiveCommand CloseCreateListMenuCommand { get; set; }

        public ReactiveCommand OpenUpdateListMenuCommand { get; set; }

        public ReactiveCommand CloseUpdateListMenuCommand { get; set; }

        public ReactiveCommand CreateListCommand { get; set; }

        public ReactiveCommand UpdateListCommand { get; set; }

        public ReactiveCommand DeleteListCommand { get; set; }

        public ReadOnlyReactiveCollection<ListViewModel> UserLists { get; }

        public ReadOnlyReactiveCollection<ListViewModel> SubscribeLists { get; }

        public ReadOnlyReactiveCollection<ListViewModel> MembershipLists { get; }

        public Notice Notice { get; set; }
    }
}