using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class ListMembersSettingsFlyoutModel : BindableBase
    {
        private long _listMembersCursor;

        public ListMembersSettingsFlyoutModel()
        {
            ListMembers = new ObservableCollection<User>();
        }

        public ObservableCollection<User> ListMembers { get; set; }

        public async Task UpdateListMembers(bool useCursor = false)
        {
            if (Updating)
                return;

            if (Id == 0 || Tokens == null)
                return;

            if (useCursor && _listMembersCursor == 0)
                return;

            Updating = true;

            if (!useCursor || _listMembersCursor == 0)
                ListMembers.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"list_id", _id},
                    {"count", 20}
                };
                if (useCursor && _listMembersCursor != 0)
                    param.Add("cursor", _listMembersCursor);

                var listMembers = await Tokens.Lists.Members.ListAsync(param);
                if (!useCursor || _listMembersCursor == 0)
                    ListMembers.Clear();

                foreach (var user in listMembers)
                    ListMembers.Add(user);

                _listMembersCursor = listMembers.NextCursor;
            }
            catch
            {
                if (!useCursor || _listMembersCursor == 0)
                    ListMembers.Clear();
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