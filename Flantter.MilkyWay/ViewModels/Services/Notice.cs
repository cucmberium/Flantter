using System.Reactive.Concurrency;
using System.Threading;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;

namespace Flantter.MilkyWay.ViewModels.Services
{
    public class Notice
    {
        private Notice()
        {
            ShowUserProfileCommand = new ReactiveCommand();
            ShowConversationCommand = new ReactiveCommand();
            ShowMediaCommand = new ReactiveCommand();
            ShowStatusDetailCommand = new ReactiveCommand();
            LoadMentionCommand = new ReactiveCommand();
            ReplyCommand = new ReactiveCommand();
            RetweetCommand = new ReactiveCommand();
            FavoriteCommand = new ReactiveCommand();
            RetweetFavoriteCommand = new ReactiveCommand();
            UrlClickCommand = new ReactiveCommand();
            ReplyToAllCommand = new ReactiveCommand();
            ReplyToStatusesCommand = new ReactiveCommand();
            SendDirectMessageCommand = new ReactiveCommand();
            UrlQuoteRetweetCommand = new ReactiveCommand();
            CopyTweetCommand = new ReactiveCommand();
            CopyTweetUrlCommand = new ReactiveCommand();
            ShowRetweetersCommand = new ReactiveCommand();
            MuteUserCommand = new ReactiveCommand();
            MuteClientCommand = new ReactiveCommand();
            MuteWordCommand = new ReactiveCommand();
            DeleteTweetCommand = new ReactiveCommand();
            DeleteRetweetCommand = new ReactiveCommand();
            DeleteFromCollectionCommand = new ReactiveCommand();
            AddToCollectionCommand = new ReactiveCommand();
            ShowUserListsCommand = new ReactiveCommand();
            ShowUserCollectionsCommand = new ReactiveCommand();
            OpenStatusUrlCommand = new ReactiveCommand();
            ShowSearchCommand = new ReactiveCommand();
            ShareStatusCommand = new ReactiveCommand();
            ShowListMembersCommand = new ReactiveCommand();
            ShowListStatusesCommand = new ReactiveCommand();
            ShowCollectionStatusesCommand = new ReactiveCommand();
            ShowRetweetsOfMeCommand = new ReactiveCommand();
            ShowUserFollowInfoCommand = new ReactiveCommand();
            RetweetStatusesCommand = new ReactiveCommand();
            FavoriteStatusesCommand = new ReactiveCommand();
            ShowMyListsCommand = new ReactiveCommand();
            ShowMyCollectionsCommand = new ReactiveCommand();
            GetGapStatusCommand = new ReactiveCommand();

            AddListColumnCommand = new ReactiveCommand();
            AddFilterColumnCommand = new ReactiveCommand();
            AddCollectionColumnCommand = new ReactiveCommand();

            TweetAreaAccountChangeCommand = new ReactiveCommand();
            TweetAreaDeletePictureCommand = new ReactiveCommand();
            TweetAreaOpenCommand = new ReactiveCommand();

            SearchSettingsFlyoutDeleteSearchQueryCommand = new ReactiveCommand();

            ShowSettingsFlyoutCommand = new ReactiveCommand();

            DonateCommand = new ReactiveCommand();

            AuthAccountCommand = new ReactiveCommand();
            AddAccountCommand = new ReactiveCommand();
            AddColumnCommand = new ReactiveCommand();
            DeleteAccountCommand = new ReactiveCommand();
            DeleteColumnCommand = new ReactiveCommand();

            SortColumnCommand = new ReactiveCommand();
            ChangeColumnSelectedIndexCommand = new ReactiveCommand();

            OpenCollectionCommand = new ReactiveCommand();

            ShowLeftSwipeMenuCommand = new ReactiveCommand();
            ShowChangeAccountCommand = new ReactiveCommand();
            ChangeAccountCommand = new ReactiveCommand();
            ExitAppCommand = new ReactiveCommand();

            ShowMainSettingCommand = new ReactiveCommand();
            ShowBehaviorSettingCommand = new ReactiveCommand();
            ShowPostingSettingCommand = new ReactiveCommand();
            ShowDisplaySettingCommand = new ReactiveCommand();
            ShowNotificationSettingCommand = new ReactiveCommand();
            ShowMuteSettingCommand = new ReactiveCommand();
            ShowDatabaseSettingCommand = new ReactiveCommand();
            ShowAccountsSettingCommand = new ReactiveCommand();
            ShowAccountSettingCommand = new ReactiveCommand();
            ShowAdvancedSettingCommand = new ReactiveCommand();
            ShowAppInfoCommand = new ReactiveCommand();
            ShowSupportAccountCommand = new ReactiveCommand();
            ShowColumnSettingCommand = new ReactiveCommand();
            ChangeBackgroundImageCommand = new ReactiveCommand();
            ChangeThemeCommand = new ReactiveCommand();

            DeleteMuteUserCommand = new ReactiveCommand();
            DeleteMuteClientCommand = new ReactiveCommand();
            DeleteMuteWordCommand = new ReactiveCommand();
            UpdateMuteFilterCommand = new ReactiveCommand();

            CopySelectedTweetCommand = new ReactiveCommand();
            ReplyToSelectedTweetCommand = new ReactiveCommand();
            SendDirectMessageToSelectedTweetCommand = new ReactiveCommand();
            FavoriteSelectedTweetCommand = new ReactiveCommand();
            RetweetSelectedTweetCommand = new ReactiveCommand();
            ShowUserProfileOfSelectedTweetCommand = new ReactiveCommand();
            ShowConversationOfSelectedTweetCommand = new ReactiveCommand();
            ChangeSelectedTweetCommand = new ReactiveCommand();

            IncrementColumnSelectedIndexCommand = new ReactiveCommand();
            DecrementColumnSelectedIndexCommand = new ReactiveCommand();

            UpdateAllTimelineCommand = new ReactiveCommand();

            DeleteDatabaseFileCommand = new ReactiveCommand();

            ShowFilePickerMessenger = new Messenger();
            ShowComfirmMessageDialogMessenger = new Messenger();
            ShowMessageDialogMessenger = new Messenger();
            ShowAuthorizePopupMessenger = new Messenger();
        }

