using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class UserListsSettingsFlyoutModel : BindableBase
    {
        private long _memberListsCursor;

        private long _subscribeListsCursor;

        private long _userListsCursor;

        public UserListsSettingsFlyoutModel()
        {
            UserLists = new ObservableCollection<List>();
            SubscribeLists = new ObservableCollection<List>();
            MembershipLists = new ObservableCollection<List>();
        }

        public bool OpenSubscribeLists { get; set; }
        public bool OpenMembershipLists { get; set; }

        public async Task UpdateUserLists(bool useCursor = false)
        {
            if (UpdatingUserLists)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            if (useCursor && _userListsCursor == 0)
                return;

            UpdatingUserLists = true;

            if (!useCursor || _userListsCursor == 0)
                UserLists.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"user_id", _userId},
                    {"count", 20}
                };
                if (useCursor && _userListsCursor != 0)
                    param.Add("cursor", _userListsCursor);

                var userLists = await Tokens.Lists.OwnershipsAsync(param);

                if (!useCursor || _userListsCursor == 0)
                    UserLists.Clear();

                foreach (var list in userLists)
                    UserLists.Add(list);

                _userListsCursor = userLists.NextCursor;
            }
            catch
            {
                if (!useCursor || _userListsCursor == 0)
                    UserLists.Clear();

                UpdatingUserLists = false;
                return;
            }

            UpdatingUserLists = false;
        }

        public async Task UpdateSubscribeLists(bool useCursor = false)
        {
            if (UpdatingSubscribeLists)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            if (useCursor && _subscribeListsCursor == 0)
                return;

            UpdatingSubscribeLists = true;

            if (!useCursor || _subscribeListsCursor == 0)
                SubscribeLists.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"user_id", _userId},
                    {"count", 20}
                };
                if (useCursor && _subscribeListsCursor != 0)
                    param.Add("cursor", _subscribeListsCursor);

                var subscribeLists = await Tokens.Lists.SubscriptionsAsync(param);

                if (!useCursor || _subscribeListsCursor == 0)
                    SubscribeLists.Clear();

                foreach (var list in subscribeLists)
                    SubscribeLists.Add(list);

                _subscribeListsCursor = subscribeLists.NextCursor;
            }
            catch
            {
                if (!useCursor || _subscribeListsCursor == 0)
                    SubscribeLists.Clear();

                UpdatingSubscribeLists = false;
                return;
            }

            UpdatingSubscribeLists = false;
        }

        public async Task UpdateMembershipLists(bool useCursor = false)
        {
            if (UpdatingMembershipLists)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            if (useCursor && _memberListsCursor == 0)
                return;

            UpdatingMembershipLists = true;

            if (!useCursor || _memberListsCursor == 0)
                MembershipLists.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"user_id", _userId},
                    {"count", 20}
                };
                if (useCursor && _memberListsCursor != 0)
                    param.Add("cursor", _memberListsCursor);

                var membershipLists = await Tokens.Lists.MembershipsAsync(param);

                if (!useCursor || _memberListsCursor == 0)
                    MembershipLists.Clear();

                foreach (var list in membershipLists)
                    MembershipLists.Add(list);

                _memberListsCursor = membershipLists.NextCursor;
            }
            catch
            {
                if (!useCursor || _memberListsCursor == 0)
                    MembershipLists.Clear();

                UpdatingMembershipLists = false;
                return;
            }

            UpdatingMembershipLists = false;
        }

        #region Tokens変更通知プロパティ

        private Tokens _tokens;

        public Tokens Tokens
        {
            get => _tokens;
            set => SetProperty(ref _tokens, value);
        }

        #endregion

        #region UserId変更通知プロパティ

        private long _userId;

        public long UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        #endregion

        #region UserLists変更通知プロパティ

        private ObservableCollection<List> _userLists;

        public ObservableCollection<List> UserLists
        {
            get => _userLists;
            set => SetProperty(ref _userLists, value);
        }

        #endregion

        #region SubscribeLists変更通知プロパティ

        private ObservableCollection<List> _subscribeLists;

        public ObservableCollection<List> SubscribeLists
        {
            get => _subscribeLists;
            set => SetProperty(ref _subscribeLists, value);
        }

        #endregion

        #region MembershipLists変更通知プロパティ

        private ObservableCollection<List> _membershipLists;

        public ObservableCollection<List> MembershipLists
        {
            get => _membershipLists;
            set => SetProperty(ref _membershipLists, value);
        }

        #endregion

        #region UpdatingUserLists変更通知プロパティ

        private bool _updatingUserLists;

        public bool UpdatingUserLists
        {
            get => _updatingUserLists;
            set => SetProperty(ref _updatingUserLists, value);
        }

        #endregion

        #region UpdatingSubscribeLists変更通知プロパティ

        private bool _updatingSubscribeLists;

        public bool UpdatingSubscribeLists
        {
            get => _updatingSubscribeLists;
            set => SetProperty(ref _updatingSubscribeLists, value);
        }

        #endregion

        #region UpdatingMembershipLists変更通知プロパティ

        private bool _updatingMembershipLists;

        public bool UpdatingMembershipLists
        {
            get => _updatingMembershipLists;
            set => SetProperty(ref _updatingMembershipLists, value);
        }

        #endregion
    }
}