using CoreTweet;
using CoreTweet.Core;
using Flantter.MilkyWay.Common;
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
    public class ListStatusesSettingsFlyoutModel : BindableBase
    {
        public ListStatusesSettingsFlyoutModel()
        {
            this.ListStatuses = new ObservableCollection<Twitter.Objects.Status>();
        }

        #region Tokens変更通知プロパティ
        private CoreTweet.Tokens _Tokens;
        public CoreTweet.Tokens Tokens
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

        #region Id変更通知プロパティ
        private long _Id;
        public long Id
        {
            get { return this._Id; }
            set { this.SetProperty(ref this._Id, value); }
        }
        #endregion

        public ObservableCollection<Twitter.Objects.Status> ListStatuses { get; set; }

        public async Task UpdateListStatuses(long maxid = 0, bool clear = true)
        {
            if (this.Updating)
                return;

            if (this._Id == 0 || this.Tokens == null)
                return;

            this.Updating = true;

            if (maxid == 0 && clear)
                this.ListStatuses.Clear();

            ListedResponse<Status> listStatus;
            try
            {
                if (maxid == 0)
                    listStatus = await Tokens.Lists.StatusesAsync(list_id => this._Id, count => 20);
                else
                    listStatus = await Tokens.Lists.StatusesAsync(list_id => this._Id, count => 20, max_id => maxid);
            }
            catch
            {
                if (maxid == 0 && clear)
                    this.ListStatuses.Clear();

                this.Updating = false;
                return;
            }

            if (maxid == 0 && clear)
                this.ListStatuses.Clear();

            foreach (var item in listStatus)
            {
                var status = new Twitter.Objects.Status(item);

                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = this.ListStatuses.IndexOf(this.ListStatuses.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) == id));
                if (index == -1)
                {
                    index = this.ListStatuses.IndexOf(this.ListStatuses.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) < id));
                    if (index == -1)
                        this.ListStatuses.Add(status);
                    else
                        this.ListStatuses.Insert(index, status);
                }
            }

            // Todo : 受信したツイートをデータベースに登録

            this.Updating = false;
        }
    }
}
