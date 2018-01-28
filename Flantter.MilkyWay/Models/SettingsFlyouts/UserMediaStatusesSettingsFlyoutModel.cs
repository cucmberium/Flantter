using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.Services;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class UserMediaStatusesSettingsFlyoutModel : BindableBase
    {
        public UserMediaStatusesSettingsFlyoutModel()
        {
            UserMediaStatuses = new ObservableCollection<Status>();
        }

        public ObservableCollection<Status> UserMediaStatuses { get; set; }

        private long _lastStatusId;

        public async Task UpdateUserMediaStatuses(bool incrementalLoad = false, bool clear = true)
        {
            if (Updating)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            Updating = true;

            if (!incrementalLoad && clear)
                UserMediaStatuses.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"user_id", _userId},
                    {"count", 200},
                    {"tweet_mode", CoreTweet.TweetMode.Extended},
                    {"only_media", true}
                };
                if (incrementalLoad && _lastStatusId != 0)
                    param.Add("max_id", _lastStatusId);

                var userMediaStatuses = await Tokens.Statuses.UserTimelineAsync(param);
                if (!incrementalLoad && clear)
                    UserMediaStatuses.Clear();

                foreach (var status in userMediaStatuses)
                {
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, Tokens.UserId, Tokens.Instance, new List<string> {"none://"}, false));

                    if (status.Entities.Media.Count == 0 || status.HasRetweetInformation)
                        continue;

                    var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                    var index = UserMediaStatuses.IndexOf(
                        UserMediaStatuses.FirstOrDefault(x =>
                            (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) == id));
                    if (index == -1)
                    {
                        index = UserMediaStatuses.IndexOf(
                            UserMediaStatuses.FirstOrDefault(
                                x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) < id));
                        if (index == -1)
                            UserMediaStatuses.Add(status);
                        else
                            UserMediaStatuses.Insert(index, status);
                    }
                }

                var lastStatus = userMediaStatuses.Last();
                _lastStatusId = lastStatus.HasRetweetInformation ? lastStatus.RetweetInformation.Id : lastStatus.Id;
            }
            catch
            {
                if (!incrementalLoad && clear)
                    UserMediaStatuses.Clear();

                Updating = false;
                return;
            }

            Updating = false;
        }

        #region Tokens変更通知プロパティ

        private Tokens _tokens;

        public Tokens Tokens
        {
            get => _tokens;
            set => SetProperty(ref _tokens, value);
        }

        #endregion

        #region Updating変更通知プロパティ

        private bool _updating;

        public bool Updating
        {
            get => _updating;
            set => SetProperty(ref _updating, value);
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
    }
}