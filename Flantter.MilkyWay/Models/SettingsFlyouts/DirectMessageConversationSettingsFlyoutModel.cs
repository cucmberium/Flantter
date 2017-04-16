using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Notifications;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class DirectMessageConversationSettingsFlyoutModel : BindableBase
    {
        public DirectMessageConversationSettingsFlyoutModel()
        {
            DirectMessages = new ObservableCollection<DirectMessage>();
        }

        public ObservableCollection<DirectMessage> DirectMessages { get; set; }

        public async Task UpdateDirectMessageConversation(long maxid = 0, bool clear = true)
        {
            if (UpdatingDirectMessages)
                return;

            if (string.IsNullOrWhiteSpace(_screenName) || Tokens == null)
                return;

            UpdatingDirectMessages = true;


            if (maxid == 0 && clear)
                DirectMessages.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 50},
                    {"include_entities", true},
                    {"full_text", true}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);

                if (maxid == 0 && clear)
                    DirectMessages.Clear();

                IEnumerable<DirectMessage> directMessages =
                    await Tokens.DirectMessages.ReceivedAsync(count => 50, full_text => true);
                if (Tokens.Platform == Tokens.PlatformEnum.Twitter)
                    directMessages = directMessages.Concat(await Tokens.DirectMessages.SentAsync(param));
                directMessages = directMessages.OrderByDescending(x => x.Id);

                foreach (var directMessage in directMessages)
                {
                    if (directMessage.Sender.ScreenName != _screenName)
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

            if (string.IsNullOrWhiteSpace(_screenName) || string.IsNullOrWhiteSpace(_text) || Tokens == null)
                return;

            SendingDirectMessage = true;

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"text", _text},
                    {"screen_name", _screenName}
                };
                if (Tokens.Platform == Tokens.PlatformEnum.Mastodon)
                {
                    var id = DirectMessages.FirstOrDefault(x => x.Sender.Id != Tokens.UserId)?.Id;
                    if (id == 0)
                    {
                        var userTimeline = await Tokens.Statuses.UserTimelineAsync(screen_name => _screenName);
                        id = userTimeline.FirstOrDefault()?.Id;
                    }
                    param.Add("in_reply_to_status_id", id);
                }
                var directMessage = await Tokens.DirectMessages.NewAsync(text => _text, screen_name => _screenName);
                DirectMessages.Insert(0, directMessage);
            }
            catch (CoreTweet.TwitterException ex)
            {
                SendingDirectMessage = false;
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (Exception e)
            {
                SendingDirectMessage = false;
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    new ResourceLoader().GetString("Notification_System_ErrorOccurred"),
                    new ResourceLoader().GetString("Notification_System_CheckNetwork"));
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