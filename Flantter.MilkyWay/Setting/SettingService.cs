using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Themes;
using Windows.Globalization;

namespace Flantter.MilkyWay.Setting
{
    public static class SettingSupport
    {
        public enum SizeEnum
        {
            ExtraSmall = 40,
            Small = 45,
            Medium = 50,
            Large = 55,
        }

        public enum DoubleTappedEventEnum
        {
            None = 0,
            TweetDetail = 1,
            UserProfile = 2,
            Favorite = 3,
            Reply = 4,
            Retweet = 5,
        }

        public enum TrendsPlaceEnum
        {
            Default = 0,
            Global = 1,
            Japan = 2,
            UnitedStates = 3,
            UnitedKingdom = 4,
            Canada = 5,
            Australia = 6,
        }

        public enum TileNotificationEnum
        {
            None = 0,
            Mentions = 1,
            Images = 2,
        }

        public enum TweetAnimationEnum
        {
            None = 0,
            Expand = 1,
            Slide = 2,
            ScrollToTop = 3,
        }

        public enum ThemeEnum
        {
            Light = 0,
            Dark = 1,
        }

        public enum ColumnTypeEnum
        {
            Home = 0,
            Mentions = 1,
            DirectMessages = 2,
            Events = 3,
            Favorites = 4,
            Search = 5,
            UserTimeline = 6,
            List = 7,
            Filter = 8,
        }

        public static long GetTrendsWoeId(TrendsPlaceEnum place)
        {
            switch (place)
            {
                case TrendsPlaceEnum.Default:
                    switch (ApplicationLanguages.Languages.First())
                    {
                        case "ja":
                            return 23424856;
                        default:
                            return 1;
                    }
                case TrendsPlaceEnum.Global:
                    return 1;
                case TrendsPlaceEnum.Japan:
                    return 23424856;
                case TrendsPlaceEnum.UnitedStates:
                    return 23424977;
                case TrendsPlaceEnum.UnitedKingdom:
                    return 23424975;
                case TrendsPlaceEnum.Canada:
                    return 23424775;
                case TrendsPlaceEnum.Australia:
                    return 23424748;

            }

            return 1;
        }
    }

    public class SettingService : SettingServiceBase<SettingService>
    {
        // 通知設定
        public bool FavoriteNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool UnfavoriteNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool FollowNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool RetweetNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool MentionNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool DirectMessageNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool SystemNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool StreamNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool AchievementNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool NotificationSound { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public SettingSupport.TileNotificationEnum TileNotification { get { return (SettingSupport.TileNotificationEnum)GetValue(0); } set { SetValue((int)value); OnPropertyChanged(); } }

        // 表示設定
        public string Theme { get { return GetValue("Dark"); } set { SetValue(value); OnPropertyChanged(); ThemeService.Theme.ChangeTheme(); } }
        public double FontSize { get { return GetValue(12.0); } set { SetValue(value); OnPropertyChanged(); } }
        public double ColumnBackgroundBrushAlpha { get { return GetValue(255.0); } set { SetValue(value); OnPropertyChanged(); ThemeService.Theme.ChangeBackgroundAlpha(); } }
        public double TweetBackgroundBrushAlpha { get { return GetValue(10.0); } set { SetValue(value); OnPropertyChanged(); ThemeService.Theme.ChangeBackgroundAlpha(); } }
        public double MinColumnSize { get { return GetValue(336.0); } set { SetValue(value); OnPropertyChanged(); } }
        public int MaxColumnCount { get { return GetValue(2); } set { SetValue(value); OnPropertyChanged(); } }
        public double TweetPostFieldFontSize { get { return GetValue(14.5); } set { SetValue(value); OnPropertyChanged(); } }
        public double TweetCommandBarHeight { get { return GetValue(40.0); } set { SetValue(value); OnPropertyChanged(); } }
        public SettingSupport.SizeEnum IconSize { get { return (SettingSupport.SizeEnum)GetValue(45); } set { SetValue((int)value); OnPropertyChanged(); } }
        public SettingSupport.TweetAnimationEnum TweetAnimation { get { return (SettingSupport.TweetAnimationEnum)GetValue(3); } set { SetValue((int)value); OnPropertyChanged(); } }
        public double TweetMediaThumbnailSize { get { return GetValue(100.0); } set { SetValue(value); OnPropertyChanged(); } }

        [LocalValue]
        public bool UseBackgroundImage { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); OnPropertyChanged("BackgroundImagePath"); } }
        [LocalValue]
        public string BackgroundImagePath 
        { 
            get 
            {
                if (UseBackgroundImage)
                    return GetValue(string.Empty);
                else 
                    return string.Empty;
            } 
            set { SetValue(value); OnPropertyChanged(); } }

        // 動作設定
        public bool AutoTitleBarVisibility { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool TitleBarVisibility { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool ShowRetweetToMentionColumn { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool CloseBottomAppBarAfterTweet { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); } }
        public bool ShowFavoriteConfirmDialog { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool ShowRetweetConfirmDialog { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool RemoveRetweetAlreadyReceive { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool UnlockAfterScroll { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool DisableStreamingScroll { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); } }
        public bool BottomBarSearchBoxEnabled { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool PreventForcedTermination { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool EnableDatabase { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public SettingSupport.DoubleTappedEventEnum DoubleTappedAction { get { return (SettingSupport.DoubleTappedEventEnum)GetValue(0); } set { SetValue((int)value); OnPropertyChanged(); } }
        public SettingSupport.TrendsPlaceEnum TrendsPlace { get { return (SettingSupport.TrendsPlaceEnum)GetValue(0); } set { SetValue((int)value); OnPropertyChanged(); } }

        // Mute設定
        public string MuteFilter { get { return GetValue("(False)"); } set { SetValue(value); OnPropertyChanged(); } }

        // 上級者向け設定
        public bool UseOfficialApi { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); } }
        public bool UseExtendedConversation { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); } }
		
        [LocalValue]
        public string CustomFontName { get { return GetValue("Global User Interface"); } set { SetValue(value); OnPropertyChanged(); } }

        [LocalValue]
        public bool UseCustomTheme { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); ThemeService.Theme.ChangeTheme(); } }
        [LocalValue]
        public string CustomThemePath { get { return GetValue(string.Empty); } set { SetValue(value); OnPropertyChanged(); ThemeService.Theme.ChangeTheme(); } }

        // その他
        public string AdvancedSettingData { get { return GetValue(string.Empty); } set { SetValue(value); OnPropertyChanged(); } }
    }

    public class SettingProvider
    {
        public SettingService Setting { get { return SettingService.Setting; } }
    }
}
