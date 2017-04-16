using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Services.Database;
using Flantter.MilkyWay.Models.Twitter;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class ConversationSettingsFlyoutModel : BindableBase
    {
        public ConversationSettingsFlyoutModel()
        {
            Conversation = new ObservableCollection<Status>();
        }

        public ObservableCollection<Status> Conversation { get; set; }

        public async Task UpdateConversation()
        {
            if (Updating)
                return;

            if (ConversationStatus == null || Tokens == null)
                return;

            Updating = true;

            Conversation.Clear();

            var updateCount = 0;

            var nextId = ConversationStatus.HasRetweetInformation
                ? ConversationStatus.RetweetInformation.Id
                : ConversationStatus.Id;

            if (SettingService.Setting.UseOfficialApi &&
                TwitterConnectionHelper.OfficialConsumerKeyList.Contains(Tokens.TwitterTokens.ConsumerKey))
            {
                try
                {
                    var res = await Tokens.TwitterTokens.SendRequestAsync(CoreTweet.MethodType.Get,
                        "https://api.twitter.com/1.1/conversation/show.json",
                        new Dictionary<string, object> {{"id", nextId}, {"tweet_mode", CoreTweet.TweetMode.extended}});
                    var json = await res.Source.Content.ReadAsStringAsync();
                    var statuses = CoreTweet.Core.CoreBase.ConvertArray<CoreTweet.Status>(json, string.Empty);

                    statuses.Reverse();

                    Conversation.Clear();

                    foreach (var item in statuses)
                    {
                        var statusObject = new Status(item);
                        Connecter.Instance.TweetReceive_OnCommandExecute(this,
                            new TweetEventArgs(statusObject, Tokens.UserId, new List<string> {"none://"}, false));

                        statusObject.InReplyToStatusId = 0;

                        Conversation.Add(statusObject);
                    }
                }
                catch
                {
                    Updating = false;
                    return;
                }
            }
            else if (SettingService.Setting.UseExtendedConversation &&
                     ConversationStatus.CreatedAt.ToLocalTime() + TimeSpan.FromDays(7) > DateTime.Now)
            {
                var conversation = new List<Status>();
                foreach (var user in ConversationStatus.Entities.UserMentions)
                {
                    var conversationTweets =
                        await Tokens.Search.TweetsAsync(
                            q => "from:" + ConversationStatus.User.ScreenName + " to:" + user.ScreenName, count => 100,
                            tweet_mode => CoreTweet.TweetMode.extended);
                    foreach (var item in conversationTweets)
                    {
                        conversation.Add(item);
                        Connecter.Instance.TweetReceive_OnCommandExecute(this,
                            new TweetEventArgs(item, Tokens.UserId, new List<string> {"none://"}, false));
                    }
                    conversationTweets =
                        await Tokens.Search.TweetsAsync(
                            q => "from:" + user.ScreenName + " to:" + ConversationStatus.User.ScreenName, count => 100,
                            tweet_mode => CoreTweet.TweetMode.extended);
                    foreach (var item in conversationTweets)
                    {
                        conversation.Add(item);
                        Connecter.Instance.TweetReceive_OnCommandExecute(this,
                            new TweetEventArgs(item, Tokens.UserId, new List<string> {"none://"}, false));
                    }

                    while (true)
                    {
                        var status = SettingService.Setting.EnableDatabase
                            ? Database.Instance.GetReplyStatusFromId(nextId)
                            : null;

                        if (status == null && !conversation.Any(x => x.InReplyToStatusId == nextId))
                            break;

                        status = status ?? conversation.First(x => x.InReplyToStatusId == nextId);

                        status.InReplyToStatusId = 0;
                        Conversation.Insert(0, status);
                        nextId = status.Id;
                    }

                    nextId = ConversationStatus.HasRetweetInformation
                        ? ConversationStatus.RetweetInformation.Id
                        : ConversationStatus.Id;
                    while (true)
                    {
                        var status = SettingService.Setting.EnableDatabase
                            ? Database.Instance.GetStatusFromId(nextId)
                            : null;
                        if (status == null)
                            if (conversation.Any(x => x.Id == nextId))
                            {
                                status = conversation.First(x => x.Id == nextId);
                            }
                            else
                            {
                                Status item;
                                try
                                {
                                    item = await Tokens.Statuses.ShowAsync(id => nextId, include_entities => true,
                                        tweet_mode => CoreTweet.TweetMode.extended);
                                }
                                catch
                                {
                                    Updating = false;
                                    return;
                                }

                                status = item;
                                Connecter.Instance.TweetReceive_OnCommandExecute(this,
                                    new TweetEventArgs(status, Tokens.UserId, new List<string> {"none://"}, false));
                            }

                        nextId = status.InReplyToStatusId;

                        status.InReplyToStatusId = 0;

                        Conversation.Add(status);

                        if (nextId == 0 || updateCount > 20)
                            break;
                    }
                }
            }
            else
            {
                while (true)
                {
                    var status = SettingService.Setting.EnableDatabase
                        ? Database.Instance.GetReplyStatusFromId(nextId)
                        : null;

                    if (status == null)
                        break;

                    status.InReplyToStatusId = 0;

                    Conversation.Insert(0, status);

                    nextId = status.Id;
                }

                nextId = ConversationStatus.HasRetweetInformation
                    ? ConversationStatus.RetweetInformation.Id
                    : ConversationStatus.Id;
                while (true)
                {
                    updateCount += 1;

                    var status = SettingService.Setting.EnableDatabase
                        ? Database.Instance.GetStatusFromId(nextId)
                        : null;

                    if (status == null)
                    {
                        Status item;
                        try
                        {
                            item = await Tokens.Statuses.ShowAsync(id => nextId, include_entities => true,
                                tweet_mode => CoreTweet.TweetMode.extended);
                        }
                        catch
                        {
                            Updating = false;
                            return;
                        }

                        status = item;
                        Connecter.Instance.TweetReceive_OnCommandExecute(this,
                            new TweetEventArgs(status, Tokens.UserId, new List<string> {"none://"}, false));
                    }

                    nextId = status.InReplyToStatusId;

                    status.InReplyToStatusId = 0;

                    Conversation.Add(status);

                    if (nextId == 0 || updateCount > 20)
                        break;
                }
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

        #region ConversationId変更通知プロパティ

        private Status _conversationStatus;

        public Status ConversationStatus
        {
            get => _conversationStatus;
            set => SetProperty(ref _conversationStatus, value);
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