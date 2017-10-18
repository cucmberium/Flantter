using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.ViewModels.Apis.Objects;
using Flantter.MilkyWay.ViewModels.Services;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class UserFollowInfoSettingsFlyoutViewModel
    {
        public UserFollowInfoSettingsFlyoutViewModel()
        {
            Model = new UserFollowInfoSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            PivotSelectedIndex = new ReactiveProperty<int>(0);
            PivotSelectedIndex.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    switch (x)
                    {
                        case 1:
                            if (!Model.OpenFollowers)
                            {
                                await Model.UpdateFollowers();
                                Model.OpenFollowers = true;
                            }
                            break;
                        case 2:
                            if (!Model.OpenCrush)
                            {
                                await Model.UpdateCrush();
                                Model.OpenCrush = true;
                            }
                            break;
                        case 3:
                            if (!Model.OpenCrushedOn)
                            {
                                await Model.UpdateCrushedOn();
                                Model.OpenCrushedOn = true;
                            }
                            break;
                        case 4:
                            if (!Model.OpenBlock)
                            {
                                await Model.UpdateBlock();
                                Model.OpenBlock = true;
                            }
                            break;
                        case 5:
                            if (!Model.OpenMute)
                            {
                                await Model.UpdateMute();
                                Model.OpenMute = true;
                            }
                            break;
                    }
                });

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    PivotSelectedIndex.Value = 0;

                    Model.OpenFollowers = false;
                    Model.OpenCrush = false;
                    Model.OpenCrushedOn = false;
                    Model.OpenBlock = false;
                    Model.OpenMute = false;

                    Model.GetIds = false;
                    Model.FollowingIds.Clear();
                    Model.FollowersIds.Clear();
                    Model.CrushIds.Clear();
                    Model.CrushedOnIds.Clear();

                    Model.Following.Clear();
                    Model.Followers.Clear();
                    Model.Crush.Clear();
                    Model.CrushedOn.Clear();
                    Model.Block.Clear();
                    Model.Mute.Clear();
                });

            UpdateCommand = new ReactiveCommand();
            UpdateCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateFollowing(); });

            FollowingIncrementalLoadCommand = new ReactiveCommand();
            FollowingIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateFollowing(true); });

            FollowersIncrementalLoadCommand = new ReactiveCommand();
            FollowersIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateFollowers(true); });

            CrushIncrementalLoadCommand = new ReactiveCommand();
            CrushIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateCrush(true); });

            CrushedOnIncrementalLoadCommand = new ReactiveCommand();
            CrushedOnIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateCrushedOn(true); });

            BlockIncrementalLoadCommand = new ReactiveCommand();
            BlockIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateBlock(true); });

            MuteIncrementalLoadCommand = new ReactiveCommand();
            MuteIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateMute(true); });

            Following = Model.Following.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            Followers = Model.Followers.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            Crush = Model.Crush.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            CrushedOn = Model.CrushedOn.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            Block = Model.Block.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            Mute = Model.Mute.ToReadOnlyReactiveCollection(x => new UserViewModel(x));

            UpdatingFollowing = Model.ObserveProperty(x => x.UpdatingFollowing).ToReactiveProperty();
            UpdatingFollowers = Model.ObserveProperty(x => x.UpdatingFollowers).ToReactiveProperty();
            UpdatingCrush = Model.ObserveProperty(x => x.UpdatingCrush).ToReactiveProperty();
            UpdatingCrushedOn = Model.ObserveProperty(x => x.UpdatingCrushedOn).ToReactiveProperty();
            UpdatingBlock = Model.ObserveProperty(x => x.UpdatingBlock).ToReactiveProperty();
            UpdatingMute = Model.ObserveProperty(x => x.UpdatingMute).ToReactiveProperty();

            Notice = Notice.Instance;
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

        public ReadOnlyReactiveCollection<UserViewModel> Following { get; }

        public ReadOnlyReactiveCollection<UserViewModel> Followers { get; }

        public ReadOnlyReactiveCollection<UserViewModel> Crush { get; }

        public ReadOnlyReactiveCollection<UserViewModel> CrushedOn { get; }

        public ReadOnlyReactiveCollection<UserViewModel> Block { get; }

        public ReadOnlyReactiveCollection<UserViewModel> Mute { get; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateCommand { get; set; }

        public ReactiveCommand FollowingIncrementalLoadCommand { get; set; }

        public ReactiveCommand FollowersIncrementalLoadCommand { get; set; }

        public ReactiveCommand CrushIncrementalLoadCommand { get; set; }

        public ReactiveCommand CrushedOnIncrementalLoadCommand { get; set; }

        public ReactiveCommand BlockIncrementalLoadCommand { get; set; }

        public ReactiveCommand MuteIncrementalLoadCommand { get; set; }

        public Notice Notice { get; set; }
    }
}