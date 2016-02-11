using CoreTweet;
using CoreTweet.Core;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRTXamlToolkit.Async;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class UserFollowInfoSettingsFlyoutModel : BindableBase
    {
        public UserFollowInfoSettingsFlyoutModel()
        {
            this.IdsAsyncLock = new AsyncLock();

            this.Following = new ObservableCollection<Twitter.Objects.User>();
            this.Followers = new ObservableCollection<Twitter.Objects.User>();
            this.Crush = new ObservableCollection<Twitter.Objects.User>();
            this.CrushedOn = new ObservableCollection<Twitter.Objects.User>();
            this.Block = new ObservableCollection<Twitter.Objects.User>();
            this.Mute = new ObservableCollection<Twitter.Objects.User>();
        }

        #region Tokens変更通知プロパティ
        private CoreTweet.Tokens _Tokens;
        public CoreTweet.Tokens Tokens
        {
            get { return this._Tokens; }
            set { this.SetProperty(ref this._Tokens, value); }
        }
        #endregion
        
        public bool OpenFollowers { get; set; }

        public bool OpenCrush { get; set; }

        public bool OpenCrushedOn { get; set; }

        public bool OpenBlock { get; set; }

        public bool OpenMute { get; set; }

        #region UpdatingFollowing変更通知プロパティ
        private bool _UpdatingFollowing;
        public bool UpdatingFollowing
        {
            get { return this._UpdatingFollowing; }
            set { this.SetProperty(ref this._UpdatingFollowing, value); }
        }
        #endregion

        #region UpdatingFollowers変更通知プロパティ
        private bool _UpdatingFollowers;
        public bool UpdatingFollowers
        {
            get { return this._UpdatingFollowers; }
            set { this.SetProperty(ref this._UpdatingFollowers, value); }
        }
        #endregion

        #region UpdatingCrush変更通知プロパティ
        private bool _UpdatingCrush;
        public bool UpdatingCrush
        {
            get { return this._UpdatingCrush; }
            set { this.SetProperty(ref this._UpdatingCrush, value); }
        }
        #endregion

        #region UpdatingCrushedOn変更通知プロパティ
        private bool _UpdatingCrushedOn;
        public bool UpdatingCrushedOn
        {
            get { return this._UpdatingCrushedOn; }
            set { this.SetProperty(ref this._UpdatingCrushedOn, value); }
        }
        #endregion

        #region UpdatingBlock変更通知プロパティ
        private bool _UpdatingBlock;
        public bool UpdatingBlock
        {
            get { return this._UpdatingBlock; }
            set { this.SetProperty(ref this._UpdatingBlock, value); }
        }
        #endregion

        #region UpdatingMute変更通知プロパティ
        private bool _UpdatingMute;
        public bool UpdatingMute
        {
            get { return this._UpdatingMute; }
            set { this.SetProperty(ref this._UpdatingMute, value); }
        }
        #endregion

        public ObservableCollection<Twitter.Objects.User> Following { get; set; }

        public ObservableCollection<Twitter.Objects.User> Followers { get; set; }

        public ObservableCollection<Twitter.Objects.User> Crush { get; set; }

        public ObservableCollection<Twitter.Objects.User> CrushedOn { get; set; }

        public ObservableCollection<Twitter.Objects.User> Block { get; set; }

        public ObservableCollection<Twitter.Objects.User> Mute { get; set; }

        private long followingCursor = 0;
        public async Task UpdateFollowing(bool useCursor = false)
        {
            if (this.UpdatingFollowing)
                return;

            if (this.Tokens == null)
                return;

            if (useCursor && followingCursor == 0)
                return;

            this.UpdatingFollowing = true;

            if (!useCursor || followingCursor == 0)
                this.Following.Clear();

            Cursored<User> following;
            try
            {
                if (useCursor && followingCursor != 0)
                    following = await Tokens.Friends.ListAsync(user_id => Tokens.UserId, count => 20, cursor => followingCursor);
                else
                    following = await Tokens.Friends.ListAsync(user_id => Tokens.UserId, count => 20);
            }
            catch
            {
                if (!useCursor || followingCursor == 0)
                    this.Following.Clear();

                this.UpdatingFollowing = false;
                return;
            }

            if (!useCursor || followingCursor == 0)
                this.Following.Clear();

            foreach (var item in following)
            {
                var user = new Twitter.Objects.User(item);
                this.Following.Add(user);
            }

            followingCursor = following.NextCursor;

            this.UpdatingFollowing = false;
        }

        private long followersCursor = 0;
        public async Task UpdateFollowers(bool useCursor = false)
        {
            if (this.UpdatingFollowers)
                return;

            if (this.Tokens == null)
                return;

            if (useCursor && followersCursor == 0)
                return;

            this.UpdatingFollowers = true;

            if (!useCursor || followersCursor == 0)
                this.Followers.Clear();

            Cursored<CoreTweet.User> followers;
            try
            {
                if (useCursor && followersCursor != 0)
                    followers = await Tokens.Followers.ListAsync(user_id => Tokens.UserId, count => 20, cursor => followersCursor);
                else
                    followers = await Tokens.Followers.ListAsync(user_id => Tokens.UserId, count => 20);
            }
            catch
            {
                if (!useCursor || followersCursor == 0)
                    this.Followers.Clear();

                this.UpdatingFollowers = false;
                return;
            }

            if (!useCursor || followersCursor == 0)
                this.Followers.Clear();

            foreach (var item in followers)
            {
                var user = new Twitter.Objects.User(item);
                this.Followers.Add(user);
            }

            followersCursor = followers.NextCursor;
            this.UpdatingFollowers = false;
        }
        
        private long blockCursor = 0;
        public async Task UpdateBlock(bool useCursor = false)
        {
            if (this.UpdatingBlock)
                return;

            if (this.Tokens == null)
                return;

            if (useCursor && blockCursor == 0)
                return;

            this.UpdatingBlock = true;

            if (!useCursor || blockCursor == 0)
                this.Block.Clear();

            Cursored<CoreTweet.User> block;
            try
            {
                if (useCursor && blockCursor != 0)
                    block = await Tokens.Blocks.ListAsync(user_id => Tokens.UserId, count => 20, cursor => blockCursor);
                else
                    block = await Tokens.Blocks.ListAsync(user_id => Tokens.UserId, count => 20);
            }
            catch
            {
                if (!useCursor || blockCursor == 0)
                    this.Block.Clear();

                this.UpdatingBlock = false;
                return;
            }

            if (!useCursor || blockCursor == 0)
                this.Block.Clear();

            foreach (var item in block)
            {
                var user = new Twitter.Objects.User(item);
                this.Block.Add(user);
            }

            blockCursor = block.NextCursor;
            this.UpdatingBlock = false;
        }
        
        private long muteCursor = 0;
        public async Task UpdateMute(bool useCursor = false)
        {
            if (this.UpdatingMute)
                return;

            if (this.Tokens == null)
                return;

            if (useCursor && muteCursor == 0)
                return;

            this.UpdatingMute = true;

            if (!useCursor || muteCursor == 0)
                this.Mute.Clear();

            Cursored<User> mute;
            try
            {
                if (useCursor && muteCursor != 0)
                    mute = await Tokens.Mutes.Users.ListAsync(user_id => Tokens.UserId, count => 20, cursor => muteCursor);
                else
                    mute = await Tokens.Mutes.Users.ListAsync(user_id => Tokens.UserId, count => 20);
            }
            catch
            {
                if (!useCursor || muteCursor == 0)
                    this.Mute.Clear();

                this.UpdatingMute = false;
                return;
            }

            if (!useCursor || muteCursor == 0)
                this.Mute.Clear();

            foreach (var item in mute)
            {
                var user = new Twitter.Objects.User(item);
                this.Mute.Add(user);
            }

            muteCursor = mute.NextCursor;

            this.UpdatingMute = false;
        }

        
        public AsyncLock IdsAsyncLock { get; set; }
        public bool GetIds { get; set; }
        public List<long> FollowingIds { get; set; } = new List<long>();
        public List<long> FollowersIds { get; set; } = new List<long>();
        public List<long> CrushIds { get; set; } = new List<long>();
        public List<long> CrushedOnIds { get; set; } = new List<long>();

        private long crushCursor = 0;
        public async Task UpdateCrush(bool useCursor = false)
        {
            if (this.UpdatingCrush)
                return;

            if (this.Tokens == null)
                return;

            if (useCursor && crushCursor == 0)
                return;

            this.UpdatingCrush = true;

            if (!useCursor || crushCursor == 0)
                this.Crush.Clear();

            if (!GetIds)
                await this.UpdateFriendShipIds();

            if (!GetIds)
            {
                this.UpdatingCrush = true;
                return;
            }

            using (await this.IdsAsyncLock.LockAsync())
            {
                var ids = new List<long>();
                for (var idsCount = 0; crushCursor < this.CrushIds.Count && idsCount <= 99; crushCursor++, idsCount++)
                    ids.Add(this.CrushIds[(int)crushCursor]);
                
                ListedResponse<User> crush;
                try
                {
                    crush = await Tokens.Users.LookupAsync(user_id => ids.AsEnumerable());
                }
                catch
                {
                    if (!useCursor || crushCursor == 0)
                        this.Crush.Clear();

                    this.UpdatingCrush = false;
                    return;
                }

                if (!useCursor || crushCursor == 0)
                    this.Crush.Clear();

                foreach (var item in crush)
                {
                    var user = new Twitter.Objects.User(item);
                    this.Crush.Add(user);
                }
            }

            if (crushCursor == this.CrushIds.Count)
                crushCursor = 0;

            this.UpdatingCrush = false;
        }

        private long crushedOnCursor = 0;
        public async Task UpdateCrushedOn(bool useCursor = false)
        {
            if (this.UpdatingCrushedOn)
                return;

            if (this.Tokens == null)
                return;

            if (useCursor && crushedOnCursor == 0)
                return;

            this.UpdatingCrushedOn = true;

            if (!useCursor || crushedOnCursor == 0)
                this.CrushedOn.Clear();

            if (!GetIds)
                await this.UpdateFriendShipIds();

            if (!GetIds)
            {
                this.UpdatingCrushedOn = true;
                return;
            }

            using (await this.IdsAsyncLock.LockAsync())
            {
                var ids = new List<long>();
                for (var idsCount = 0; crushedOnCursor < this.CrushedOnIds.Count && idsCount <= 99; crushedOnCursor++, idsCount++)
                    ids.Add(this.CrushedOnIds[(int)crushedOnCursor]);

                ListedResponse<User> CrushedOn;
                try
                {
                    CrushedOn = await Tokens.Users.LookupAsync(user_id => ids.AsEnumerable());
                }
                catch
                {
                    if (!useCursor || crushedOnCursor == 0)
                        this.CrushedOn.Clear();

                    this.UpdatingCrushedOn = false;
                    return;
                }

                if (!useCursor || crushedOnCursor == 0)
                    this.CrushedOn.Clear();

                foreach (var item in CrushedOn)
                {
                    var user = new Twitter.Objects.User(item);
                    this.CrushedOn.Add(user);
                }
            }

            if (crushedOnCursor == this.CrushedOnIds.Count)
                crushedOnCursor = 0;

            this.UpdatingCrushedOn = false;
        }


        public async Task UpdateFriendShipIds()
        {
            using (await this.IdsAsyncLock.LockAsync())
            {
                if (this.GetIds)
                    return;

                Cursored<long> userFollowingIds;
                Cursored<long> userFollowerIds;
                long idsCursor = 0;

                try
                {
                    while (true)
                    {
                        if (idsCursor == 0)
                            userFollowingIds = await Tokens.Friends.IdsAsync(user_id => Tokens.UserId, count => 5000);
                        else
                            userFollowingIds = await Tokens.Friends.IdsAsync(user_id => Tokens.UserId, cursor => idsCursor, count => 5000);

                        foreach (var id in userFollowingIds)
                            this.FollowingIds.Add(id);

                        if (userFollowingIds.NextCursor == 0)
                            break;

                        idsCursor = userFollowingIds.NextCursor;
                    }
                }
                catch
                {
                    return;
                }

                try
                {
                    while (true)
                    {
                        if (idsCursor == 0)
                            userFollowerIds = await Tokens.Followers.IdsAsync(user_id => Tokens.UserId, count => 5000);
                        else
                            userFollowerIds = await Tokens.Followers.IdsAsync(user_id => Tokens.UserId, cursor => idsCursor, count => 5000);

                        foreach (var id in userFollowerIds)
                            this.FollowersIds.Add(id);

                        if (userFollowerIds.NextCursor == 0)
                            break;

                        idsCursor = userFollowerIds.NextCursor;
                    }

                }
                catch
                {
                    return;
                }

                this.CrushIds = this.FollowingIds.Except(this.FollowersIds).ToList();
                this.CrushedOnIds = this.FollowersIds.Except(this.FollowingIds).ToList();

                this.GetIds = true;
            }
        }
    }
}
