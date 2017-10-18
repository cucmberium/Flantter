using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class UserFollowInfoSettingsFlyoutModel : BindableBase
    {
        private long _blockCursor;

        private long _crushCursor;

        private long _crushedOnCursor;

        private long _followersCursor;

        private long _followingCursor;

        private long _muteCursor;

        public UserFollowInfoSettingsFlyoutModel()
        {
            IdsAsyncLock = new AsyncLock();

            Following = new ObservableCollection<User>();
            Followers = new ObservableCollection<User>();
            Crush = new ObservableCollection<User>();
            CrushedOn = new ObservableCollection<User>();
            Block = new ObservableCollection<User>();
            Mute = new ObservableCollection<User>();
        }

        public bool OpenFollowers { get; set; }

        public bool OpenCrush { get; set; }

        public bool OpenCrushedOn { get; set; }

        public bool OpenBlock { get; set; }

        public bool OpenMute { get; set; }

        public ObservableCollection<User> Following { get; set; }

        public ObservableCollection<User> Followers { get; set; }

        public ObservableCollection<User> Crush { get; set; }

        public ObservableCollection<User> CrushedOn { get; set; }

        public ObservableCollection<User> Block { get; set; }

        public ObservableCollection<User> Mute { get; set; }


        public AsyncLock IdsAsyncLock { get; set; }
        public bool GetIds { get; set; }
        public List<long> FollowingIds { get; set; } = new List<long>();
        public List<long> FollowersIds { get; set; } = new List<long>();
        public List<long> CrushIds { get; set; } = new List<long>();
        public List<long> CrushedOnIds { get; set; } = new List<long>();

        public async Task UpdateFollowing(bool useCursor = false)
        {
            if (UpdatingFollowing)
                return;

            if (Tokens == null)
                return;

            if (useCursor && _followingCursor == 0)
                return;

            UpdatingFollowing = true;

            if (!useCursor || _followingCursor == 0)
                Following.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"user_id", Tokens.UserId},
                    {"count", 20}
                };
                if (useCursor && _followingCursor != 0)
                    param.Add("cursor", _followingCursor);

                var following = await Tokens.Friends.ListAsync(param);
                if (!useCursor || _followingCursor == 0)
                    Following.Clear();

                foreach (var user in following)
                    Following.Add(user);

                _followingCursor = following.NextCursor;
            }
            catch
            {
                if (!useCursor || _followingCursor == 0)
                    Following.Clear();

                UpdatingFollowing = false;
                return;
            }


            UpdatingFollowing = false;
        }

        public async Task UpdateFollowers(bool useCursor = false)
        {
            if (UpdatingFollowers)
                return;

            if (Tokens == null)
                return;

            if (useCursor && _followersCursor == 0)
                return;

            UpdatingFollowers = true;

            if (!useCursor || _followersCursor == 0)
                Followers.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"user_id", Tokens.UserId},
                    {"count", 20}
                };
                if (useCursor && _followersCursor != 0)
                    param.Add("cursor", _followersCursor);

                var followers = await Tokens.Followers.ListAsync(param);
                if (!useCursor || _followersCursor == 0)
                    Followers.Clear();

                foreach (var user in followers)
                    Followers.Add(user);

                _followersCursor = followers.NextCursor;
            }
            catch
            {
                if (!useCursor || _followersCursor == 0)
                    Followers.Clear();

                UpdatingFollowers = false;
                return;
            }

            UpdatingFollowers = false;
        }

        public async Task UpdateBlock(bool useCursor = false)
        {
            if (UpdatingBlock)
                return;

            if (Tokens == null)
                return;

            if (useCursor && _blockCursor == 0)
                return;

            UpdatingBlock = true;

            if (!useCursor || _blockCursor == 0)
                Block.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 20}
                };
                if (useCursor && _blockCursor != 0)
                    param.Add("cursor", _blockCursor);

                var block = await Tokens.Blocks.ListAsync(param);
                if (!useCursor || _blockCursor == 0)
                    Block.Clear();

                foreach (var user in block)
                    Block.Add(user);

                _blockCursor = block.NextCursor;
            }
            catch
            {
                if (!useCursor || _blockCursor == 0)
                    Block.Clear();

                UpdatingBlock = false;
                return;
            }

            UpdatingBlock = false;
        }

        public async Task UpdateMute(bool useCursor = false)
        {
            if (UpdatingMute)
                return;

            if (Tokens == null)
                return;

            if (useCursor && _muteCursor == 0)
                return;

            UpdatingMute = true;

            if (!useCursor || _muteCursor == 0)
                Mute.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 20}
                };
                if (useCursor && _muteCursor != 0)
                    param.Add("cursor", _muteCursor);

                var mute = await Tokens.Mutes.Users.ListAsync(param);
                if (!useCursor || _muteCursor == 0)
                    Mute.Clear();

                foreach (var user in mute)
                    Mute.Add(user);

                _muteCursor = mute.NextCursor;
            }
            catch
            {
                if (!useCursor || _muteCursor == 0)
                    Mute.Clear();

                UpdatingMute = false;
                return;
            }

            UpdatingMute = false;
        }

        public async Task UpdateCrush(bool useCursor = false)
        {
            if (UpdatingCrush)
                return;

            if (Tokens == null)
                return;

            if (useCursor && _crushCursor == 0)
                return;

            UpdatingCrush = true;

            if (!useCursor || _crushCursor == 0)
                Crush.Clear();

            if (!GetIds)
                await UpdateFriendShipIds();

            if (!GetIds)
            {
                UpdatingCrush = true;
                return;
            }

            using (await IdsAsyncLock.LockAsync())
            {
                var ids = new List<long>();
                for (var idsCount = 0; _crushCursor < CrushIds.Count && idsCount <= 99; _crushCursor++, idsCount++)
                    ids.Add(CrushIds[(int) _crushCursor]);

                try
                {
                    var crush = await Tokens.Users.LookupAsync(user_id => ids.AsEnumerable());

                    if (!useCursor || _crushCursor == 0)
                        Crush.Clear();

                    foreach (var user in crush)
                        Crush.Add(user);
                }
                catch
                {
                    if (!useCursor || _crushCursor == 0)
                        Crush.Clear();

                    UpdatingCrush = false;
                    return;
                }
            }

            if (_crushCursor == CrushIds.Count)
                _crushCursor = 0;

            UpdatingCrush = false;
        }

        public async Task UpdateCrushedOn(bool useCursor = false)
        {
            if (UpdatingCrushedOn)
                return;

            if (Tokens == null)
                return;

            if (useCursor && _crushedOnCursor == 0)
                return;

            UpdatingCrushedOn = true;

            if (!useCursor || _crushedOnCursor == 0)
                CrushedOn.Clear();

            if (!GetIds)
                await UpdateFriendShipIds();

            if (!GetIds)
            {
                UpdatingCrushedOn = true;
                return;
            }

            using (await IdsAsyncLock.LockAsync())
            {
                var ids = new List<long>();
                for (var idsCount = 0;
                    _crushedOnCursor < CrushedOnIds.Count && idsCount <= 99;
                    _crushedOnCursor++, idsCount++)
                    ids.Add(CrushedOnIds[(int) _crushedOnCursor]);

                try
                {
                    var crushedOn = await Tokens.Users.LookupAsync(user_id => ids.AsEnumerable());
                    if (!useCursor || _crushedOnCursor == 0)
                        CrushedOn.Clear();

                    foreach (var user in crushedOn)
                        CrushedOn.Add(user);
                }
                catch
                {
                    if (!useCursor || _crushedOnCursor == 0)
                        CrushedOn.Clear();

                    UpdatingCrushedOn = false;
                    return;
                }
            }

            if (_crushedOnCursor == CrushedOnIds.Count)
                _crushedOnCursor = 0;

            UpdatingCrushedOn = false;
        }


        public async Task UpdateFriendShipIds()
        {
            using (await IdsAsyncLock.LockAsync())
            {
                if (GetIds)
                    return;

                long idsCursor = 0;

                try
                {
                    while (true)
                    {
                        var param = new Dictionary<string, object>
                        {
                            {"user_id", Tokens.UserId},
                            {"count", 5000}
                        };
                        if (idsCursor != 0)
                            param.Add("cursor", idsCursor);

                        var userFollowingIds = await Tokens.Friends.IdsAsync(param);

                        foreach (var id in userFollowingIds)
                            FollowingIds.Add(id);

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
                        var param = new Dictionary<string, object>
                        {
                            {"user_id", Tokens.UserId},
                            {"count", 5000}
                        };
                        if (idsCursor != 0)
                            param.Add("cursor", idsCursor);

                        var userFollowerIds = await Tokens.Followers.IdsAsync(param);

                        foreach (var id in userFollowerIds)
                            FollowersIds.Add(id);

                        if (userFollowerIds.NextCursor == 0)
                            break;

                        idsCursor = userFollowerIds.NextCursor;
                    }
                }
                catch
                {
                    return;
                }

                CrushIds = FollowingIds.Except(FollowersIds).ToList();
                CrushedOnIds = FollowersIds.Except(FollowingIds).ToList();

                GetIds = true;
            }
        }

        #region Tokens変更通知プロパティ

        private Tokens _tokens;

        public Tokens Tokens
        {
            get => _tokens;
            set => SetProperty(ref _tokens, value);
        }

        #endregion

        #region UpdatingFollowing変更通知プロパティ

        private bool _updatingFollowing;

        public bool UpdatingFollowing
        {
            get => _updatingFollowing;
            set => SetProperty(ref _updatingFollowing, value);
        }

        #endregion

        #region UpdatingFollowers変更通知プロパティ

        private bool _updatingFollowers;

        public bool UpdatingFollowers
        {
            get => _updatingFollowers;
            set => SetProperty(ref _updatingFollowers, value);
        }

        #endregion

        #region UpdatingCrush変更通知プロパティ

        private bool _updatingCrush;

        public bool UpdatingCrush
        {
            get => _updatingCrush;
            set => SetProperty(ref _updatingCrush, value);
        }

        #endregion

        #region UpdatingCrushedOn変更通知プロパティ

        private bool _updatingCrushedOn;

        public bool UpdatingCrushedOn
        {
            get => _updatingCrushedOn;
            set => SetProperty(ref _updatingCrushedOn, value);
        }

        #endregion

        #region UpdatingBlock変更通知プロパティ

        private bool _updatingBlock;

        public bool UpdatingBlock
        {
            get => _updatingBlock;
            set => SetProperty(ref _updatingBlock, value);
        }

        #endregion

        #region UpdatingMute変更通知プロパティ

        private bool _updatingMute;

        public bool UpdatingMute
        {
            get => _updatingMute;
            set => SetProperty(ref _updatingMute, value);
        }

        #endregion
    }
}