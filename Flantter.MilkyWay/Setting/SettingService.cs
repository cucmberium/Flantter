using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Services.Database;
using Flantter.MilkyWay.Themes;
using SharpDX.DirectWrite;

namespace Flantter.MilkyWay.Setting
{
    public static class SettingSupport
    {
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
            Collection = 9,
            Federated = 10,
            Local = 11
        }

        public enum DoubleTappedActionEnum
        {
            None = 0,
            StatusDetail = 1,
            UserProfile = 2,
            Favorite = 3,
            Reply = 4,
            Retweet = 5
        }

        public enum PlatformEnum
        {
            Twitter = 0,
            Mastodon = 1
        }

        public enum SizeEnum
        {
            ExtraSmall = 40,
            Small = 45,
            Medium = 50,
            Large = 55
        }

        public enum ThemeEnum
        {
            Light = 0,
            Dark = 1
        }

        public enum TileNotificationEnum
        {
            None = 0,
            Mentions = 1,
            Images = 2
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
            Singapore = 7
        }

        public enum TweetAnimationEnum
        {
            None = 0,
            Expand = 1,
            Slide = 2,
            ScrollToTop = 3
        }

        public enum StatusPrivacyEnum
        {
            Public = 0,
            Unlisted = 1,
            Private = 2,
            Direct = 3
        }

        public static readonly List<string> PictureSavePath =
            new List<string> {"Picture/", "Picture/Flantter/", "Manual"};

