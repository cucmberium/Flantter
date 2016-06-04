using CoreTweet;
using CoreTweet.Core;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Twitter;
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
        private CoreTweet.Tokens _Tokens;
        public CoreTweet.Tokens Tokens
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
                AsyncResponse res;
                try
                {
                    res = await this.Tokens.SendRequestAsync(MethodType.Get, "https://api.twitter.com/1.1/conversation/show.json", new Dictionary<string, object>() { { "id", nextId }, { "tweet_mode", TweetMode.extended } });
                    var json = await res.Source.Content.ReadAsStringAsync();
                    var statuses = CoreBase.ConvertArray<Status>(json, string.Empty);

                    statuses.Reverse();

                    this.Conversation.Clear();

                    foreach (var item in statuses)
                    {
                        item.InReplyToStatusId = 0;
                        this.Conversation.Add(new Twitter.Objects.Status(item));
                    }
                }
                catch
                {
                    this.Updating = false;
                    return;
                }
            }
            else
            {
                if (SettingService.Setting.UseExtendedConversation && this.ConversationStatus.CreatedAt.ToLocalTime() + TimeSpan.FromDays(7) > DateTime.Now)
                {
                    var conversation = new List<Status>();

                    foreach (var user in ConversationStatus.Entities.UserMentions)
                    {
                        try
                        {
                            var conversationTweets = await this.Tokens.Search.TweetsAsync(q => "from:" + this.ConversationStatus.User.ScreenName + " to:" + user.ScreenName, count => 100, tweet_mode => TweetMode.extended);
                            foreach (var status in conversationTweets)
                            {
                                conversation.Add(status);
                                // Todo : データベースに登録
                            }
                            conversationTweets = await this.Tokens.Search.TweetsAsync(q => "from:" + user.ScreenName + " to:" + this.ConversationStatus.User.ScreenName, count => 100, tweet_mode => TweetMode.extended);
                            foreach (var status in conversationTweets)
                            {
                                conversation.Add(status);
                                // Todo : データベースに登録
                            }

                            while (true)
                            {
                                var items = conversation.Where(x => x.InReplyToStatusId == nextId);
                                if (items.Count() > 0)
                                {
                                    var item = items.First();

                                    var status = new Twitter.Objects.Status(item);
                                    status.InReplyToStatusId = 0;

                                    this.Conversation.Insert(0, status);

                                    nextId = item.Id;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            nextId = this.ConversationStatus.HasRetweetInformation ? this.ConversationStatus.RetweetInformation.Id : this.ConversationStatus.Id;
                            while (true)
                            {
                                var items = conversation.Where(x => x.Id == nextId);
                                if (items.Count() > 0)
                                {
                                    var item = items.First();
                                    
                                    nextId = item.InReplyToStatusId.HasValue ? item.InReplyToStatusId.Value : 0;

                                    var status = new Twitter.Objects.Status(item);
                                    status.InReplyToStatusId = 0;

                                    this.Conversation.Add(status);

                                    if (nextId == 0)
                                        break;
                                }
                                else
                                {
                                    updateCount += 1;
                                    Status item;
                                    try
                                    {
                                        item = await this.Tokens.Statuses.ShowAsync(id => nextId, include_entities => true, tweet_mode => TweetMode.extended);
                                    }
                                    catch
                                    {
                                        this.Updating = false;
                                        return;
                                    }

                                    if (item.RetweetedStatus != null)
                                        nextId = item.RetweetedStatus.InReplyToStatusId.HasValue ? item.RetweetedStatus.InReplyToStatusId.Value : 0;
                                    else
                                        nextId = item.InReplyToStatusId.HasValue ? item.InReplyToStatusId.Value : 0;

                                    var status = new Twitter.Objects.Status(item);
                                    status.InReplyToStatusId = 0;

                                    this.Conversation.Add(status);

                                    if (nextId == 0 || updateCount > 20)
                                        break;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    nextId = this.ConversationStatus.HasRetweetInformation ? this.ConversationStatus.RetweetInformation.Id : this.ConversationStatus.Id;
                    while (true)
                    {
                        // Todo : データベースから過去のツイートを抽出

                        updateCount += 1;
                        Status item;
                        try
                        {
                            item = await this.Tokens.Statuses.ShowAsync(id => nextId, include_entities => true, tweet_mode => TweetMode.extended);
                        }
                        catch
                        {
                            this.Updating = false;
                            return;
                        }

                        if (item.RetweetedStatus != null)
                            nextId = item.RetweetedStatus.InReplyToStatusId.HasValue ? item.RetweetedStatus.InReplyToStatusId.Value : 0;
                        else
                            nextId = item.InReplyToStatusId.HasValue ? item.InReplyToStatusId.Value : 0;

                        var status = new Twitter.Objects.Status(item);
                        status.InReplyToStatusId = 0;

                        this.Conversation.Add(status);

                        if (nextId == 0 || updateCount > 20)
                            break;
                    }
                }
            }

            this.Updating = false;
        }
    }
}