        public static Notice Instance { get; } = new Notice();

        public Messenger ShowFilePickerMessenger { get; }
        public Messenger ShowComfirmMessageDialogMessenger { get; }
        public Messenger ShowMessageDialogMessenger { get; }
        public Messenger ShowAuthorizePopupMessenger { get; }

        public ReactiveCommand ShowUserProfileCommand { get; }
        public ReactiveCommand ShowConversationCommand { get; }
        public ReactiveCommand ShowMediaCommand { get; }
        public ReactiveCommand ShowStatusDetailCommand { get; }
        public ReactiveCommand LoadMentionCommand { get; }
        public ReactiveCommand ReplyCommand { get; }
        public ReactiveCommand RetweetCommand { get; }
        public ReactiveCommand FavoriteCommand { get; }
        public ReactiveCommand RetweetFavoriteCommand { get; }
        public ReactiveCommand UrlClickCommand { get; }
        public ReactiveCommand ReplyToAllCommand { get; }
        public ReactiveCommand ReplyToStatusesCommand { get; }
        public ReactiveCommand SendDirectMessageCommand { get; }
        public ReactiveCommand UrlQuoteRetweetCommand { get; }
        public ReactiveCommand CopyTweetCommand { get; }
        public ReactiveCommand CopyTweetUrlCommand { get; }
        public ReactiveCommand ShowRetweetersCommand { get; }
        public ReactiveCommand MuteUserCommand { get; }
        public ReactiveCommand MuteClientCommand { get; }
        public ReactiveCommand MuteWordCommand { get; }
        public ReactiveCommand DeleteTweetCommand { get; }
        public ReactiveCommand DeleteRetweetCommand { get; }
        public ReactiveCommand DeleteFromCollectionCommand { get; }
        public ReactiveCommand AddToCollectionCommand { get; }
        public ReactiveCommand ShowUserListsCommand { get; }
        public ReactiveCommand ShowUserCollectionsCommand { get; }
        public ReactiveCommand OpenStatusUrlCommand { get; }
        public ReactiveCommand ShowSearchCommand { get; }
        public ReactiveCommand ShareStatusCommand { get; }
        public ReactiveCommand ShowListMembersCommand { get; }
        public ReactiveCommand ShowListStatusesCommand { get; }
        public ReactiveCommand ShowCollectionStatusesCommand { get; }
        public ReactiveCommand ShowRetweetsOfMeCommand { get; }
        public ReactiveCommand ShowUserFollowInfoCommand { get; }
        public ReactiveCommand RetweetStatusesCommand { get; }
        public ReactiveCommand FavoriteStatusesCommand { get; }
        public ReactiveCommand ShowMyListsCommand { get; }
        public ReactiveCommand ShowMyCollectionsCommand { get; }
        public ReactiveCommand GetGapStatusCommand { get; }

