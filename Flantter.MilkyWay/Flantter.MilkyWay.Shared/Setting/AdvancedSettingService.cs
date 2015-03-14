using Flantter.MilkyWay.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Notifications;

namespace Flantter.MilkyWay.Setting
{
    public class AccountSetting
    {
        public string Name { get; set; }
        public string ScreenName { get; set; }
        public long UserId { get; set; }

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }

        public ObservableCollection<ColumnSetting> Column { get; set; }

        public bool IncludeFollowingsActivity { get; set; }
        public bool PossiblySensitive { get; set; }

        public string ProfileImageUrl { get; set; }
        public string ProfileBannerUrl { get; set; }

        public bool IsEnabled { get; set; }
    }

    public class ColumnSetting
    {
        public string Name { get; set; }
        public SettingSupport.ColumnTypeEnum Action { get; set; }
        public string Parameter { get; set; }
        public string Filter { get; set; }

        public bool DisableStartupRefresh { get; set; }
        public bool AutoRefresh { get; set; }
        public double AutoRefreshTimerInterval { get; set; }
        public bool Stream { get; set; }
        public int Index { get; set; }

        public int FetchingNumberOfTweet { get; set; }
    }

    public class AdvancedSettingService : AdvancedSettingServiceBase<AdvancedSettingService>
    {
        private readonly List<Type> knownTypes = new List<Type> { typeof(ObservableCollection<AccountSetting>), typeof(ObservableCollection<string>), typeof(SettingSupport.DoubleTappedEventEnum), typeof(SettingSupport.TrendsPlaceEnum), typeof(SettingSupport.SizeEnum), typeof(SettingSupport.TileNotificationEnum), typeof(SettingSupport.TweetAnimationEnum) };
        public void SaveToStream(Stream s)
        {
            var serializer = new DataContractSerializer(typeof(Dictionary<string, object>), knownTypes);
            serializer.WriteObject(s, Dict);
        }
        public void LoadFromStream(Stream s)
        {
            var serializer = new DataContractSerializer(typeof(Dictionary<string, object>), knownTypes);
            Dict = serializer.ReadObject(s) as Dictionary<string, object>;
        }
        public void LoadFromString(string s)
        {
            var serializer = new DataContractSerializer(typeof(Dictionary<string, object>), knownTypes);
            using (var sr = new StringReader(s))
            using (var xr = XmlReader.Create(sr))
            {
                Dict = serializer.ReadObject(xr) as Dictionary<string, object>;
            }
        }

        // アカウント,カラム設定
        public ObservableCollection<AccountSetting> Account { get { return GetValue((ObservableCollection<AccountSetting>)null); } set { SetValue(value); OnPropertyChanged(); } }
        
        // ミュート設定
        public ObservableCollection<string> MuteUsers { get { return GetValue((ObservableCollection<string>)null); } set { SetValue(value); OnPropertyChanged(); } }
        public ObservableCollection<string> MuteClients { get { return GetValue((ObservableCollection<string>)null); } set { SetValue(value); OnPropertyChanged(); } }
    }

    public class AdvancedSettingProvider
    {
        public AdvancedSettingService AdvancedSetting { get { return AdvancedSettingService.AdvancedSetting; } }
    }
}
