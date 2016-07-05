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
using Windows.ApplicationModel.Resources;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class DirectMessageConversationSettingsFlyoutModel : BindableBase
    {
        public DirectMessageConversationSettingsFlyoutModel()
        {
            this.DirectMessages = new ObservableCollection<Twitter.Objects.DirectMessage>();
        }

        #region Tokens変更通知プロパティ
        private CoreTweet.Tokens _Tokens;
        public CoreTweet.Tokens Tokens
        {
            get { return this._Tokens; }
            set { this.SetProperty(ref this._Tokens, value); }
        }
        #endregion

        #region ScreenName変更通知プロパティ
        private string _ScreenName;
        public string ScreenName
        {
            get { return this._ScreenName; }
            set { this.SetProperty(ref this._ScreenName, value); }
        }
        #endregion

        #region Text変更通知プロパティ
        private string _Text;
        public string Text
        {
            get { return this._Text; }
            set { this.SetProperty(ref this._Text, value); }
        }
        #endregion

        #region UpdatingDirectMessages変更通知プロパティ
        private bool _UpdatingDirectMessages;
        public bool UpdatingDirectMessages
        {
            get { return this._UpdatingDirectMessages; }
            set { this.SetProperty(ref this._UpdatingDirectMessages, value); }
        }
        #endregion

        #region SendingDirectMessage変更通知プロパティ
        private bool _SendingDirectMessage;
        public bool SendingDirectMessage
        {
            get { return this._SendingDirectMessage; }
            set { this.SetProperty(ref this._SendingDirectMessage, value); }
        }
        #endregion

        public ObservableCollection<Twitter.Objects.DirectMessage> DirectMessages { get; set; }

        public async Task UpdateDirectMessageConversation(long maxid = 0, bool clear = true)
        {
            if (this.UpdatingDirectMessages)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || this.Tokens == null)
                return;

            this.UpdatingDirectMessages = true;


            if (maxid == 0 && clear)
                this.DirectMessages.Clear();

            // Todo : すべてデータベースから抽出するように?

            ListedResponse<DirectMessage> receivedDirectMessages;
            ListedResponse<DirectMessage> sentDirectMessages;
            try
            {
                if (maxid == 0)
                {
                    receivedDirectMessages = await Tokens.DirectMessages.ReceivedAsync(count => 50, full_text => true);
                    sentDirectMessages = await Tokens.DirectMessages.SentAsync(count => 50, full_text => true);
                }
                else
                {
                    receivedDirectMessages = await Tokens.DirectMessages.ReceivedAsync(count => 50, max_id => maxid, full_text => true);
                    sentDirectMessages = await Tokens.DirectMessages.SentAsync(count => 50, max_id => maxid, full_text => true);
                }
            }
            catch
            {
                if (maxid == 0 && clear)
                    this.DirectMessages.Clear();

                this.UpdatingDirectMessages = false;
                return;
            }

            if (maxid == 0 && clear)
                this.DirectMessages.Clear();

            foreach (var item in receivedDirectMessages)
            {
                var directMessage = new Twitter.Objects.DirectMessage(item);

                if (directMessage.Sender.ScreenName != this._ScreenName)
                    continue;

                var index = this.DirectMessages.IndexOf(this.DirectMessages.FirstOrDefault(x => x.Id == directMessage.Id));
                if (index == -1)
                {
                    index = this.DirectMessages.IndexOf(this.DirectMessages.FirstOrDefault(x => x.Id < directMessage.Id));
                    if (index == -1)
                        this.DirectMessages.Add(directMessage);
                    else
                        this.DirectMessages.Insert(index, directMessage);
                }
            }
            foreach (var item in sentDirectMessages)
            {
                var directMessage = new Twitter.Objects.DirectMessage(item);

                if (directMessage.Recipient.ScreenName != this._ScreenName)
                    continue;

                var index = this.DirectMessages.IndexOf(this.DirectMessages.FirstOrDefault(x => x.Id == directMessage.Id));
                if (index == -1)
                {
                    index = this.DirectMessages.IndexOf(this.DirectMessages.FirstOrDefault(x => x.Id < directMessage.Id));
                    if (index == -1)
                        this.DirectMessages.Add(directMessage);
                    else
                        this.DirectMessages.Insert(index, directMessage);
                }
            }

            this.UpdatingDirectMessages = false;
        }

        public async Task SendDirectMessage()
        {
            if (this.SendingDirectMessage)
                return;

            if (string.IsNullOrWhiteSpace(this._ScreenName) || string.IsNullOrWhiteSpace(this._Text) || this.Tokens == null)
                return;

            this.SendingDirectMessage = true;

            DirectMessageResponse directMessageResponse = null;
            try
            {
                directMessageResponse = await this.Tokens.DirectMessages.NewAsync(text => this._Text, screen_name => this._ScreenName);
            }
            catch (TwitterException ex)
            {
                this.SendingDirectMessage = false;
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (Exception e)
            {
                this.SendingDirectMessage = false;
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                return;
            }

            var directMessage = new Twitter.Objects.DirectMessage(directMessageResponse);
            this.DirectMessages.Insert(0, directMessage);

            this.Text = "";
            this.SendingDirectMessage = false;
        }
    }
}
