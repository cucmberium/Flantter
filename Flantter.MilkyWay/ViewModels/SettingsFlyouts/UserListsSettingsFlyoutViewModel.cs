using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class UserListsSettingsFlyoutViewModel
    {
        public UserListsSettingsFlyoutViewModel()
        {
            Model = new UserListsSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            ScreenName = Model.ToReactivePropertyAsSynchronized(x => x.ScreenName);

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

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    ScreenName.Value = "";
                    PivotSelectedIndex.Value = 0;

                    Model.OpenSubscribeLists = false;
                    Model.OpenMembershipLists = false;

                    Model.UserLists.Clear();
                    Model.SubscribeLists.Clear();
                    Model.MembershipLists.Clear();
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

            UserLists = Model.UserLists.ToReadOnlyReactiveCollection(x => new ListViewModel(x));
            SubscribeLists = Model.SubscribeLists.ToReadOnlyReactiveCollection(x => new ListViewModel(x));
            MembershipLists = Model.MembershipLists.ToReadOnlyReactiveCollection(x => new ListViewModel(x));

            Notice = Notice.Instance;
        }

        public UserListsSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<string> ScreenName { get; set; }

        public ReactiveProperty<int> PivotSelectedIndex { get; set; }

        public ReactiveProperty<bool> UpdatingUserLists { get; set; }

        public ReactiveProperty<bool> UpdatingSubscribeLists { get; set; }

        public ReactiveProperty<bool> UpdatingMembershipLists { get; set; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand UserListsIncrementalLoadCommand { get; set; }

        public ReactiveCommand SubscribeListsIncrementalLoadCommand { get; set; }

        public ReactiveCommand MembershipListsIncrementalLoadCommand { get; set; }

        public ReadOnlyReactiveCollection<ListViewModel> UserLists { get; }

        public ReadOnlyReactiveCollection<ListViewModel> SubscribeLists { get; }

        public ReadOnlyReactiveCollection<ListViewModel> MembershipLists { get; }

        public Notice Notice { get; set; }
    }
}