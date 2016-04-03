using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels.Services
{
    public class Notice
    {
        private static Notice _Instance = new Notice();
        private Notice()
        {
            this.ShowUserProfileCommand = new ReactiveCommand();
            this.ShowConversationCommand = new ReactiveCommand();
            this.ShowMediaCommand = new ReactiveCommand();
            this.ShowStatusDetailCommand = new ReactiveCommand();
            this.LoadMentionCommand = new ReactiveCommand();
            this.ReplyCommand = new ReactiveCommand();
            this.RetweetCommand = new ReactiveCommand();
            this.FavoriteCommand = new ReactiveCommand();
            this.RetweetFavoriteCommand = new ReactiveCommand();
            this.UrlClickCommand = new ReactiveCommand();
            this.ReplyToAllCommand = new ReactiveCommand();
            this.SendDirectMessageCommand = new ReactiveCommand();
            this.UrlQuoteRetweetCommand = new ReactiveCommand();
            this.CopyTweetCommand = new ReactiveCommand();
            this.ShowRetweetersCommand = new ReactiveCommand();
            this.MuteUserCommand = new ReactiveCommand();
            this.MuteClientCommand = new ReactiveCommand();
            this.DeleteTweetCommand = new ReactiveCommand();
            this.DeleteRetweetCommand = new ReactiveCommand();
            this.ShowUserListsCommand = new ReactiveCommand();
            this.OpenStatusUrlCommand = new ReactiveCommand();
            this.ShowSearchCommand = new ReactiveCommand();
            this.ShareStatusCommand = new ReactiveCommand();
            this.ShowListMembersCommand = new ReactiveCommand();
            this.ShowListStatusesCommand = new ReactiveCommand();
            this.ShowRetweetsOfMeCommand = new ReactiveCommand();
            this.ShowUserFollowInfoCommand = new ReactiveCommand();

            this.TweetAreaAccountChangeCommand = new ReactiveCommand();
            this.TweetAreaDeletePictureCommand = new ReactiveCommand();
            this.TweetAreaOpenCommand = new ReactiveCommand();

            this.SearchSettingsFlyoutDeleteSearchQueryCommand = new ReactiveCommand();

            this.ShowSettingsFlyoutCommand = new ReactiveCommand();

            this.AddColumnCommand = new ReactiveCommand();

            this.ShowLeftSwipeMenuCommand = new ReactiveCommand();
            this.ChangeAccountCommand = new ReactiveCommand();
            this.ExitAppCommand = new ReactiveCommand();

            this.ShowMainSettingCommand = new ReactiveCommand();
            this.ShowBehaviorSettingCommand = new ReactiveCommand();
            this.ShowPostingSettingCommand = new ReactiveCommand();
            this.ShowDisplaySettingCommand = new ReactiveCommand();
            this.ShowNotificationSettingCommand = new ReactiveCommand();
            this.ShowAppInfoCommand = new ReactiveCommand();
            this.ShowSupportAccountCommand = new ReactiveCommand();
            this.ChangeBackgroundImageCommand = new ReactiveCommand();
        }

        public static Notice Instance
        {
            get { return _Instance; }
        }
        
        public ReactiveCommand ShowUserProfileCommand { get; private set; }
        public ReactiveCommand ShowConversationCommand { get; private set; }
        public ReactiveCommand ShowMediaCommand { get; private set; }
        public ReactiveCommand ShowStatusDetailCommand { get; private set; }
        public ReactiveCommand LoadMentionCommand { get; private set; }
        public ReactiveCommand ReplyCommand { get; private set; }
        public ReactiveCommand RetweetCommand { get; private set; }
        public ReactiveCommand FavoriteCommand { get; private set; }
        public ReactiveCommand RetweetFavoriteCommand { get; private set; }
        public ReactiveCommand UrlClickCommand { get; private set; }
        public ReactiveCommand ReplyToAllCommand { get; private set; }
        public ReactiveCommand SendDirectMessageCommand { get; private set; }
        public ReactiveCommand UrlQuoteRetweetCommand { get; private set; }
        public ReactiveCommand CopyTweetCommand { get; private set; }
        public ReactiveCommand ShowRetweetersCommand { get; private set; }
        public ReactiveCommand MuteUserCommand { get; private set; }
        public ReactiveCommand MuteClientCommand { get; private set; }
        public ReactiveCommand DeleteTweetCommand { get; private set; }
        public ReactiveCommand DeleteRetweetCommand { get; private set; }
        public ReactiveCommand ShowUserListsCommand { get; private set; }
        public ReactiveCommand OpenStatusUrlCommand { get; private set; }
        public ReactiveCommand ShowSearchCommand { get; private set; }
        public ReactiveCommand ShareStatusCommand { get; private set; }
        public ReactiveCommand ShowListMembersCommand { get; private set; }
        public ReactiveCommand ShowListStatusesCommand { get; private set; }
        public ReactiveCommand ShowRetweetsOfMeCommand { get; private set; }
        public ReactiveCommand ShowUserFollowInfoCommand { get; private set; }

        public ReactiveCommand TweetAreaAccountChangeCommand { get; private set; }
        public ReactiveCommand TweetAreaDeletePictureCommand { get; private set; }
        public ReactiveCommand TweetAreaOpenCommand { get; private set; }

        public ReactiveCommand SearchSettingsFlyoutDeleteSearchQueryCommand { get; private set; }

        public ReactiveCommand ShowSettingsFlyoutCommand { get; private set; }

        public ReactiveCommand AddColumnCommand { get; private set; }


        public ReactiveCommand ShowLeftSwipeMenuCommand { get; private set; }
        public ReactiveCommand ChangeAccountCommand { get; private set; }
        public ReactiveCommand ShowAppSettingsCommand { get; private set; }
        public ReactiveCommand ExitAppCommand { get; private set; }

        public ReactiveCommand ShowMainSettingCommand { get; private set; }
        public ReactiveCommand ShowBehaviorSettingCommand { get; private set; }
        public ReactiveCommand ShowPostingSettingCommand { get; private set; }
        public ReactiveCommand ShowDisplaySettingCommand { get; private set; }
        public ReactiveCommand ShowNotificationSettingCommand { get; private set; }
        public ReactiveCommand ShowAppInfoCommand { get; private set; }
        public ReactiveCommand ShowSupportAccountCommand { get; private set; }
        public ReactiveCommand ChangeBackgroundImageCommand { get; private set; }

    }

    public class NoticeProvider
    {
        public Notice Notice { get { return Notice.Instance; } }
    }
}
