using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class ListStatusesSettingsFlyoutModel : BindableBase
    {
        public ListStatusesSettingsFlyoutModel()
        {
            ListStatuses = new ObservableCollection<Status>();
        }

        public ObservableCollection<Status> ListStatuses { get; set; }

        public async Task UpdateListStatuses(long maxid = 0, bool clear = true)
        {
            if (Updating)
                return;

            if (_id == 0 || Tokens == null)
                return;

            Updating = true;

            if (maxid == 0 && clear)
                ListStatuses.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"list_id", _id},
                    {"count", 20},
                    {"tweet_mode", CoreTweet.TweetMode.Extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);

                var listStatuses = await Tokens.Lists.StatusesAsync(param);
                if (maxid == 0 && clear)
                    ListStatuses.Clear();

                foreach (var status in listStatuses)
                {
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, Tokens.UserId, new List<string> {"none://"}, false));

                    var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                    var index = ListStatuses.IndexOf(
                        ListStatuses.FirstOrDefault(x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) == id));
                    if (index == -1)
                    {
                        index = ListStatuses.IndexOf(
                            ListStatuses.FirstOrDefault(
                                x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) < id));
                        if (index == -1)
                            ListStatuses.Add(status);
                        else
                            ListStatuses.Insert(index, status);
                    }
                }
            }
            catch
            {
                if (maxid == 0 && clear)
                    ListStatuses.Clear();

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

        #region Id変更通知プロパティ

        private long _id;

        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        #endregion
    }
}