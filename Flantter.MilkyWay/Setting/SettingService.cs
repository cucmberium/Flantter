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
using SharpDX.DirectWrite;

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

        public enum DoubleTappedActionEnum
        {
            None = 0,
            StatusDetail = 1,
            UserProfile = 2,
            Favorite = 3,
            Reply = 4,
            Retweet = 5,
        }

        public enum TrendsRegionEnum
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

        public static long GetTrendsWoeId(TrendsRegionEnum place)
        {
            switch (place)
            {
                case TrendsRegionEnum.Default:
                    switch (ApplicationLanguages.Languages.First())
                    {
                        case "ja":
                        case "ja-JP":
                            return 23424856;
                        case "en-US":
                            return 23424977;
                        case "en-CA":
                            return 23424775;
                        case "en-AU":
                            return 23424748;
                        case "en-GB":
                            return 23424975;
                        case "en":
                        default:
                            return 1;
                    }

                case TrendsRegionEnum.Global:
                    return 1;
                case TrendsRegionEnum.Japan:
                    return 23424856;
                case TrendsRegionEnum.UnitedStates:
                    return 23424977;
                case TrendsRegionEnum.UnitedKingdom:
                    return 23424975;
                case TrendsRegionEnum.Canada:
                    return 23424775;
                case TrendsRegionEnum.Australia:
                    return 23424748;
                default:
                    return 1;
            }
        }

        public static string GetTrendsPlaceString(TrendsRegionEnum place)
        {
            switch (place)
            {
                case TrendsRegionEnum.Default:
                    switch (ApplicationLanguages.Languages.First().ToLower())
                    {
                        case "ja":
                        case "ja-jp":
                            return "Japan";
                        case "en-us":
                            return "UnitedStates";
                        case "en-ca":
                            return "Canada";
                        case "en-au":
                            return "Australia";
                        case "en-gb":
                            return "UnitedKingdom";
                        case "en":
                        default:
                            return "Global";
                    }
                default:
                    return place.ToString();
            }
        }

        public static IEnumerable<string> GetSystemFontFamilies()
        {
            var fontlist = new List<string>();

            using (var factory = new Factory())
            using (var fontCollection = factory.GetSystemFontCollection(false))
            {
                var familyCount = fontCollection.FontFamilyCount;
                for (int i = 0; i < familyCount; i++)
                {
                    using (var fontFamily = fontCollection.GetFontFamily(i))
                    using (var familyNames = fontFamily.FamilyNames)
                    {
                        int index;
                        familyNames.FindLocaleName("en-us", out index);
                        fontlist.Add(familyNames.GetString(index));
                    }
                }
            }

            fontlist.Add("Global User Interface");
            fontlist.Sort(StringComparer.OrdinalIgnoreCase);
            return fontlist;
        }
    }

    public class SettingSupportProvider
    {
        public static IEnumerable<SettingSupport.DoubleTappedActionEnum> DoubleTappedEventListTypeValues
        {
            get { return Enum.GetValues(typeof(SettingSupport.DoubleTappedActionEnum)).Cast<SettingSupport.DoubleTappedActionEnum>(); }
        }

        public static IEnumerable<SettingSupport.TrendsRegionEnum> TrendsRegionListTypeValues
        {
            get { return Enum.GetValues(typeof(SettingSupport.TrendsRegionEnum)).Cast<SettingSupport.TrendsRegionEnum>(); }
        }

        public static IEnumerable<SettingSupport.SizeEnum> SizeListTypeValues
        {
            get { return Enum.GetValues(typeof(SettingSupport.SizeEnum)).Cast<SettingSupport.SizeEnum>(); }
        }

        public static IEnumerable<SettingSupport.TileNotificationEnum> TileNotificationListTypeValues
        {
            get { return Enum.GetValues(typeof(SettingSupport.TileNotificationEnum)).Cast<SettingSupport.TileNotificationEnum>(); }
        }

        public static IEnumerable<SettingSupport.TweetAnimationEnum> TweetAnimationListTypeValues
        {
            get { return Enum.GetValues(typeof(SettingSupport.TweetAnimationEnum)).Cast<SettingSupport.TweetAnimationEnum>(); }
        }
        public static IEnumerable<SettingSupport.ThemeEnum> ThemeListTypeValues
        {
            get { return Enum.GetValues(typeof(SettingSupport.ThemeEnum)).Cast<SettingSupport.ThemeEnum>(); }
        }
        public static IEnumerable<string> SystemFontFamilies
        {
            get { return SettingSupport.GetSystemFontFamilies(); }
        }
    }

    public class SettingService : SettingServiceBase<SettingService>
    {
        // 動作設定
        public SettingSupport.DoubleTappedActionEnum DoubleTappedAction { get { return (SettingSupport.DoubleTappedActionEnum)GetValue(0); } set { SetValue((int)value); OnPropertyChanged(); } }
        public SettingSupport.TrendsRegionEnum TrendsRegion { get { return (SettingSupport.TrendsRegionEnum)GetValue(0); } set { SetValue((int)value); OnPropertyChanged(); } }
        public bool RetweetConfirmation { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool FavoriteConfirmation { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool ExtendTitleBar { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool ShowRetweetInMentionColumn { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); } }
        public bool RemoveRetweetAlreadyReceive { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); } }
        public bool BottomBarSearchBoxEnabled { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool PreventForcedTermination { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool EnableDatabase { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }

        // 投稿設定
        public bool CloseBottomAppBarAfterTweet { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); } }
        public bool ConvertPostingImage { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }


        // 表示設定
        public SettingSupport.ThemeEnum Theme { get { return (SettingSupport.ThemeEnum)GetValue(1); } set { SetValue((int)value); OnPropertyChanged(); ThemeService.Theme.ChangeTheme(); } }
        public double FontSize { get { return GetValue(12.0); } set { SetValue(value); OnPropertyChanged(); } }
        public double ColumnBackgroundBrushAlpha { get { return GetValue(255.0); } set { SetValue(value); OnPropertyChanged(); ThemeService.Theme.ChangeBackgroundAlpha(); } }
        public double TweetBackgroundBrushAlpha { get { return GetValue(10.0); } set { SetValue(value); OnPropertyChanged(); ThemeService.Theme.ChangeBackgroundAlpha(); } }
        public double MinColumnSize { get { return GetValue(336.0); } set { SetValue(value); OnPropertyChanged(); } }
        public int MaxColumnCount { get { return GetValue(2); } set { SetValue(value); OnPropertyChanged(); } }
        public double TweetAreaFontSize { get { return GetValue(14.5); } set { SetValue(value); OnPropertyChanged(); } }
        public double TweetCommandBarHeight { get { return GetValue(40.0); } set { SetValue(value); OnPropertyChanged(); } }
        public SettingSupport.SizeEnum IconSize { get { return (SettingSupport.SizeEnum)GetValue(45); } set { SetValue((int)value); OnPropertyChanged(); } }
        public SettingSupport.TweetAnimationEnum TweetAnimation { get { return (SettingSupport.TweetAnimationEnum)GetValue(3); } set { SetValue((int)value); OnPropertyChanged(); } }
        public bool DisableStreamingScroll { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); } }
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
            set { SetValue(value); OnPropertyChanged(); }
        }

        // 通知設定
        public bool FavoriteNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool UnfavoriteNotification { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); } }
        public bool FollowNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool RetweetNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool MentionNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool DirectMessageNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool QuotedTweetNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool SystemNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool AchievementNotification { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public bool NotificationSound { get { return GetValue(true); } set { SetValue(value); OnPropertyChanged(); } }
        public SettingSupport.TileNotificationEnum TileNotification { get { return (SettingSupport.TileNotificationEnum)GetValue(0); } set { SetValue((int)value); OnPropertyChanged(); } }
                
        // Mute設定
        public string MuteFilter { get { return GetValue("(False)"); } set { SetValue(value); OnPropertyChanged(); } }

        // 上級者向け設定
        public bool UseOfficialApi { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); } }
        public bool UseExtendedConversation { get { return GetValue(false); } set { SetValue(value); OnPropertyChanged(); } }
		
        [LocalValue]
        public string CustomFontName { get { return GetValue("Yu Gothic UI"); } set { if (!string.IsNullOrWhiteSpace(value)) SetValue(value); OnPropertyChanged(); } }

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