        public static readonly List<string> FontWeight = new List<string>
        {
            "Black",
            "Bold",
            "ExtraBlack",
            "ExtraBold",
            "ExtraLight",
            "Light",
            "Medium",
            "Normal",
            "SemiBold",
            "SemiLight",
            "Thin"
        };

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
                case TrendsRegionEnum.Singapore:
                    return 23424948;
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
                        default:
                            return "Global";
                    }
                default:
                    return place.ToString();
            }
        }

        public static List<string> GetSystemFontFamilies()
        {
            try
            {
                var fontlist = new List<string>();

                using (var factory = new Factory())
                using (var fontCollection = factory.GetSystemFontCollection(false))
                {
                    var familyCount = fontCollection.FontFamilyCount;
                    for (var i = 0; i < familyCount; i++)
                        using (var fontFamily = fontCollection.GetFontFamily(i))
                        using (var familyNames = fontFamily.FamilyNames)
                        {
                            familyNames.FindLocaleName("en-us", out int index);
                            fontlist.Add(familyNames.GetString(index));
                        }
                }

                fontlist.Sort(StringComparer.OrdinalIgnoreCase);
                return fontlist;
            }
            catch
            {
            }

            return new List<string>();
        }
    }

    public class SettingSupportProvider
    {
        public static IEnumerable<SettingSupport.DoubleTappedActionEnum> DoubleTappedEventListTypeValues => Enum
            .GetValues(typeof(SettingSupport.DoubleTappedActionEnum))
            .Cast<SettingSupport.DoubleTappedActionEnum>();

        public static IEnumerable<SettingSupport.TrendsRegionEnum> TrendsRegionListTypeValues => Enum
            .GetValues(typeof(SettingSupport.TrendsRegionEnum))
            .Cast<SettingSupport.TrendsRegionEnum>();

        public static IEnumerable<SettingSupport.SizeEnum> SizeListTypeValues => Enum
            .GetValues(typeof(SettingSupport.SizeEnum))
            .Cast<SettingSupport.SizeEnum>();

        public static IEnumerable<SettingSupport.TileNotificationEnum> TileNotificationListTypeValues => Enum
            .GetValues(typeof(SettingSupport.TileNotificationEnum))
            .Cast<SettingSupport.TileNotificationEnum>();

        public static IEnumerable<SettingSupport.TweetAnimationEnum> TweetAnimationListTypeValues => Enum
            .GetValues(typeof(SettingSupport.TweetAnimationEnum))
            .Cast<SettingSupport.TweetAnimationEnum>();

        public static IEnumerable<SettingSupport.ThemeEnum> ThemeListTypeValues => Enum
            .GetValues(typeof(SettingSupport.ThemeEnum))
            .Cast<SettingSupport.ThemeEnum>();

        public static IEnumerable<SettingSupport.StatusPrivacyEnum> StatusPrivacyListTypeValues => Enum
            .GetValues(typeof(SettingSupport.StatusPrivacyEnum))
            .Cast<SettingSupport.StatusPrivacyEnum>();

        public static IEnumerable<string> SystemFontFamilies => SettingSupport.GetSystemFontFamilies();

        public static IEnumerable<string> PictureSavePathList => SettingSupport.PictureSavePath;

        public static IEnumerable<string> FontWeightList => SettingSupport.FontWeight;
    }

    public class SettingService : SettingServiceBase<SettingService>
    {
        // 動作設定
        public SettingSupport.DoubleTappedActionEnum DoubleTappedAction
        {
            get => (SettingSupport.DoubleTappedActionEnum) GetValue(0);
            set
            {
                SetValue((int) value);
                OnPropertyChanged();
            }
        }

        public SettingSupport.TrendsRegionEnum TrendsRegion
        {
            get => (SettingSupport.TrendsRegionEnum) GetValue(0);
            set
            {
                SetValue((int) value);
                OnPropertyChanged();
            }
        }

        public bool RetweetConfirmation
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool FavoriteConfirmation
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool NotificateRetweetedRetweet
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool ExtendTitleBar
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool ShowRetweetInMentionColumn
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool RemoveRetweetAlreadyReceive
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool RemoveRetweetOfMyTweet
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool BottomBarSearchBoxEnabled
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool PreventForcedTermination
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public int PictureSavePath
        {
            get => GetValue(0);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool DisableStartupTimelineUpdate
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool EnableTweetTextSelection
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool EnableCreateAtLink
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool ExtendedExecution
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        // 投稿設定
        public bool ShowAppBarToTop
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool CloseAppBarAfterTweet
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool ResetPostingAccountBeforeTweetAreaOpening
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool RefreshTimelineAfterTweet
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool ConvertPostingImage
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool ScalePostingImage
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        // 表示設定
        [LocalValue]
        public SettingSupport.ThemeEnum Theme
        {
            get => (SettingSupport.ThemeEnum) GetValue(1);
            set
            {
                SetValue((int) value);
                OnPropertyChanged();
                ThemeService.Theme.ChangeTheme();
            }
        }

        [LocalValue]
        public double FontSize
        {
            get => GetValue(12.0);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public double ColumnBackgroundBrushAlpha
        {
            get => GetValue(255.0);
            set
            {
                SetValue(value);
                OnPropertyChanged();
                ThemeService.Theme.ChangeBackgroundAlpha();
            }
        }

        [LocalValue]
        public double TweetBackgroundBrushAlpha
        {
            get => GetValue(10.0);
            set
            {
                SetValue(value);
                OnPropertyChanged();
                ThemeService.Theme.ChangeBackgroundAlpha();
            }
        }

        [LocalValue]
        public double MinColumnSize
        {
            get => GetValue(320.0);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public int MaxColumnCount
        {
            get => GetValue(2);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public double TweetAreaFontSize
        {
            get => GetValue(14.5);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public double TweetCommandBarHeight
        {
            get => GetValue(40.0);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public SettingSupport.SizeEnum IconSize
        {
            get => (SettingSupport.SizeEnum) GetValue(45);
            set
            {
                SetValue((int) value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public SettingSupport.TweetAnimationEnum TweetAnimation
        {
            get => (SettingSupport.TweetAnimationEnum) GetValue(3);
            set
            {
                SetValue((int) value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public bool DisableStreamingScroll
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public double TweetMediaThumbnailHeight
        {
            get => GetValue(100.0);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public double TweetMediaThumbnailWidth
        {
            get => GetValue(160.0);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public bool ShowHighQualityImageResolution
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public bool ShowGifProfileImage
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public bool ShowQuotedStatusMedia
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public bool UseTransparentBackground
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public bool EnableNsfwFilter
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public bool UseBackgroundImage
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
                OnPropertyChanged("BackgroundImagePath");
            }
        }

        [LocalValue]
        public string BackgroundImagePath
        {
            get
            {
                if (!UseBackgroundImage)
                    return "http://localhost/";
                var str = GetValue("http://localhost/");
                return string.IsNullOrWhiteSpace(str) ? "http://localhost/" : str;
            }
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        // 通知設定
        public bool FavoriteNotification
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool FollowNotification
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool RetweetNotification
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool MentionNotification
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool SystemNotification
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool TweetCompleteNotification
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool AchievementNotification
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool NotificationSound
        {
            get => GetValue(true);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public SettingSupport.TileNotificationEnum TileNotification
        {
            get => (SettingSupport.TileNotificationEnum) GetValue(0);
            set
            {
                SetValue((int) value);
                OnPropertyChanged();
            }
        }

        // Mute設定
        public string MuteFilter
        {
            get => GetValue("(False)");
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        // データベース設定
        [LocalValue]
        public bool EnableDatabase
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
                if (value) Database.Instance.Initialize();
                else Database.Instance.Free();
            }
        }

        [LocalValue]
        public int MaximumHoldingNumberOfTweet
        {
            get => GetValue(10000);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public bool RestoreTimelineOnStartup
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public bool StopStreamingOnStartup
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        // プラグイン設定
        [LocalValue]
        public bool EnablePlugins
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        // 上級者向け設定
        public bool UseOfficialApi
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public bool UseExtendedConversation
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public string CustomFontName
        {
            get => GetValue("Yu Gothic UI");
            set
            {
                if (!string.IsNullOrWhiteSpace(value)) SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public string FontWeight
        {
            get => GetValue("Normal");
            set
            {
                if (!string.IsNullOrWhiteSpace(value)) SetValue(value);
                OnPropertyChanged();
            }
        }

        [LocalValue]
        public bool UseCustomTheme
        {
            get => GetValue(false);
            set
            {
                SetValue(value);
                OnPropertyChanged();
                ThemeService.Theme.ChangeTheme();
            }
        }

        [LocalValue]
        public string CustomThemePath
        {
            get => GetValue(string.Empty);
            set
            {
                SetValue(value);
                OnPropertyChanged();
                ThemeService.Theme.ChangeTheme();
            }
        }

        // Other
        public string UserUuid
        {
            get => GetValue(string.Empty);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }
    }

    public class SettingProvider
    {
        public SettingService Setting => SettingService.Setting;
    }
}