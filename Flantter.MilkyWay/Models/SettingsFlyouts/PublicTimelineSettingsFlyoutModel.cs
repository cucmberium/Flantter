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
    public class PublicTimelineSettingsFlyoutModel : BindableBase
    {
        public PublicTimelineSettingsFlyoutModel()
        {
            PublicTimelineStatuses = new ObservableCollection<Status>();
        }

        public ObservableCollection<Status> PublicTimelineStatuses { get; set; }

        public async Task UpdatePublicTimeline(long maxid = 0, bool clear = true)
        {
            if (Updating)
                return;

            if (Tokens == null)
                return;

            Updating = true;

            if (maxid == 0 && clear)
                PublicTimelineStatuses.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 20}
                };
                if (_type == "Local")
                    param.Add("local", true);
                if (maxid != 0)
                    param.Add("max_id", maxid);

                var publicTimelineStatuses = await Tokens.Statuses.PublicTimelineAsync(param);
                if (maxid == 0 && clear)
                    PublicTimelineStatuses.Clear();

                foreach (var status in publicTimelineStatuses)
                {
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(status, Tokens.UserId, new List<string> {"none://"}, false));

                    var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                    var index = PublicTimelineStatuses.IndexOf(
                        PublicTimelineStatuses.FirstOrDefault(x =>
                            (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) == id));
                    if (index == -1)
                    {
                        index = PublicTimelineStatuses.IndexOf(
                            PublicTimelineStatuses.FirstOrDefault(
                                x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) < id));
                        if (index == -1)
                            PublicTimelineStatuses.Add(status);
                        else
                            PublicTimelineStatuses.Insert(index, status);
                    }
                }
            }
            catch
            {
                if (maxid == 0 && clear)
                    PublicTimelineStatuses.Clear();

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

        #region Type変更通知プロパティ

        private string _type;

        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        #endregion
    }
}