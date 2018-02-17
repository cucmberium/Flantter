using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class FavoritersSettingsFlyoutModel : BindableBase
    {
        private long _favoritersCursor;

        public FavoritersSettingsFlyoutModel()
        {
            Favoriters = new ObservableCollection<User>();
        }

        public ObservableCollection<User> Favoriters { get; set; }

        public async Task UpdateFavoriters(bool useCursor = false)
        {
            if (Updating)
                return;

            if (Id == 0 || Tokens == null)
                return;

            if (useCursor && _favoritersCursor == 0)
                return;

            Updating = true;

            if (!useCursor || _favoritersCursor == 0)
                Favoriters.Clear();
            try
            {
                var param = new Dictionary<string, object>
                {
                    {"id", _id}
                };
                if (useCursor && _favoritersCursor != 0)
                    param.Add("cursor", _favoritersCursor);

                var favoriters = await Tokens.Statuses.FavoritersAsync(param);
                if (!useCursor || _favoritersCursor == 0)
                    Favoriters.Clear();

                foreach (var user in favoriters)
                    Favoriters.Add(user);

                _favoritersCursor = favoriters.NextCursor;
            }
            catch
            {
                if (!useCursor || _favoritersCursor == 0)
                    Favoriters.Clear();

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