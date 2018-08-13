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
    public class DirectMessagesSettingsFlyoutModel : BindableBase
    {
        private string _dmCursor;

        public DirectMessagesSettingsFlyoutModel()
        {
            DirectMessages = new ObservableCollection<DirectMessage>();
        }

        public ObservableCollection<DirectMessage> DirectMessages { get; set; }

        public async Task UpdateDirectMessages(bool useCursor = false, bool clear = true)
        {
            if (Updating)
                return;

            if (Tokens == null)
                return;

            Updating = true;

            if (!useCursor && clear)
                DirectMessages.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 50}
                };

                if (useCursor && string.IsNullOrWhiteSpace(_dmCursor))
                    param.Add("cursor", _dmCursor);

                var directMessages = await Tokens.DirectMessages.ReceivedAsync(param);
                if (!useCursor && clear)
                    DirectMessages.Clear();

                foreach (var directMessage in directMessages)
                {
                    var id = directMessage.Id;
                    var index = DirectMessages.IndexOf(DirectMessages.FirstOrDefault(x => x.Id == id));
                    if (index == -1)
                    {
                        index = DirectMessages.IndexOf(DirectMessages.FirstOrDefault(x => x.Id < id));
                        if (index == -1)
                            DirectMessages.Add(directMessage);
                        else
                            DirectMessages.Insert(index, directMessage);
                    }
                }

                _dmCursor = directMessages.NextCursor;
            }
            catch
            {
                if (!useCursor && clear)
                    DirectMessages.Clear();

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