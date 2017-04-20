using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Services.Database;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class StatusDetailSettingsFlyoutModel : BindableBase
    {
        public StatusDetailSettingsFlyoutModel()
        {
            ActionStatuses = new ObservableCollection<Status>();
            Status = null;
        }

        public ObservableCollection<Status> ActionStatuses { get; set; }

        public async Task UpdateStatus()
        {
            if (UpdatingStatus)
                return;

            if (_statusId == 0 || Tokens == null)
                return;

            UpdatingStatus = true;

            Status = null;

            var status = SettingService.Setting.EnableDatabase ? Database.Instance.GetStatusFromId(_statusId) : null;
            if (status == null)
                try
                {
                    status = await Tokens.Statuses.ShowAsync(id => _statusId, tweet_mode => CoreTweet.TweetMode.extended);
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, Tokens.UserId, new List<string> {"none://"}, false));
                }
                catch
                {
                    Status = null;

                    UpdatingStatus = false;
                    return;
                }


            Status = status;

            UpdatingStatus = false;
        }

        public async Task UpdateActionStatuses()
        {
            if (UpdatingActionStatuses)
                return;

            if (_statusId == 0 || Tokens == null)
                return;

            UpdatingActionStatuses = true;

            ActionStatuses.Clear();

            Status sourceStatus = null;
            Status sourceStatusRetweeted = null;
            if (_status != null)
            {
                sourceStatus = _status;
            }
            else
            {
                if (!UpdatingStatus)
                {
                    await UpdateStatus();
                    if (_status == null)
                    {
                        UpdatingActionStatuses = false;
                        return;
                    }

                    sourceStatus = _status;
                }
                else
                {
                    try
                    {
                        sourceStatus = await Tokens.Statuses.ShowAsync(id => _statusId);
                    }
                    catch
                    {
                        UpdatingStatus = false;
                        return;
                    }
                }
            }

            if (sourceStatus == null)
                return;

            if (sourceStatus.HasRetweetInformation)
                try
                {
                    sourceStatusRetweeted = await Tokens.Statuses.ShowAsync(id => sourceStatus.Id);
                }
                catch
                {
                }

            try
            {
                var search = await Tokens.Search.TweetsAsync(q => "to:" + sourceStatus.User.ScreenName, count => 50);
                var urlQuoteRetweetSearch =
                    await Tokens.Search.TweetsAsync(
                        q => "https://twitter.com/" + sourceStatus.User.ScreenName + "/status/" +
                             sourceStatus.Id.ToString(), count => 20);

                ActionStatuses.Clear();

                ActionStatuses.Add(sourceStatus);
                if (sourceStatusRetweeted != null)
                    ActionStatuses.Add(sourceStatusRetweeted);

                foreach (var status in search)
                {
                    if (status.InReplyToStatusId != sourceStatus.Id || status.HasRetweetInformation)
                        continue;

                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, Tokens.UserId, new List<string> {"none://"}, false));

                    var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                    var index = ActionStatuses.IndexOf(
                        ActionStatuses.FirstOrDefault(x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) == id));
                    if (index == -1)
                    {
                        index = ActionStatuses.IndexOf(
                            ActionStatuses.FirstOrDefault(
                                x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) < id));
                        if (index == -1)
                            ActionStatuses.Add(status);
                        else
                            ActionStatuses.Insert(index, status);
                    }
                }
                foreach (var status in urlQuoteRetweetSearch)
                {
                    if (status.HasRetweetInformation)
                        continue;

                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, Tokens.UserId, new List<string> {"none://"}, false));

                    var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                    var index = ActionStatuses.IndexOf(
                        ActionStatuses.FirstOrDefault(x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) == id));
                    if (index == -1)
                    {
                        index = ActionStatuses.IndexOf(
                            ActionStatuses.FirstOrDefault(
                                x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) < id));
                        if (index == -1)
                            ActionStatuses.Add(status);
                        else
                            ActionStatuses.Insert(index, status);
                    }
                }
            }
            catch
            {
                ActionStatuses.Clear();

                UpdatingActionStatuses = false;
                return;
            }

            UpdatingActionStatuses = false;
        }

        #region Tokens変更通知プロパティ

        private Tokens _tokens;

        public Tokens Tokens
        {
            get => _tokens;
            set => SetProperty(ref _tokens, value);
        }

        #endregion

        #region StatusId変更通知プロパティ

        private long _statusId;

        public long StatusId
        {
            get => _statusId;
            set => SetProperty(ref _statusId, value);
        }

        #endregion

        #region UpdatingStatus変更通知プロパティ

        private bool _updatingStatus;

        public bool UpdatingStatus
        {
            get => _updatingStatus;
            set => SetProperty(ref _updatingStatus, value);
        }

        #endregion

        #region UpdatingActionStatuses変更通知プロパティ

        private bool _updatingActionStatuses;

        public bool UpdatingActionStatuses
        {
            get => _updatingActionStatuses;
            set => SetProperty(ref _updatingActionStatuses, value);
        }

        #endregion

        #region Status変更通知プロパティ

        private Status _status;

        public Status Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        #endregion
    }
}