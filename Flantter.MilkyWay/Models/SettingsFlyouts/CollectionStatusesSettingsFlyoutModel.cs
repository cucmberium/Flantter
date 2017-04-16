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
    public class CollectionStatusesSettingsFlyoutModel : BindableBase
    {
        public CollectionStatusesSettingsFlyoutModel()
        {
            this.CollectionStatuses = new ObservableCollection<Twitter.Objects.CollectionEntry>();
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

        #region Id変更通知プロパティ
        private string _Id;
        public string Id
        {
            get { return this._Id; }
            set { this.SetProperty(ref this._Id, value); }
        }
        #endregion

        public ObservableCollection<Twitter.Objects.CollectionEntry> CollectionStatuses { get; set; }

        public async Task UpdateCollectionStatuses(long maxposition = 0, bool clear = true)
        {
            if (this.Updating)
                return;

            if (string.IsNullOrWhiteSpace(this._Id) || this.Tokens == null)
                return;

            this.Updating = true;

            if (maxposition == 0 && clear)
                this.CollectionStatuses.Clear();
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"id", this._Id},
                    {"count", 20},
                };
                if (maxposition == 0)
                    param.Add("max_position", maxposition);

                var collectionStatuses = await this.Tokens.Collections.EntriesAsync(param);
                if (maxposition == 0 && clear)
                    this.CollectionStatuses.Clear();

                foreach (var item in collectionStatuses)
                {
                    Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(item.Status, this.Tokens.UserId, new List<string>() { "none://" }, false));

                    if (!this.CollectionStatuses.Any(x => x.Status.Id == item.Status.Id))
                        this.CollectionStatuses.Add(item);
                }
            }
            catch
            {
                if (maxposition == 0 && clear)
                    this.CollectionStatuses.Clear();

                this.Updating = false;
                return;
            }

            this.Updating = false;
        }
    }
}
