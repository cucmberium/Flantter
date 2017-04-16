using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Services;
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
    public class RetweetsOfMeSettingsFlyoutModel : BindableBase
    {
        public RetweetsOfMeSettingsFlyoutModel()
        {
            this.RetweetsOfMe = new ObservableCollection<Twitter.Objects.Status>();
        }

        #region Tokens変更通知プロパティ
        private Tokens _Tokens;
        public Tokens Tokens
        {
            get { return this._Tokens; }
            set { this.SetProperty(ref this._Tokens, value); }
        }
        #endregion

        #region Updating変更通知プロパティ
        private bool _Updating;
        public bool Updating
        {
            get { return this._Updating; }
            set { this.SetProperty(ref this._Updating, value); }
        }
        #endregion

        public ObservableCollection<Twitter.Objects.Status> RetweetsOfMe { get; set; }

        public async Task UpdateRetweetsOfMe(long maxid = 0, bool clear = true)
        {
            if (this.Updating)
                return;

            if (this.Tokens == null)
                return;

            this.Updating = true;

            if (maxid == 0 && clear)
                this.RetweetsOfMe.Clear();
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"count", 20},
                    {"tweet_mode", CoreTweet.TweetMode.extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);

                var listStatus = await Tokens.Statuses.RetweetsOfMeAsync(param);

                if (maxid == 0 && clear)
                    this.RetweetsOfMe.Clear();

                foreach (var status in listStatus)
                {
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.Tokens.UserId, new List<string>() { "none://" }, false));

                    var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                    var index = this.RetweetsOfMe.IndexOf(this.RetweetsOfMe.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) == id));
                    if (index == -1)
                    {
                        index = this.RetweetsOfMe.IndexOf(this.RetweetsOfMe.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) < id));
                        if (index == -1)
                            this.RetweetsOfMe.Add(status);
                        else
                            this.RetweetsOfMe.Insert(index, status);
                    }
                }
            }
            catch
            {
                if (maxid == 0 && clear)
                    this.RetweetsOfMe.Clear();

                this.Updating = false;
                return;
            }

            this.Updating = false;
        }
    }
}
