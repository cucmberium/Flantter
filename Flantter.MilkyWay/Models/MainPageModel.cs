using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Notifications;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models
{
    public class MainPageModel : BindableBase
    {
        private bool _initialized;

        #region Constructor

        private MainPageModel()
        {
            _accounts = new ObservableCollection<AccountModel>();
            ReadOnlyAccounts = new ReadOnlyObservableCollection<AccountModel>(_accounts);

            foreach (var account in AdvancedSettingService.AdvancedSetting.Accounts)
                _accounts.Add(new AccountModel(account));
        }

        #endregion

        #region Initialize

        public async Task Initialize()
        {
            if (_initialized)
                return;

            Connecter.Instance.Initialize();

            await Task.WhenAll(_accounts.Select(x => x.Initialize()));

            Core.Instance.Initialize();

            if (SettingService.Setting.EnablePlugins)
                await Task.Run(() => Plugin.Core.Instance.Initialize());

            _initialized = true;

            await AdvancedSettingService.AdvancedSetting.BackupToAppSettings();
        }

        #endregion

        public void CopyTweetToClipBoard(object tweet)
        {
            if (tweet is Status status)
            {
                try
                {
                    var textPackage = new DataPackage();
                    textPackage.SetText(status.Text.ResolveEntity());
                    Clipboard.SetContent(textPackage);
                }
                catch
                {
                }

                return;
            }

            if (tweet is DirectMessage directMessage)
                try
                {
                    var textPackage = new DataPackage();
                    textPackage.SetText(directMessage.Text.ResolveEntity());
                    Clipboard.SetContent(textPackage);
                }
                catch
                {
                }
        }

        public void CopyTweetUrlToClipBoard(object tweet)
        {
            var status = tweet as Status;
            if (status == null)
                return;

            try
            {
                var textPackage = new DataPackage();
                textPackage.SetText(status.Url);
                Clipboard.SetContent(textPackage);
            }
            catch
            {
            }
        }

        public async Task ChangeBackgroundImage(StorageFile file)
        {
            SettingService.Setting.BackgroundImagePath = "";

            try
            {
                await file.CopyAsync(ApplicationData.Current.LocalFolder, "background_image" + file.FileType,
                    NameCollisionOption.ReplaceExisting);
                SettingService.Setting.BackgroundImagePath =
                    "ms-appdata:///local/" + "background_image" + file.FileType;
            }
            catch
            {
            }
        }

        public async Task ChangeTheme(StorageFile file)
        {
            try
            {
                await file.CopyAsync(ApplicationData.Current.LocalFolder, "Theme.xaml",
                    NameCollisionOption.ReplaceExisting);
                SettingService.Setting.CustomThemePath = "ms-appdata:///local/" + "Theme.xaml";
            }
            catch
            {
            }
        }

        public async Task MuteClient(string client)
        {
            if (AdvancedSettingService.AdvancedSetting.MuteClients == null)
                AdvancedSettingService.AdvancedSetting.MuteClients = new ObservableCollection<string>();

            if (!AdvancedSettingService.AdvancedSetting.MuteClients.Contains(client))
            {
                AdvancedSettingService.AdvancedSetting.MuteClients.Add(client);
                await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
            }
        }

        public async Task MuteWord(string word)
        {
            if (AdvancedSettingService.AdvancedSetting.MuteWords == null)
                AdvancedSettingService.AdvancedSetting.MuteWords = new ObservableCollection<string>();

            if (!AdvancedSettingService.AdvancedSetting.MuteWords.Contains(word))
            {
                AdvancedSettingService.AdvancedSetting.MuteWords.Add(word);
                await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
            }
        }

        public async Task DeleteMuteUser(string screenName)
        {
            if (AdvancedSettingService.AdvancedSetting.MuteUsers == null)
                AdvancedSettingService.AdvancedSetting.MuteUsers = new ObservableCollection<string>();

            if (AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(screenName))
            {
                AdvancedSettingService.AdvancedSetting.MuteUsers.Remove(screenName);
                await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
            }
        }

        public async Task DeleteMuteClient(string client)
        {
            if (AdvancedSettingService.AdvancedSetting.MuteClients == null)
                AdvancedSettingService.AdvancedSetting.MuteClients = new ObservableCollection<string>();

            if (AdvancedSettingService.AdvancedSetting.MuteClients.Contains(client))
            {
                AdvancedSettingService.AdvancedSetting.MuteClients.Remove(client);
                await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
            }
        }

        public async Task DeleteMuteWord(string word)
        {
            if (AdvancedSettingService.AdvancedSetting.MuteWords == null)
                AdvancedSettingService.AdvancedSetting.MuteWords = new ObservableCollection<string>();

            if (AdvancedSettingService.AdvancedSetting.MuteWords.Contains(word))
            {
                AdvancedSettingService.AdvancedSetting.MuteWords.Remove(word);
                await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
            }
        }

        public async Task AddAccount(AccountSetting account)
        {
            AdvancedSettingService.AdvancedSetting.Accounts.Add(account);
            await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();

            var accountModel = new AccountModel(account);
            _accounts.Add(accountModel);
            await accountModel.Initialize();
        }

        public async Task ChangeAccount(AccountSetting account)
        {
            foreach (var accountModel in _accounts)
            {
                accountModel.IsEnabled = account.UserId == accountModel.AccountSetting.UserId && account.Instance == accountModel.AccountSetting.Instance;

                accountModel.LeftSwipeMenuIsOpen = false;
            }

            foreach (var accountSetting in AdvancedSettingService.AdvancedSetting.Accounts)
                accountSetting.IsEnabled = account.UserId == accountSetting.UserId && account.Instance == accountSetting.Instance;

            await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
        }

        public async Task DeleteAccount(AccountSetting account)
        {
            var accountModel = _accounts.First(x => x.AccountSetting.UserId == account.UserId && x.AccountSetting.Instance == account.Instance);

            accountModel.LeftSwipeMenuIsOpen = false;

            accountModel.Dispose();
            _accounts.Remove(accountModel);

            AdvancedSettingService.AdvancedSetting.Accounts.Remove(account);

            if (accountModel.IsEnabled)
            {
                accountModel.IsEnabled = false;
                _accounts.First().IsEnabled = true;

                AdvancedSettingService.AdvancedSetting.Accounts
                    .First(x => x.UserId == _accounts.First().AccountSetting.UserId && x.Instance == _accounts.First().Instance)
                    .IsEnabled = true;
            }

            await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();

            Connecter.Instance.RemoveAccount(account);
        }

        #region Accounts

        private readonly ObservableCollection<AccountModel> _accounts;

        public ReadOnlyObservableCollection<AccountModel> ReadOnlyAccounts { get; }

        #endregion

        #region Instance

        public static MainPageModel Instance { get; } = new MainPageModel();

        #endregion
    }
}