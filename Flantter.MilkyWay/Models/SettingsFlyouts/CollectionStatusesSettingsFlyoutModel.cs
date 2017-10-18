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
    public class CollectionStatusesSettingsFlyoutModel : BindableBase
    {
        public CollectionStatusesSettingsFlyoutModel()
        {
            CollectionStatuses = new ObservableCollection<CollectionEntry>();
        }

        public ObservableCollection<CollectionEntry> CollectionStatuses { get; set; }

        public async Task UpdateCollectionStatuses(long maxposition = 0, bool clear = true)
        {
            if (Updating)
                return;

            if (string.IsNullOrWhiteSpace(_id) || Tokens == null)
                return;

            Updating = true;

            if (maxposition == 0 && clear)
                CollectionStatuses.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"id", _id},
                    {"count", 20}
                };
                if (maxposition != 0)
                    param.Add("max_position", maxposition);

                var collectionStatuses = await Tokens.Collections.EntriesAsync(param);
                if (maxposition == 0 && clear)
                    CollectionStatuses.Clear();

                foreach (var item in collectionStatuses)
                {
                    Connecter.Instance.TweetReceive_OnCommandExecute(this,
                        new TweetEventArgs(item.Status, Tokens.UserId, new List<string> {"none://"}, false));

                    if (CollectionStatuses.All(x => x.Status.Id != item.Status.Id))
                        CollectionStatuses.Add(item);
                }
            }
            catch
            {
                if (maxposition == 0 && clear)
                    CollectionStatuses.Clear();

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

        private string _id;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        #endregion
    }
}