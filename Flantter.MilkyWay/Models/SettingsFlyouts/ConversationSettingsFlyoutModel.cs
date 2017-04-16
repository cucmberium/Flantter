using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Services.Database;
using Flantter.MilkyWay.Models.Twitter;
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
    public class ConversationSettingsFlyoutModel : BindableBase
    {
        public ConversationSettingsFlyoutModel()
        {
            this.Conversation = new ObservableCollection<Twitter.Objects.Status>();
        }

        #region Tokens変更通知プロパティ
        private Tokens _Tokens;
        public Tokens Tokens
        {
            get { return this._Tokens; }
            set { this.SetProperty(ref this._Tokens, value); }
        }
        #endregion

        #region ConversationId変更通知プロパティ
        private Twitter.Objects.Status _ConversationStatus;
        public Twitter.Objects.Status ConversationStatus
        {
            get { return this._ConversationStatus; }
            set { this.SetProperty(ref this._ConversationStatus, value); }
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

        public ObservableCollection<Twitter.Objects.Status> Conversation { get; set; }

        public async Task UpdateConversation()
        {
            if (this.Updating)
                return;

            if (this.ConversationStatus == null || this.Tokens == null)
                return;

            this.Updating = true;

            this.Conversation.Clear();

            var updateCount = 0;

            long nextId = this.ConversationStatus.HasRetweetInformation ? this.ConversationStatus.RetweetInformation.Id : this.ConversationStatus.Id;

            if (SettingService.Setting.UseOfficialApi && TwitterConnectionHelper.OfficialConsumerKeyList.Contains(this.Tokens.ConsumerKey))
            {
                CoreTweet.AsyncResponse res;
                try
                {
                    res = await this.Tokens.TwitterTokens.SendRequestAsync(CoreTweet.MethodType.Get, "https://api.twitter.com/1.1/conversation/show.json", new Dictionary<string, object>() { { "id", nextId }, { "tweet_mode", CoreTweet.TweetMode.extended } });
                    var json = await res.Source.Content.ReadAsStringAsync();
                    var statuses = CoreTweet.Core.CoreBase.ConvertArray<CoreTweet.Status>(json, string.Empty);

                    statuses.Reverse();

                    this.Conversation.Clear();

                    foreach (var item in statuses)
                    {
                        var statusObject = new Twitter.Objects.Status(item);
                        Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(statusObject, this.Tokens.UserId, new List<string>() { "none://" }, false));

                        statusObject.InReplyToStatusId = 0;

                        this.Conversation.Add(statusObject);                        
                    }
                }
                catch
                {
                    this.Updating = false;
                    return;
                }
            }
            else if (SettingService.Setting.UseExtendedConversation && this.ConversationStatus.CreatedAt.ToLocalTime() + TimeSpan.FromDays(7) > DateTime.Now)
            {
                var conversation = new List<Twitter.Objects.Status>();
                foreach (var user in ConversationStatus.Entities.UserMentions)
                {
                    var conversationTweets = await this.Tokens.Search.TweetsAsync(q => "from:" + this.ConversationStatus.User.ScreenName + " to:" + user.ScreenName, count => 100, tweet_mode => CoreTweet.TweetMode.extended);
                    foreach (var item in conversationTweets)
                    {
                        conversation.Add(item);
                        Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(item, this.Tokens.UserId, new List<string>() { "none://" }, false));
                    }
                    conversationTweets = await this.Tokens.Search.TweetsAsync(q => "from:" + user.ScreenName + " to:" + this.ConversationStatus.User.ScreenName, count => 100, tweet_mode => CoreTweet.TweetMode.extended);
                    foreach (var item in conversationTweets)
                    {
                        conversation.Add(item);
                        Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(item, this.Tokens.UserId, new List<string>() { "none://" }, false));
                    }

                    while (true)
                    {
                        var status = SettingService.Setting.EnableDatabase ? Database.Instance.GetReplyStatusFromId(nextId) : null;

                        if (status == null && !conversation.Any(x => x.InReplyToStatusId == nextId))
                            break;

                        status = status ?? conversation.First(x => x.InReplyToStatusId == nextId);

                        status.InReplyToStatusId = 0;
                        this.Conversation.Insert(0, status);
                        nextId = status.Id;
                    }

                    nextId = this.ConversationStatus.HasRetweetInformation ? this.ConversationStatus.RetweetInformation.Id : this.ConversationStatus.Id;
                    while (true)
                    {
                        var status = SettingService.Setting.EnableDatabase ? Database.Instance.GetStatusFromId(nextId) : null;
                        if (status == null)
                        {
                            if (conversation.Any(x => x.Id == nextId))
                            {
                                status = conversation.First(x => x.Id == nextId);
                            }
                            else
                            {
                                Twitter.Objects.Status item;
                                try
                                {
                                    item = await this.Tokens.Statuses.ShowAsync(id => nextId, include_entities => true, tweet_mode => CoreTweet.TweetMode.extended);
                                }
                                catch
                                {
                                    this.Updating = false;
                                    return;
                                }

                                status = item;
                                Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.Tokens.UserId, new List<string>() { "none://" }, false));
                            }
                        }

                        nextId = status.InReplyToStatusId;

                        status.InReplyToStatusId = 0;

                        this.Conversation.Add(status);

                        if (nextId == 0 || updateCount > 20)
                            break;
                    }
                }
            }
            else
            {
                while (true)
                {
                    var status = SettingService.Setting.EnableDatabase ? Database.Instance.GetReplyStatusFromId(nextId) : null;

                    if (status == null)
                        break;

                    status.InReplyToStatusId = 0;

                    this.Conversation.Insert(0, status);

                    nextId = status.Id;
                }

                nextId = this.ConversationStatus.HasRetweetInformation ? this.ConversationStatus.RetweetInformation.Id : this.ConversationStatus.Id;
                while (true)
                {
                    updateCount += 1;

                    var status = SettingService.Setting.EnableDatabase ? Database.Instance.GetStatusFromId(nextId) : null;

                    if (status == null)
                    {
                        Twitter.Objects.Status item;
                        try
                        {
                            item = await this.Tokens.Statuses.ShowAsync(id => nextId, include_entities => true, tweet_mode => CoreTweet.TweetMode.extended);
                        }
                        catch
                        {
                            this.Updating = false;
                            return;
                        }

                        status = item;
                        Connecter.Instance.TweetReceive_OnCommandExecute(this, new TweetEventArgs(status, this.Tokens.UserId, new List<string>() { "none://" }, false));
                    }

                    nextId = status.InReplyToStatusId;

                    status.InReplyToStatusId = 0;

                    this.Conversation.Add(status);

                    if (nextId == 0 || updateCount > 20)
                        break;
                }
            }

            this.Updating = false;
        }
    }
}
