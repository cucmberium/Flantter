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
    public class RetweetsOfMeSettingsFlyoutModel : BindableBase
    {
        public RetweetsOfMeSettingsFlyoutModel()
        {
            RetweetsOfMe = new ObservableCollection<Status>();
        }

        public ObservableCollection<Status> RetweetsOfMe { get; set; }

        public async Task UpdateRetweetsOfMe(long maxid = 0, bool clear = true)
        {
            if (Updating)
                return;

            if (Tokens == null)
                return;

            Updating = true;

            if (maxid == 0 && clear)
                RetweetsOfMe.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 20},
                    {"tweet_mode", CoreTweet.TweetMode.extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);

                var listStatus = await Tokens.Statuses.RetweetsOfMeAsync(param);

                if (maxid == 0 && clear)
                    RetweetsOfMe.Clear();

                foreach (var status in listStatus)
                {
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, Tokens.UserId, new List<string> {"none://"}, false));

                    var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                    var index = RetweetsOfMe.IndexOf(
                        RetweetsOfMe.FirstOrDefault(x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) ==
                                                         id));
                    if (index == -1)
                    {
                        index = RetweetsOfMe.IndexOf(
                            RetweetsOfMe.FirstOrDefault(
                                x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) < id));
                        if (index == -1)
                            RetweetsOfMe.Add(status);
                        else
                            RetweetsOfMe.Insert(index, status);
                    }
                }
            }
            catch
            {
                if (maxid == 0 && clear)
                    RetweetsOfMe.Clear();

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
    }
}