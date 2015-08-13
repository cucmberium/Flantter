using Flantter.MilkyWay.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Storage;
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
        public bool Streaming { get; set; }
        public int Index { get; set; }

        public int FetchingNumberOfTweet { get; set; }
    }

    public class AdvancedSettingService : AdvancedSettingServiceBase<AdvancedSettingService>
    {
        private readonly List<Type> knownTypes = new List<Type> { typeof(ObservableCollection<AccountSetting>), typeof(ObservableCollection<string>), typeof(SettingSupport.DoubleTappedEventEnum), typeof(SettingSupport.TrendsPlaceEnum), typeof(SettingSupport.SizeEnum), typeof(SettingSupport.TileNotificationEnum), typeof(SettingSupport.TweetAnimationEnum) };
        public async Task SaveToStream(StorageFile s)
        {
            string json = JsonConvert.SerializeObject(Dict);
            await Windows.Storage.FileIO.WriteTextAsync(s, json);
        }
        public async Task LoadFromStream(StorageFile s)
        {
            string json = await Windows.Storage.FileIO.ReadTextAsync(s);
            Dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (Dict.ContainsKey("Account"))
                Dict["Account"] = JsonConvert.DeserializeObject<ObservableCollection<AccountSetting>>(JsonConvert.SerializeObject(Dict["Account"]));
            if (Dict.ContainsKey("MuteUsers"))
                Dict["MuteUsers"] = JsonConvert.DeserializeObject<ObservableCollection<string>>(JsonConvert.SerializeObject(Dict["MuteUsers"]));
            if (Dict.ContainsKey("MuteClients"))
                Dict["MuteClients"] = JsonConvert.DeserializeObject<ObservableCollection<string>>(JsonConvert.SerializeObject(Dict["MuteClients"]));
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
