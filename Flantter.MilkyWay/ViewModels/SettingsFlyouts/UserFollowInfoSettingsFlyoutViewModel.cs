using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
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
    public class UserFollowInfoSettingsFlyoutViewModel
    {
        public UserFollowInfoSettingsFlyoutViewModel()
        {
            this.Model = new UserFollowInfoSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.PivotSelectedIndex = new ReactiveProperty<int>(0);
            this.PivotSelectedIndex.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                switch (x)
                {
                    case 1:
                        if (!this.Model.OpenFollowers)
                        {
                            await this.Model.UpdateFollowers();
                            this.Model.OpenFollowers = true;
                        }
                        break;
                    case 2:
                        if (!this.Model.OpenCrush)
                        {
                            await this.Model.UpdateCrush();
                            this.Model.OpenCrush = true;
                        }
                        break;
                    case 3:
                        if (!this.Model.OpenCrushedOn)
                        {
                            await this.Model.UpdateCrushedOn();
                            this.Model.OpenCrushedOn = true;
                        }
                        break;
                    case 4:
                        if (!this.Model.OpenBlock)
                        {
                            await this.Model.UpdateBlock();
                            this.Model.OpenBlock = true;
                        }
                        break;
                    case 5:
                        if (!this.Model.OpenMute)
                        {
                            await this.Model.UpdateMute();
                            this.Model.OpenMute = true;
                        }
                        break;
                }
            });

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.PivotSelectedIndex.Value = 0;

                this.Model.OpenFollowers = false;
                this.Model.OpenCrush = false;
                this.Model.OpenCrushedOn = false;
                this.Model.OpenBlock = false;
                this.Model.OpenMute = false;

                this.Model.GetIds = false;
                this.Model.FollowingIds.Clear();
                this.Model.FollowersIds.Clear();
                this.Model.CrushIds.Clear();
                this.Model.CrushedOnIds.Clear();

                this.Model.Following.Clear();
                this.Model.Followers.Clear();
                this.Model.Crush.Clear();
                this.Model.CrushedOn.Clear();
                this.Model.Block.Clear();
                this.Model.Mute.Clear();
            });

            this.UpdateCommand = new ReactiveCommand();
            this.UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateFollowing();
            });

            this.FollowingIncrementalLoadCommand = new ReactiveCommand();
            this.FollowingIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateFollowing(true);
            });

            this.FollowersIncrementalLoadCommand = new ReactiveCommand();
            this.FollowersIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateFollowers(true);
            });

            this.CrushIncrementalLoadCommand = new ReactiveCommand();
            this.CrushIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateCrush(true);
            });

            this.CrushedOnIncrementalLoadCommand = new ReactiveCommand();
            this.CrushedOnIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateCrushedOn(true);
            });

            this.BlockIncrementalLoadCommand = new ReactiveCommand();
            this.BlockIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateBlock(true);
            });

            this.MuteIncrementalLoadCommand = new ReactiveCommand();
            this.MuteIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateMute(true);
            });

            this.Following = this.Model.Following.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            this.Followers = this.Model.Followers.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            this.Crush = this.Model.Crush.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            this.CrushedOn = this.Model.CrushedOn.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            this.Block = this.Model.Block.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            this.Mute = this.Model.Mute.ToReadOnlyReactiveCollection(x => new UserViewModel(x));

            this.UpdatingFollowing = this.Model.ObserveProperty(x => x.UpdatingFollowing).ToReactiveProperty();
            this.UpdatingFollowers = this.Model.ObserveProperty(x => x.UpdatingFollowers).ToReactiveProperty();
            this.UpdatingCrush = this.Model.ObserveProperty(x => x.UpdatingCrush).ToReactiveProperty();
            this.UpdatingCrushedOn = this.Model.ObserveProperty(x => x.UpdatingCrushedOn).ToReactiveProperty();
            this.UpdatingBlock = this.Model.ObserveProperty(x => x.UpdatingBlock).ToReactiveProperty();
            this.UpdatingMute = this.Model.ObserveProperty(x => x.UpdatingMute).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;
        }

        public UserFollowInfoSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<bool> UpdatingFollowing { get; set; }

        public ReactiveProperty<bool> UpdatingFollowers { get; set; }

        public ReactiveProperty<bool> UpdatingCrush { get; set; }

        public ReactiveProperty<bool> UpdatingCrushedOn { get; set; }

        public ReactiveProperty<bool> UpdatingBlock { get; set; }

        public ReactiveProperty<bool> UpdatingMute { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<int> PivotSelectedIndex { get; set; }

        public ReadOnlyReactiveCollection<UserViewModel> Following { get; private set; }

        public ReadOnlyReactiveCollection<UserViewModel> Followers { get; private set; }

        public ReadOnlyReactiveCollection<UserViewModel> Crush { get; private set; }

        public ReadOnlyReactiveCollection<UserViewModel> CrushedOn { get; private set; }

        public ReadOnlyReactiveCollection<UserViewModel> Block { get; private set; }

        public ReadOnlyReactiveCollection<UserViewModel> Mute { get; private set; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand FollowingIncrementalLoadCommand { get; set; }

        public ReactiveCommand FollowersIncrementalLoadCommand { get; set; }
        
        public ReactiveCommand CrushIncrementalLoadCommand { get; set; }

        public ReactiveCommand CrushedOnIncrementalLoadCommand { get; set; }

        public ReactiveCommand BlockIncrementalLoadCommand { get; set; }

        public ReactiveCommand MuteIncrementalLoadCommand { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
