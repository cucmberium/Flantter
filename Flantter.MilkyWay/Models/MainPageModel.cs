using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

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
        public void Initialize()
        {
            Connecter.Instance.Initialize();

            foreach (var accountModel in this._Accounts)
                accountModel.Initialize();

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

        public void AddAccount(AccountSetting account)
        {
            var accountModel = new AccountModel(account);
            accountModel.Initialize();
            this._Accounts.Add(accountModel);

            AdvancedSettingService.AdvancedSetting.Accounts.Add(account);
            AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
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

            if (accountModel.IsEnabled)
            {
                accountModel.IsEnabled = false;
                this._Accounts.First().IsEnabled = true;
            }

            AdvancedSettingService.AdvancedSetting.Accounts.Remove(account);
            AdvancedSettingService.AdvancedSetting.Accounts.First(x => x.UserId == this._Accounts.First().UserId).IsEnabled = true;
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
