using CoreTweet;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Services.Database;
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
    public class StatusDetailSettingsFlyoutModel : BindableBase
    {
        public StatusDetailSettingsFlyoutModel()
        {
            this.ActionStatuses = new ObservableCollection<Twitter.Objects.Status>();
            this.Status = null;
        }

        #region Tokens変更通知プロパティ
        private CoreTweet.Tokens _Tokens;
        public CoreTweet.Tokens Tokens
        {
            get { return this._Tokens; }
            set { this.SetProperty(ref this._Tokens, value); }
        }
        #endregion

        #region StatusId変更通知プロパティ
        private long _StatusId;
        public long StatusId
        { 
            get { return this._StatusId; }
            set { this.SetProperty(ref this._StatusId, value); }
        }
        #endregion

        #region UpdatingStatus変更通知プロパティ
        private bool _UpdatingStatus;
        public bool UpdatingStatus
        {
            get { return this._UpdatingStatus; }
            set { this.SetProperty(ref this._UpdatingStatus, value); }
        }
        #endregion

        #region UpdatingActionStatuses変更通知プロパティ
        private bool _UpdatingActionStatuses;
        public bool UpdatingActionStatuses
        {
            get { return this._UpdatingActionStatuses; }
            set { this.SetProperty(ref this._UpdatingActionStatuses, value); }
        }
        #endregion

        #region Status変更通知プロパティ
        private Twitter.Objects.Status _Status;
        public Twitter.Objects.Status Status
        {
            get { return this._Status; }
            set { this.SetProperty(ref this._Status, value); }
        }
        #endregion

        public ObservableCollection<Twitter.Objects.Status> ActionStatuses { get; set; }

        public async Task UpdateStatus()
        {
            if (this.UpdatingStatus)
                return;

            if (this._StatusId == 0 || this.Tokens == null)
                return;

            this.UpdatingStatus = true;

            this.Status = null;

            var status = SettingService.Setting.EnableDatabase ? Database.Instance.GetStatusFromId(this._StatusId) : null;
            if (status == null)
            {
                Status item = null;
                try
                {
                    item = await Tokens.Statuses.ShowAsync(id => this._StatusId, tweet_mode => TweetMode.extended);
                }
                catch
                {
                    this.Status = null;

                    this.UpdatingStatus = false;
                    return;
                }

                status = new Twitter.Objects.Status(item);
                Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.Tokens.UserId, new List<string>() { "none://" }, false));
            }
            

            this.Status = status;

            this.UpdatingStatus = false;
        }

        public async Task UpdateActionStatuses()
        {
            if (this.UpdatingActionStatuses)
                return;

            if (this._StatusId == 0 || this.Tokens == null)
                return;

            this.UpdatingActionStatuses = true;

            this.ActionStatuses.Clear();

            Twitter.Objects.Status sourceStatus = null, sourceStatusRetweeted = null;
            if (this._Status != null)
            {
                sourceStatus = this._Status;
            }
            else
            {
                if (!this.UpdatingStatus)
                {
                    await this.UpdateStatus();
                    if (this._Status == null)
                    {
                        this.UpdatingActionStatuses = false;
                        return;
                    }

                    sourceStatus = this._Status;
                }
                else
                {
                    try
                    {
                        var cStatus = await Tokens.Statuses.ShowAsync(id => this._StatusId);
                        sourceStatus = new Twitter.Objects.Status(cStatus);
                    }
                    catch
                    {
                        this.UpdatingStatus = false;
                        return;
                    }
                }
            }

            if (sourceStatus == null)
                return;
            
            if (sourceStatus.HasRetweetInformation)
            {
                try
                {
                    var status = await Tokens.Statuses.ShowAsync(id => sourceStatus.Id);
                    sourceStatusRetweeted = new Twitter.Objects.Status(status);
                }
                catch
                {
                }
            }

            SearchResult search, urlQuoteRetweetSearch;
            try
            {
                search = await Tokens.Search.TweetsAsync(q => "to:" + sourceStatus.User.ScreenName, count => 50);
                urlQuoteRetweetSearch = await Tokens.Search.TweetsAsync(q => "https://twitter.com/" + sourceStatus.User.ScreenName + "/status/" + sourceStatus.Id.ToString(), count => 20);
            }
            catch
            {
                this.ActionStatuses.Clear();

                this.UpdatingActionStatuses = false;
                return;
            }


            this.ActionStatuses.Clear();

            this.ActionStatuses.Add(sourceStatus);
            if (sourceStatusRetweeted != null)
                this.ActionStatuses.Add(sourceStatusRetweeted); 


            foreach (var item in search)
            {
                if (!item.InReplyToStatusId.HasValue || item.InReplyToStatusId.Value != sourceStatus.Id || item.RetweetedStatus != null)
                    continue;

                var status = new Twitter.Objects.Status(item);
                Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.Tokens.UserId, new List<string>() { "none://" }, false));

                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = this.ActionStatuses.IndexOf(this.ActionStatuses.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) == id));
                if (index == -1)
                {
                    index = this.ActionStatuses.IndexOf(this.ActionStatuses.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) < id));
                    if (index == -1)
                        this.ActionStatuses.Add(status);
                    else
                        this.ActionStatuses.Insert(index, status);
                }
            }
            foreach (var item in urlQuoteRetweetSearch)
            {
                if (item.RetweetedStatus != null)
                    continue;

                var status = new Twitter.Objects.Status(item);
                Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.Tokens.UserId, new List<string>() { "none://" }, false));

                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = this.ActionStatuses.IndexOf(this.ActionStatuses.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) == id));
                if (index == -1)
                {
                    index = this.ActionStatuses.IndexOf(this.ActionStatuses.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) < id));
                    if (index == -1)
                        this.ActionStatuses.Add(status);
                    else
                        this.ActionStatuses.Insert(index, status);
                }
            }
            
            this.UpdatingActionStatuses = false;
        }

    }
}
