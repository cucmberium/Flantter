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
    public class ListMembersSettingsFlyoutViewModel
    {
        private readonly ResourceLoader _resourceLoader;

        public ListMembersSettingsFlyoutViewModel()
        {
            _resourceLoader = new ResourceLoader();

            Model = new ListMembersSettingsFlyoutModel();
            Id = Model.ToReactivePropertyAsSynchronized(x => x.Id);
            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            IsMyList = new ReactiveProperty<bool>();

            ListMembersSelectedIndex = new ReactiveProperty<int>(-1);
            UpdateListButtonIsEnabled = ListMembersSelectedIndex.Select(x => x != -1).ToReactiveProperty();

            EditingListMenuOpen = new ReactiveProperty<bool>();

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                Model.ListMembers.Clear();

                EditingListMenuOpen.Value = false;
            });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    await Model.UpdateListMembers();

                    if (IsMyList.Value)
                        EditingListMenuOpen.Value = true;
                });

            ListMembersIncrementalLoadCommand = new ReactiveCommand();
            ListMembersIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateListMembers(true); });

            DeleteUserCommand = new ReactiveCommand();
            DeleteUserCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (ListMembersSelectedIndex.Value == -1)
                        return;

                    var msgNotification = new ConfirmMessageDialogNotification
                    {
                        Message = _resourceLoader.GetString("ConfirmDialog_DeleteList"),
                        Title = "Confirmation"
                    };
                    await Notice.ShowComfirmMessageDialogMessenger.Raise(msgNotification);

                    if (!msgNotification.Result)
                        return;

                    var user = Model.ListMembers.ElementAt(ListMembersSelectedIndex.Value);
                    var result = await Model.DeleteUser(user.Id);

                    if (!result)
                        return;

                    EditingListMenuOpen.Value = true;
                    await Model.UpdateListMembers();
                });

            ListMembers = Model.ListMembers.ToReadOnlyReactiveCollection(x => new UserViewModel(x));

            Updating = Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            Notice = Notice.Instance;
        }

        public ListMembersSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<long> Id { get; set; }

        public ReactiveProperty<bool> IsMyList { get; set; }

        public ReactiveProperty<int> ListMembersSelectedIndex { get; set; }

        public ReactiveProperty<bool> UpdateListButtonIsEnabled { get; set; }

        public ReactiveProperty<bool> EditingListMenuOpen { get; set; }

        public ReadOnlyReactiveCollection<UserViewModel> ListMembers { get; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand ListMembersIncrementalLoadCommand { get; set; }

        public ReactiveCommand DeleteUserCommand { get; set; }

        public Notice Notice { get; set; }
    }
}