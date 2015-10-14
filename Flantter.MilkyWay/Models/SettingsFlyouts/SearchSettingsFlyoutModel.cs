using CoreTweet;
using CoreTweet.Core;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class SearchSettingsFlyoutModel : BindableBase
    {
        public SearchSettingsFlyoutModel()
        {
            this.Statuses = new ObservableCollection<Twitter.Objects.Status>();
            this.Users = new ObservableCollection<Twitter.Objects.User>();
        }

        #region Tokens変更通知プロパティ
        private CoreTweet.Tokens _Tokens;
        public CoreTweet.Tokens Tokens
        {
            get { return this._Tokens; }
            set { this.SetProperty(ref this._Tokens, value); }
        }
        #endregion

        #region UserSearchWords変更通知プロパティ
        private string _UserSearchWords;
        public string UserSearchWords
        {
            get { return this._UserSearchWords; }
            set { this.SetProperty(ref this._UserSearchWords, value); }
        }
        #endregion

        #region UpdatingUserSearch変更通知プロパティ
        private bool _UpdatingUserSearch;
        public bool UpdatingUserSearch
        {
            get { return this._UpdatingUserSearch; }
            set { this.SetProperty(ref this._UpdatingUserSearch, value); }
        }
        #endregion

        #region StatusSearchWords変更通知プロパティ
        private string _StatusSearchWords;
        public string StatusSearchWords
        {
            get { return this._StatusSearchWords; }
            set { this.SetProperty(ref this._StatusSearchWords, value); }
        }
        #endregion

        #region UpdatingStatusSearch変更通知プロパティ
        private bool _UpdatingStatusSearch;
        public bool UpdatingStatusSearch
        {
            get { return this._UpdatingStatusSearch; }
            set { this.SetProperty(ref this._UpdatingStatusSearch, value); }
        }
        #endregion

        public ObservableCollection<Twitter.Objects.Status> Statuses { get; set; }

        public ObservableCollection<Twitter.Objects.User> Users { get; set; }

        public async Task UpdateStatuses(long maxid = 0)
        {
            if (this.UpdatingStatusSearch)
                return;

            if (string.IsNullOrWhiteSpace(this._StatusSearchWords) || this.Tokens == null)
                return;

            this.UpdatingStatusSearch = true;

            SearchResult search;
            try
            {
                if (maxid == 0)
                    search = await Tokens.Search.TweetsAsync(q => this._StatusSearchWords, count => 20);
                else
                    search = await Tokens.Search.TweetsAsync(q => this._StatusSearchWords, count => 20, max_id => maxid);
            }
            catch
            {
                if (maxid == 0)
                    this.Statuses.Clear();

                this.UpdatingStatusSearch = false;
                return;
            }

            if (maxid == 0)
                this.Statuses.Clear();

            foreach (var item in search)
            {
                var status = new Twitter.Objects.Status(item);

                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = this.Statuses.IndexOf(this.Statuses.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) == id));
                if (index == -1)
                {
                    index = this.Statuses.IndexOf(this.Statuses.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) < id));
                    if (index == -1)
                        this.Statuses.Add(status);
                    else
                        this.Statuses.Insert(index, status);
                }
            }

            // Todo : 受信したツイートをデータベースに登録

            this.UpdatingStatusSearch = false;
        }

        private long usersCursor = 0;
        public async Task UpdateUsers(bool useCursor = false)
        {
            if (this.UpdatingUserSearch)
                return;

            if (string.IsNullOrWhiteSpace(this._UserSearchWords) || this.Tokens == null)
                return;

            this.UpdatingUserSearch = true;

            ListedResponse<CoreTweet.User> following;
            try
            {
                if (useCursor && usersCursor != 0)
                    following = await Tokens.Users.SearchAsync(screen_name => this._UserSearchWords, count => 20, page => usersCursor);
                else
                    following = await Tokens.Users.SearchAsync(screen_name => this._UserSearchWords, count => 20);
            }
            catch
            {
                if (!useCursor || usersCursor == 0)
                    this.Users.Clear();

                this.UpdatingUserSearch = false;
                return;
            }

            if (!useCursor || usersCursor == 0)
                this.Users.Clear();

            foreach (var item in following)
            {
                var user = new Twitter.Objects.User(item);
                this.Users.Add(user);
            }

            if (useCursor)
                usersCursor += 1;
            else
                usersCursor = 1;

            this.UpdatingUserSearch = false;
        }
    }
}
