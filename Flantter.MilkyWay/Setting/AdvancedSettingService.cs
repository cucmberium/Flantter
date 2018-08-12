using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Flantter.MilkyWay.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Setting
{
    public class AccountSetting : BindableBase
    {
        public ObservableCollection<ColumnSetting> Column { get; set; }

        #region Name変更通知プロパティ

        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        #endregion

        #region ScreenName変更通知プロパティ

        private string _screenName;

        public string ScreenName
        {
            get => _screenName;
            set => SetProperty(ref _screenName, value);
        }

        #endregion

        #region UserId変更通知プロパティ

        private long _userId;

        public long UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        #endregion

        #region ConsumerKey変更通知プロパティ

        private string _consumerKey;

        public string ConsumerKey
        {
            get => _consumerKey;
            set => SetProperty(ref _consumerKey, value);
        }

        #endregion

        #region ConsumerSecret変更通知プロパティ

        private string _consumerSecret;

        public string ConsumerSecret
        {
            get => _consumerSecret;
            set => SetProperty(ref _consumerSecret, value);
        }

        #endregion

        #region AccessToken変更通知プロパティ

        private string _accessToken;

        public string AccessToken
        {
            get => _accessToken;
            set => SetProperty(ref _accessToken, value);
        }

        #endregion

        #region AccessTokenSecret変更通知プロパティ

        private string _accessTokenSecret;

        public string AccessTokenSecret
        {
            get => _accessTokenSecret;
            set => SetProperty(ref _accessTokenSecret, value);
        }

        #endregion

        #region PossiblySensitive変更通知プロパティ

        private bool _possiblySensitive;

        public bool PossiblySensitive
        {
            get => _possiblySensitive;
            set => SetProperty(ref _possiblySensitive, value);
        }

        #endregion

        #region StatusPrivacy変更通知プロパティ

        private SettingSupport.StatusPrivacyEnum _statusPrivacy;

        public SettingSupport.StatusPrivacyEnum StatusPrivacy
        {
            get => _statusPrivacy;
            set => SetProperty(ref _statusPrivacy, value);
        }

        #endregion

        #region ProfileImageUrl変更通知プロパティ

        private string _profileImageUrl;

        public string ProfileImageUrl
        {
            get => _profileImageUrl;
            set => SetProperty(ref _profileImageUrl, value);
        }

        #endregion

        #region ProfileBannerUrl変更通知プロパティ

        private string _profileBannerUrl;

        public string ProfileBannerUrl
        {
            get => _profileBannerUrl;
            set => SetProperty(ref _profileBannerUrl, value);
        }

        #endregion

        #region IsEnabled変更通知プロパティ

        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        #endregion

        #region Platform変更通知プロパティ

        private SettingSupport.PlatformEnum _platform;

        public SettingSupport.PlatformEnum Platform
        {
            get => _platform;
            set => SetProperty(ref _platform, value);
        }

        #endregion

        #region Instance変更通知プロパティ

        private string _instance;

        public string Instance
        {
            get => _instance;
            set => SetProperty(ref _instance, value);
        }

        #endregion
    }

    public class ColumnSetting : BindableBase
    {
        #region Name変更通知プロパティ

        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        #endregion

        #region Action変更通知プロパティ

        private SettingSupport.ColumnTypeEnum _action;

        public SettingSupport.ColumnTypeEnum Action
        {
            get => _action;
            set => SetProperty(ref _action, value);
        }

        #endregion

        #region Parameter変更通知プロパティ

        private string _parameter;

        public string Parameter
        {
            get => _parameter;
            set => SetProperty(ref _parameter, value);
        }

        #endregion

        #region Filter変更通知プロパティ

        private string _filter;

        public string Filter
        {
            get => _filter;
            set => SetProperty(ref _filter, value);
        }

        #endregion

        #region DisableStartupRefresh変更通知プロパティ

        private bool _disableStartupRefresh;

        public bool DisableStartupRefresh
        {
            get => _disableStartupRefresh;
            set => SetProperty(ref _disableStartupRefresh, value);
        }

        #endregion

        #region AutoRefresh変更通知プロパティ

        private bool _autoRefresh;

        public bool AutoRefresh
        {
            get => _autoRefresh;
            set => SetProperty(ref _autoRefresh, value);
        }

        #endregion

        #region AutoRefreshTimerInterval変更通知プロパティ

        private double _autoRefreshTimerInterval;

        public double AutoRefreshTimerInterval
        {
            get => _autoRefreshTimerInterval;
            set => SetProperty(ref _autoRefreshTimerInterval, value);
        }

        #endregion

        #region Streaming変更通知プロパティ

        private bool _streaming;

        public bool Streaming
        {
            get => _streaming;
            set => SetProperty(ref _streaming, value);
        }

        #endregion

        #region Index変更通知プロパティ

        private int _index;

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        #endregion

        #region FetchingNumberOfTweet変更通知プロパティ

        private int _fetchingNumberOfTweet;

        public int FetchingNumberOfTweet
        {
            get => _fetchingNumberOfTweet;
            set => SetProperty(ref _fetchingNumberOfTweet, value);
        }

        #endregion

        #region Identifier変更通知プロパティ

        private long _identifier;

        public long Identifier
        {
            get => _identifier;
            set => SetProperty(ref _identifier, value);
        }

        #endregion
    }

    public class AdvancedSettingService : AdvancedSettingServiceBase<AdvancedSettingService>
    {
        private readonly AsyncLock _asyncLock = new AsyncLock();

        // アカウント,カラム設定
        public ObservableCollection<AccountSetting> Accounts
        {
            get => GetValue((ObservableCollection<AccountSetting>) null);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        // ミュート設定
        public ObservableCollection<string> MuteUsers
        {
            get => GetValue((ObservableCollection<string>) null);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> MuteClients
        {
            get => GetValue((ObservableCollection<string>) null);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> MuteWords
        {
            get => GetValue((ObservableCollection<string>) null);
            set
            {
                SetValue(value);
                OnPropertyChanged();
            }
        }

        public async Task SaveToAppSettings()
        {
            using (await _asyncLock.LockAsync())
            {
                try
                {
                    var json = JsonConvert.SerializeObject(Dict);
                    var writeStorageFile =
                        await ApplicationData.Current.RoamingFolder.CreateFileAsync("setting.xml",
                            CreationCollisionOption.ReplaceExisting);
                    using (var s = await writeStorageFile.OpenStreamForWriteAsync())
                    using (var st = new StreamWriter(s))
                    {
                        st.Write(json);
                    }
                }
                catch
                {
                }
            }
        }

        public async Task LoadFromAppSettings()
        {
            using (await _asyncLock.LockAsync())
            {
                try
                {
                    var readStorageFile = await ApplicationData.Current.RoamingFolder.GetFileAsync("setting.xml");
                    using (var s = await readStorageFile.OpenStreamForReadAsync())
                    using (var sr = new StreamReader(s))
                    using (var jtr = new JsonTextReader(sr))
                    {
                        var jTokens = JToken.ReadFrom(jtr);

                        Dict = new ConcurrentDictionary<string, object>();
                        foreach (var jToken in jTokens)
                        {
                            var jProperty = (JProperty)jToken;
                            if (jProperty.Name == "Accounts")
                                Dict[jProperty.Name] = ValidateAccountSetting(jProperty.Value.ToObject<ObservableCollection<AccountSetting>>());
                            else
                                Dict[jProperty.Name] = jProperty.Value.ToObject<ObservableCollection<string>>();
                        }
                    }
                }
                catch
                {
                    try
                    {
                        var readStorageFile = await ApplicationData.Current.RoamingFolder.GetFileAsync("setting.xml.bak");
                        using (var s = await readStorageFile.OpenStreamForReadAsync())
                        using (var sr = new StreamReader(s))
                        using (var jtr = new JsonTextReader(sr))
                        {
                            var jTokens = JToken.ReadFrom(jtr);

                            Dict = new ConcurrentDictionary<string, object>();
                            foreach (var jToken in jTokens)
                            {
                                var jProperty = (JProperty)jToken;
                                if (jProperty.Name == "Accounts")
                                    Dict[jProperty.Name] = ValidateAccountSetting(jProperty.Value.ToObject<ObservableCollection<AccountSetting>>());
                                else
                                    Dict[jProperty.Name] = jProperty.Value.ToObject<ObservableCollection<string>>();
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        
        public async Task BackupToAppSettings()
        {
            using (await _asyncLock.LockAsync())
            {
                try
                {
                    var json = JsonConvert.SerializeObject(Dict);
                    var writeStorageFile =
                        await ApplicationData.Current.RoamingFolder.CreateFileAsync("setting.xml.bak",
                            CreationCollisionOption.ReplaceExisting);
                    using (var s = await writeStorageFile.OpenStreamForWriteAsync())
                    using (var st = new StreamWriter(s))
                    {
                        st.Write(json);
                    }
                }
                catch
                {
                }
            }
        }

        private ObservableCollection<AccountSetting> ValidateAccountSetting(ObservableCollection<AccountSetting> accountSettings)
        {
            foreach (var accountSetting in accountSettings)
            {
                var validatedColumnSettings = new ObservableCollection<ColumnSetting>();
                foreach (var columnSetting in accountSetting.Column)
                {
                    if (!Enum.IsDefined(typeof(SettingSupport.ColumnTypeEnum), (int) columnSetting.Action))
                        continue;

                    validatedColumnSettings.Add(columnSetting);
                }

                foreach (var (columnSetting, index) in validatedColumnSettings.OrderBy(x => x.Index)
                    .Select((columnSetting, index) => (columnSetting, index)))
                {
                    columnSetting.Index = index;
                }

                accountSetting.Column = validatedColumnSettings;
            }

            return accountSettings;
        }
    }

    public class AdvancedSettingProvider
    {
        public AdvancedSettingService AdvancedSetting => AdvancedSettingService.AdvancedSetting;
    }
}