using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Services.Database;
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
    public class StatusDetailSettingsFlyoutModel : BindableBase
    {
        public StatusDetailSettingsFlyoutModel()
        {
            this.ActionStatuses = new ObservableCollection<Twitter.Objects.Status>();
            this.Status = null;
        }

        #region Tokens変更通知プロパティ
        private Tokens _Tokens;
        public Tokens Tokens
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
                try
                {
                    status = await Tokens.Statuses.ShowAsync(id => this._StatusId, tweet_mode => CoreTweet.TweetMode.extended);
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.Tokens.UserId, new List<string>() { "none://" }, false));
                }
                catch
                {
                    this.Status = null;

                    this.UpdatingStatus = false;
                    return;
                }
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
                        sourceStatus = await Tokens.Statuses.ShowAsync(id => this._StatusId);
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
                    sourceStatusRetweeted = await Tokens.Statuses.ShowAsync(id => sourceStatus.Id);
                }
                catch
                {
                }
            }
            
            try
            {
                var search = await Tokens.Search.TweetsAsync(q => "to:" + sourceStatus.User.ScreenName, count => 50);
                var urlQuoteRetweetSearch = await Tokens.Search.TweetsAsync(q => "https://twitter.com/" + sourceStatus.User.ScreenName + "/status/" + sourceStatus.Id.ToString(), count => 20);

                this.ActionStatuses.Clear();

                this.ActionStatuses.Add(sourceStatus);
                if (sourceStatusRetweeted != null)
                    this.ActionStatuses.Add(sourceStatusRetweeted);

                foreach (var status in search)
                {
                    if (status.InReplyToStatusId != sourceStatus.Id || status.HasRetweetInformation)
                        continue;
                    
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
                foreach (var status in urlQuoteRetweetSearch)
                {
                    if (status.HasRetweetInformation)
                        continue;
                    
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
            }
            catch
            {
                this.ActionStatuses.Clear();

                this.UpdatingActionStatuses = false;
                return;
            }
            
            this.UpdatingActionStatuses = false;
        }

    }
}
