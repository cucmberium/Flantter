using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class UserListsSettingsFlyoutViewModel
    {
        public UserListsSettingsFlyoutViewModel()
        {
            this.Model = new UserListsSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.ScreenName = this.Model.ToReactivePropertyAsSynchronized(x => x.ScreenName);

            this.PivotSelectedIndex = new ReactiveProperty<int>(0);
            this.PivotSelectedIndex.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                switch (x)
                {
                    case 1:
                        if (!this.Model.OpenSubscribeLists)
                        {
                            await this.Model.UpdateSubscribeLists();
                            this.Model.OpenSubscribeLists = true;
                        }
                        break;
                    case 2:
                        if (!this.Model.OpenMembershipLists)
                        {
                            await this.Model.UpdateMembershipLists();
                            this.Model.OpenMembershipLists = true;
                        }
                        break;
                }
            });

            this.UpdatingUserLists = this.Model.ObserveProperty(x => x.UpdatingUserLists).ToReactiveProperty();
            this.UpdatingSubscribeLists = this.Model.ObserveProperty(x => x.UpdatingSubscribeLists).ToReactiveProperty();
            this.UpdatingMembershipLists = this.Model.ObserveProperty(x => x.UpdatingMembershipLists).ToReactiveProperty();

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.ScreenName.Value = "";
                this.PivotSelectedIndex.Value = 0;

                this.Model.OpenSubscribeLists = false;
                this.Model.OpenMembershipLists = false;

                this.Model.UserLists.Clear();
                this.Model.SubscribeLists.Clear();
                this.Model.MembershipLists.Clear();
            });

            this.UpdateCommand = new ReactiveCommand();
            this.UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateUserLists();
            });

            this.UserListsIncrementalLoadCommand = new ReactiveCommand();
            this.UserListsIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.UserLists.Count > 0)
                    await this.Model.UpdateUserLists(true);
            });

            this.SubscribeListsIncrementalLoadCommand = new ReactiveCommand();
            this.SubscribeListsIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.SubscribeLists.Count > 0)
                    await this.Model.UpdateSubscribeLists(true);
            });

            this.MembershipListsIncrementalLoadCommand = new ReactiveCommand();
            this.MembershipListsIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.MembershipLists.Count > 0)
                    await this.Model.UpdateMembershipLists(true);
            });

            this.UserLists = this.Model.UserLists.ToReadOnlyReactiveCollection(x => new ListViewModel(x));
            this.SubscribeLists = this.Model.SubscribeLists.ToReadOnlyReactiveCollection(x => new ListViewModel(x));
            this.MembershipLists = this.Model.MembershipLists.ToReadOnlyReactiveCollection(x => new ListViewModel(x));

            this.Notice = Services.Notice.Instance;
        }

        public UserListsSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<CoreTweet.Tokens> Tokens { get; set; }

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

        public ReadOnlyReactiveCollection<ListViewModel> UserLists { get; private set; }

        public ReadOnlyReactiveCollection<ListViewModel> SubscribeLists { get; private set; }

        public ReadOnlyReactiveCollection<ListViewModel> MembershipLists { get; private set; }

        public Services.Notice Notice { get; set; }
    }
}
