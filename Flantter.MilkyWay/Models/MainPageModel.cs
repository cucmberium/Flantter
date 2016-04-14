using Flantter.MilkyWay.Models.Exceptions;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Flantter.MilkyWay.Models
{
    public class MainPageModel : BindableBase
    {
        #region Accounts
        private ObservableCollection<AccountModel> _Accounts;
        private ReadOnlyObservableCollection<AccountModel> _ReadOnlyAccounts;
        public ReadOnlyObservableCollection<AccountModel> ReadOnlyAccounts
        {
            get
            {
                return _ReadOnlyAccounts;
            }
        }
        #endregion

        #region Initialize
        public async Task Initialize()
        {
            Connecter.Instance.Initialize();

            await Task.WhenAll(this._Accounts.Select(x => x.Initialize()));

            Notifications.Core.Instance.Initialize();
        }
        #endregion

        #region Constructor
        private MainPageModel()
        {
            this._Accounts = new ObservableCollection<AccountModel>();
            this._ReadOnlyAccounts = new ReadOnlyObservableCollection<AccountModel>(this._Accounts);
            
            foreach (var account in AdvancedSettingService.AdvancedSetting.Accounts)
            {
                this._Accounts.Add(new AccountModel(account));
            }
        }
        #endregion

        #region Destructor
        ~MainPageModel()
        {
        }
        #endregion


        public void CopyTweetToClipBoard(object tweet)
        {
            var status = tweet as Status;
            if (status != null)
            {
                try
                {
                    var textPackage = new DataPackage();
                    textPackage.SetText(status.Text);
                    Clipboard.SetContent(textPackage);
                }
                catch
                {
                }

                return;
            }

            var directMessage = tweet as DirectMessage;
            if (directMessage != null)
            {
                try
                {
                    var textPackage = new DataPackage();
                    textPackage.SetText(directMessage.Text);
                    Clipboard.SetContent(textPackage);
                }
                catch
                {
                }

                return;
            }
        }

        public async Task ChangeBackgroundImage(StorageFile file)
        {
            SettingService.Setting.BackgroundImagePath = "";

            try
            {
                await file.CopyAsync(ApplicationData.Current.LocalFolder, "background_image" + file.FileType, NameCollisionOption.ReplaceExisting);
                SettingService.Setting.BackgroundImagePath = "ms-appdata:///local/" + "background_image" + file.FileType;
            }
            catch
            {
            }
        }

        public async Task ChangeTheme(StorageFile file)
        {
            try
            {
                await file.CopyAsync(ApplicationData.Current.LocalFolder, "Theme.xaml", NameCollisionOption.ReplaceExisting);
                SettingService.Setting.CustomThemePath = "ms-appdata:///local/" + "Theme.xaml";
            }
            catch
            {
            }
        }

        public void MuteClient(string client)
        {
            if (!AdvancedSettingService.AdvancedSetting.MuteClients.Contains(client))
            {
                AdvancedSettingService.AdvancedSetting.MuteClients.Add(client);
                AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
            }
        }

        public void DeleteMuteUser(string screenName)
        {
            if (AdvancedSettingService.AdvancedSetting.MuteUsers.Contains(screenName))
            {
                AdvancedSettingService.AdvancedSetting.MuteUsers.Remove(screenName);
                AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
            }
        }

        public void DeleteMuteClient(string client)
        {
            if (AdvancedSettingService.AdvancedSetting.MuteClients.Contains(client))
            {
                AdvancedSettingService.AdvancedSetting.MuteClients.Remove(client);
                AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
            }
        }

        public async void AddAccount(AccountSetting account)
        {
            AdvancedSettingService.AdvancedSetting.Accounts.Add(account);
            AdvancedSettingService.AdvancedSetting.SaveToAppSettings();

            var accountModel = new AccountModel(account);
            this._Accounts.Add(accountModel);
            await accountModel.Initialize();
        }

        public void ChangeAccount(AccountSetting account)
        {
            foreach (var accountModel in this._Accounts)
            {
                if (account.UserId == accountModel.UserId)
                    accountModel.IsEnabled = true;
                else
                    accountModel.IsEnabled = false;

                accountModel.LeftSwipeMenuIsOpen = false;
            }

            foreach (var accountSetting in AdvancedSettingService.AdvancedSetting.Accounts)
            {
                if (account.UserId == accountSetting.UserId)
                    accountSetting.IsEnabled = true;
                else
                    accountSetting.IsEnabled = false;
            }

            AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
        }

        public void DeleteAccount(AccountSetting account)
        {
            var accountModel = this._Accounts.First(x => x.UserId == account.UserId);

            accountModel.LeftSwipeMenuIsOpen = false;

            accountModel.Dispose();
            this._Accounts.Remove(accountModel);
            
            AdvancedSettingService.AdvancedSetting.Accounts.Remove(account);

            if (accountModel.IsEnabled)
            {
                accountModel.IsEnabled = false;
                this._Accounts.First().IsEnabled = true;

                AdvancedSettingService.AdvancedSetting.Accounts.First(x => x.UserId == this._Accounts.First().UserId).IsEnabled = true;
            }

            AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
        }
        
        #region Instance
        private static MainPageModel _Instance = new MainPageModel();
        public static MainPageModel Instance
        {
            get { return _Instance; }
        }
        #endregion
    }
}
