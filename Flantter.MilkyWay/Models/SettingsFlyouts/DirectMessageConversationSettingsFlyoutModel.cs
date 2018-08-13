using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.Notifications;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class DirectMessageConversationSettingsFlyoutModel : BindableBase
    {
        private ResourceLoader _resourceLoader;

        public DirectMessageConversationSettingsFlyoutModel()
        {
            _resourceLoader = new ResourceLoader();
            DirectMessages = new ObservableCollection<DirectMessage>();
        }

        public ObservableCollection<DirectMessage> DirectMessages { get; set; }

        public async Task UpdateUserInfomation()
        {
            if (UpdatingDirectMessages)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            UpdatingDirectMessages = true;

            try
            {
                var user = await Tokens.Users.ShowAsync(user_id => _userId, include_entities => true);
                ScreenName = user.ScreenName;
            }
            catch
            {
                UpdatingDirectMessages = false;
                return;
            }

            UpdatingDirectMessages = false;
        }

        public async Task UpdateDirectMessageConversation(long maxid = 0, bool clear = true)
        {
            if (UpdatingDirectMessages)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            UpdatingDirectMessages = true;


            if (maxid == 0 && clear)
                DirectMessages.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 50}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);

                if (maxid == 0 && clear)
                    DirectMessages.Clear();

                IEnumerable<DirectMessage> directMessages =
                    await Tokens.DirectMessages.ReceivedAsync(param);
                directMessages = directMessages.OrderByDescending(x => x.Id);

                foreach (var directMessage in directMessages)
                {
                    if (directMessage.Sender.ScreenName != _screenName &&
                        directMessage.Recipient.ScreenName != _screenName)
                        continue;

                    var index = DirectMessages.IndexOf(DirectMessages.FirstOrDefault(x => x.Id == directMessage.Id));
                    if (index == -1)
                    {
                        index = DirectMessages.IndexOf(DirectMessages.FirstOrDefault(x => x.Id < directMessage.Id));
                        if (index == -1)
                            DirectMessages.Add(directMessage);
                        else
                            DirectMessages.Insert(index, directMessage);
                    }
                }
            }
            catch
            {
                if (maxid == 0 && clear)
                    DirectMessages.Clear();

                UpdatingDirectMessages = false;
                return;
            }

            UpdatingDirectMessages = false;
        }

        public async Task SendDirectMessage()
        {
            if (SendingDirectMessage)
                return;

            if (_userId == 0 || string.IsNullOrWhiteSpace(_text) || Tokens == null)
                return;

            SendingDirectMessage = true;

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"text", _text.Replace("\r", "\n")},
                    {"recipient_id", _userId}
                };
                var directMessage = await Tokens.DirectMessages.NewAsync(param);
                DirectMessages.Insert(0, directMessage);
            }
            catch (CoreTweet.TwitterException ex)
            {
                SendingDirectMessage = false;
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                SendingDirectMessage = false;
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
                return;
            }
            catch (Exception e)
            {
                SendingDirectMessage = false;
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    e.ToString());
                return;
            }

            Text = "";
            SendingDirectMessage = false;
        }

        #region Tokens変更通知プロパティ

        private Tokens _tokens;

        public Tokens Tokens
        {
            get => _tokens;
            set => SetProperty(ref _tokens, value);
        }

        #endregion

        #region UserId変更通知プロパティ

        private long _userId;

        public long UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        #endregion

        #region ScreenName変更通知プロパティ

        private string _screenName;

        public string ScreenName
        {
            get => _screenName;
            set => SetProperty(ref _screenName, value);
        }

        #endregion

        #region Text変更通知プロパティ

        private string _text;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        #endregion

        #region UpdatingDirectMessages変更通知プロパティ

        private bool _updatingDirectMessages;

        public bool UpdatingDirectMessages
        {
            get => _updatingDirectMessages;
            set => SetProperty(ref _updatingDirectMessages, value);
        }

        #endregion

        #region SendingDirectMessage変更通知プロパティ

        private bool _sendingDirectMessage;

        public bool SendingDirectMessage
        {
            get => _sendingDirectMessage;
            set => SetProperty(ref _sendingDirectMessage, value);
        }

        #endregion
    }
}