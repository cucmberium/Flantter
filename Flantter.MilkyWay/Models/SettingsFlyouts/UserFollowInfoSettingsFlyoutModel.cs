using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
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
        private Tokens _Tokens;
        public Tokens Tokens
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
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"user_id", this.Tokens.UserId},
                    {"count", 20},
                };
                if (useCursor && followingCursor != 0)
                    param.Add("cursor", followingCursor);

                var following = await Tokens.Friends.ListAsync(param);
                if (!useCursor || followingCursor == 0)
                    this.Following.Clear();

                foreach (var user in following)
                {
                    this.Following.Add(user);
                }

                followingCursor = following.NextCursor;
            }
            catch
            {
                if (!useCursor || followingCursor == 0)
                    this.Following.Clear();

                this.UpdatingFollowing = false;
                return;
            }

            

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
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"user_id", this.Tokens.UserId},
                    {"count", 20},
                };
                if (useCursor && followersCursor != 0)
                    param.Add("cursor", followersCursor);

                var followers = await Tokens.Followers.ListAsync(param);
                if (!useCursor || followersCursor == 0)
                    this.Followers.Clear();

                foreach (var user in followers)
                {
                    this.Followers.Add(user);
                }

                followersCursor = followers.NextCursor;
            }
            catch
            {
                if (!useCursor || followersCursor == 0)
                    this.Followers.Clear();

                this.UpdatingFollowers = false;
                return;
            }
            
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
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"count", 20},
                };
                if (useCursor && blockCursor != 0)
                    param.Add("cursor", blockCursor);

                var block = await Tokens.Blocks.ListAsync(param);
                if (!useCursor || blockCursor == 0)
                    this.Block.Clear();

                foreach (var user in block)
                {
                    this.Block.Add(user);
                }

                blockCursor = block.NextCursor;
            }
            catch
            {
                if (!useCursor || blockCursor == 0)
                    this.Block.Clear();

                this.UpdatingBlock = false;
                return;
            }

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
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"count", 20},
                };
                if (useCursor && muteCursor != 0)
                    param.Add("cursor", muteCursor);
                
                var mute = await Tokens.Mutes.Users.ListAsync(param);
                if (!useCursor || muteCursor == 0)
                    this.Mute.Clear();

                foreach (var user in mute)
                {
                    this.Mute.Add(user);
                }

                muteCursor = mute.NextCursor;
            }
            catch
            {
                if (!useCursor || muteCursor == 0)
                    this.Mute.Clear();

                this.UpdatingMute = false;
                return;
            }
            
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
                
                try
                {
                    var crush = await Tokens.Users.LookupAsync(user_id => ids.AsEnumerable());

                    if (!useCursor || crushCursor == 0)
                        this.Crush.Clear();

                    foreach (var user in crush)
                    {
                        this.Crush.Add(user);
                    }
                }
                catch
                {
                    if (!useCursor || crushCursor == 0)
                        this.Crush.Clear();

                    this.UpdatingCrush = false;
                    return;
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
                
                try
                {
                    var crushedOn = await Tokens.Users.LookupAsync(user_id => ids.AsEnumerable());
                    if (!useCursor || crushedOnCursor == 0)
                        this.CrushedOn.Clear();

                    foreach (var user in crushedOn)
                    {
                        this.CrushedOn.Add(user);
                    }
                }
                catch
                {
                    if (!useCursor || crushedOnCursor == 0)
                        this.CrushedOn.Clear();

                    this.UpdatingCrushedOn = false;
                    return;
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
                
                long idsCursor = 0;

                try
                {
                    while (true)
                    {
                        var param = new Dictionary<string, object>()
                        {
                            {"user_id", this.Tokens.UserId },
                            {"count", 5000},
                        };
                        if (idsCursor != 0)
                            param.Add("cursor", idsCursor);

                        var userFollowingIds = await Tokens.Friends.IdsAsync(param);

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
                        var param = new Dictionary<string, object>()
                        {
                            {"user_id", this.Tokens.UserId },
                            {"count", 5000},
                        };
                        if (idsCursor != 0)
                            param.Add("cursor", idsCursor);

                        var userFollowerIds = await Tokens.Followers.IdsAsync(param);

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
