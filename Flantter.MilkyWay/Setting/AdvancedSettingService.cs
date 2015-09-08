﻿using Flantter.MilkyWay.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public void SaveToAppSettings()
        {
            string json = JsonConvert.SerializeObject(Dict);
            SettingService.Setting.AdvancedSettingData = json;
        }
        public void LoadFromAppSettings()
        {
            string json = SettingService.Setting.AdvancedSettingData;

            var jTokens = JToken.Parse(json);

            this.Dict = new Dictionary<string, object>();
            foreach (JProperty jProperty in jTokens)
            {
                if (jProperty.Name == "Account")
                    this.Dict[jProperty.Name] = jProperty.Value.ToObject<ObservableCollection<AccountSetting>>();
                else
                    this.Dict[jProperty.Name] = jProperty.Value.ToObject<ObservableCollection<string>>();
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