        public ReactiveCommand AddListColumnCommand { get; }
        public ReactiveCommand AddFilterColumnCommand { get; }
        public ReactiveCommand AddCollectionColumnCommand { get; }

        public ReactiveCommand TweetAreaAccountChangeCommand { get; }
        public ReactiveCommand TweetAreaDeletePictureCommand { get; }
        public ReactiveCommand TweetAreaOpenCommand { get; }

        public ReactiveCommand SearchSettingsFlyoutDeleteSearchQueryCommand { get; }

        public ReactiveCommand ShowSettingsFlyoutCommand { get; }

        public ReactiveCommand DonateCommand { get; }

        public ReactiveCommand AuthAccountCommand { get; }
        public ReactiveCommand AddAccountCommand { get; }
        public ReactiveCommand AddColumnCommand { get; }
        public ReactiveCommand DeleteAccountCommand { get; }
        public ReactiveCommand DeleteColumnCommand { get; }

        public ReactiveCommand SortColumnCommand { get; }
        public ReactiveCommand ChangeColumnSelectedIndexCommand { get; }

        public ReactiveCommand OpenCollectionCommand { get; }

        public ReactiveCommand ShowLeftSwipeMenuCommand { get; }
        public ReactiveCommand ShowChangeAccountCommand { get; }
        public ReactiveCommand ChangeAccountCommand { get; }
        public ReactiveCommand ExitAppCommand { get; }

        public ReactiveCommand ShowMainSettingCommand { get; }
        public ReactiveCommand ShowBehaviorSettingCommand { get; }
        public ReactiveCommand ShowPostingSettingCommand { get; }
        public ReactiveCommand ShowDisplaySettingCommand { get; }
        public ReactiveCommand ShowNotificationSettingCommand { get; }
        public ReactiveCommand ShowMuteSettingCommand { get; }
        public ReactiveCommand ShowDatabaseSettingCommand { get; }
        public ReactiveCommand ShowAccountsSettingCommand { get; }
        public ReactiveCommand ShowAccountSettingCommand { get; }
        public ReactiveCommand ShowAdvancedSettingCommand { get; }
        public ReactiveCommand ShowAppInfoCommand { get; }
        public ReactiveCommand ShowSupportAccountCommand { get; }
        public ReactiveCommand ShowColumnSettingCommand { get; }
        public ReactiveCommand ChangeBackgroundImageCommand { get; }
        public ReactiveCommand ChangeThemeCommand { get; }

        public ReactiveCommand DeleteMuteUserCommand { get; }
        public ReactiveCommand DeleteMuteClientCommand { get; }
        public ReactiveCommand DeleteMuteWordCommand { get; }
        public ReactiveCommand UpdateMuteFilterCommand { get; }

        public ReactiveCommand CopySelectedTweetCommand { get; }
        public ReactiveCommand ReplyToSelectedTweetCommand { get; }
        public ReactiveCommand SendDirectMessageToSelectedTweetCommand { get; }
        public ReactiveCommand FavoriteSelectedTweetCommand { get; }
        public ReactiveCommand RetweetSelectedTweetCommand { get; }
        public ReactiveCommand ShowUserProfileOfSelectedTweetCommand { get; }
        public ReactiveCommand ShowConversationOfSelectedTweetCommand { get; }
        public ReactiveCommand ChangeSelectedTweetCommand { get; }

        public ReactiveCommand IncrementColumnSelectedIndexCommand { get; }
        public ReactiveCommand DecrementColumnSelectedIndexCommand { get; }

        public ReactiveCommand UpdateAllTimelineCommand { get; }

        public ReactiveCommand DeleteDatabaseFileCommand { get; }
    }

    public class NoticeProvider
    {
        public Notice Notice => Notice.Instance;
    }


    public class ShareNotice
    {
        private ShareNotice()
        {
            ShareContractAccountChangeCommand =
                new ReactiveCommand(new SynchronizationContextScheduler(SynchronizationContext.Current));
        }

        public static ShareNotice Instance { get; } = new ShareNotice();

        public ReactiveCommand ShareContractAccountChangeCommand { get; }
    }

    public class ShareNoticeProvider
    {
        public ShareNotice Notice => ShareNotice.Instance;
    }
}