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

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class UserListsSettingsFlyoutModel : BindableBase
    {
        public UserListsSettingsFlyoutModel()
        {
            this.UserLists = new ObservableCollection<Twitter.Objects.List>();
            this.SubscribeLists = new ObservableCollection<Twitter.Objects.List>();
            this.MembershipLists = new ObservableCollection<Twitter.Objects.List>();
        }

        #region Tokens変更通知プロパティ
        private Tokens _Tokens;
        public Tokens Tokens
        {
            get { return this._Tokens; }
            set { this.SetProperty(ref this._Tokens, value); }
        }
        #endregion

        #region ScreenName変更通知プロパティ
        private string _ScreenName;
        public string ScreenName
        {
            get { return this._ScreenName; }
            set { this.SetProperty(ref this._ScreenName, value); }
        }
        #endregion
        
        public bool OpenSubscribeLists { get; set; }
        public bool OpenMembershipLists { get; set; }

        #region UserLists変更通知プロパティ
        private ObservableCollection<Twitter.Objects.List> _UserLists;
        public ObservableCollection<Twitter.Objects.List> UserLists
        {
            get { return this._UserLists; }
            set { this.SetProperty(ref this._UserLists, value); }
        }
        #endregion

        #region SubscribeLists変更通知プロパティ
        private ObservableCollection<Twitter.Objects.List> _SubscribeLists;
        public ObservableCollection<Twitter.Objects.List> SubscribeLists
        {
            get { return this._SubscribeLists; }
            set { this.SetProperty(ref this._SubscribeLists, value); }
        }
        #endregion

        #region MembershipLists変更通知プロパティ
        private ObservableCollection<Twitter.Objects.List> _MembershipLists;
        public ObservableCollection<Twitter.Objects.List> MembershipLists
        {
            get { return this._MembershipLists; }
            set { this.SetProperty(ref this._MembershipLists, value); }
        }
        #endregion

        #region UpdatingUserLists変更通知プロパティ
        private bool _UpdatingUserLists;
        public bool UpdatingUserLists
        {
            get { return this._UpdatingUserLists; }
            set { this.SetProperty(ref this._UpdatingUserLists, value); }
        }
        #endregion

        #region UpdatingSubscribeLists変更通知プロパティ
        private bool _UpdatingSubscribeLists;
        public bool UpdatingSubscribeLists
        {
            get { return this._UpdatingSubscribeLists; }
            set { this.SetProperty(ref this._UpdatingSubscribeLists, value); }
        }
        #endregion

        #region UpdatingMembershipLists変更通知プロパティ
        private bool _UpdatingMembershipLists;
        public bool UpdatingMembershipLists
        {
            get { return this._UpdatingMembershipLists; }
            set { this.SetProperty(ref this._UpdatingMembershipLists, value); }
        }
        #endregion
        
        private long userListsCursor = 0;
        public async Task UpdateUserLists(bool useCursor = false)
        {
            if (this.UpdatingUserLists)
                return;

            if (this._ScreenName == null || this.Tokens == null)
                return;

            if (useCursor && userListsCursor == 0)
                return;

            this.UpdatingUserLists = true;

            if (!useCursor || userListsCursor == 0)
                this.UserLists.Clear();
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"screen_name", this._ScreenName },
                    {"count", 20},
                };
                if (useCursor && userListsCursor != 0)
                    param.Add("cursor", userListsCursor);

                var userLists = await Tokens.Lists.OwnershipsAsync(param);

                if (!useCursor || userListsCursor == 0)
                    this.UserLists.Clear();

                foreach (var list in userLists)
                {
                    this.UserLists.Add(list);
                }

                userListsCursor = userLists.NextCursor;
            }
            catch
            {
                if (!useCursor || userListsCursor == 0)
                    this.UserLists.Clear();

                this.UpdatingUserLists = false;
                return;
            }

            this.UpdatingUserLists = false;
        }

        private long subscribeListsCursor = 0;
        public async Task UpdateSubscribeLists(bool useCursor = false)
        {
            if (this.UpdatingSubscribeLists)
                return;

            if (this._ScreenName == null || this.Tokens == null)
                return;

            if (useCursor && subscribeListsCursor == 0)
                return;

            this.UpdatingSubscribeLists = true;

            if (!useCursor || subscribeListsCursor == 0)
                this.SubscribeLists.Clear();
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"screen_name", this._ScreenName },
                    {"count", 20},
                };
                if (useCursor && subscribeListsCursor != 0)
                    param.Add("cursor", subscribeListsCursor);

                var subscribeLists = await Tokens.Lists.SubscriptionsAsync(param);

                if (!useCursor || subscribeListsCursor == 0)
                    this.SubscribeLists.Clear();

                foreach (var list in subscribeLists)
                {
                    this.SubscribeLists.Add(list);
                }

                subscribeListsCursor = subscribeLists.NextCursor;
            }
            catch
            {
                if (!useCursor || subscribeListsCursor == 0)
                    this.SubscribeLists.Clear();

                this.UpdatingSubscribeLists = false;
                return;
            }

            this.UpdatingSubscribeLists = false;
        }

        private long memberListsCursor = 0;
        public async Task UpdateMembershipLists(bool useCursor = false)
        {
            if (this.UpdatingMembershipLists)
                return;

            if (this._ScreenName == null || this.Tokens == null)
                return;

            if (useCursor && memberListsCursor == 0)
                return;

            this.UpdatingMembershipLists = true;

            if (!useCursor || memberListsCursor == 0)
                this.MembershipLists.Clear();
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"screen_name", this._ScreenName },
                    {"count", 20},
                };
                if (useCursor && memberListsCursor != 0)
                    param.Add("cursor", memberListsCursor);

                var membershipLists = await Tokens.Lists.MembershipsAsync(param);

                if (!useCursor || memberListsCursor == 0)
                    this.MembershipLists.Clear();

                foreach (var list in membershipLists)
                {
                    this.MembershipLists.Add(list);
                }

                memberListsCursor = membershipLists.NextCursor;
            }
            catch
            {
                if (!useCursor || memberListsCursor == 0)
                    this.MembershipLists.Clear();

                this.UpdatingMembershipLists = false;
                return;
            }

            this.UpdatingMembershipLists = false;
        }
    }
}
