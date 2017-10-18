using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class RetweetersSettingsFlyoutModel : BindableBase
    {
        private long _retweetersCursor;

        public RetweetersSettingsFlyoutModel()
        {
            Retweeters = new ObservableCollection<User>();
        }

        public ObservableCollection<User> Retweeters { get; set; }

        public async Task UpdateRetweeters(bool useCursor = false)
        {
            if (Updating)
                return;

            if (Id == 0 || Tokens == null)
                return;

            if (useCursor && _retweetersCursor == 0)
                return;

            Updating = true;

            if (!useCursor || _retweetersCursor == 0)
                Retweeters.Clear();
            try
            {
                var param = new Dictionary<string, object>
                {
                    {"id", _id}
                };
                if (useCursor && _retweetersCursor != 0)
                    param.Add("cursor", _retweetersCursor);

                var retweeters = await Tokens.Statuses.RetweetersAsync(param);
                if (!useCursor || _retweetersCursor == 0)
                    Retweeters.Clear();

                foreach (var user in retweeters)
                    Retweeters.Add(user);

                _retweetersCursor = retweeters.NextCursor;
            }
            catch
            {
                if (!useCursor || _retweetersCursor == 0)
                    Retweeters.Clear();

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

        #region Id変更通知プロパティ

        private long _id;

        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
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