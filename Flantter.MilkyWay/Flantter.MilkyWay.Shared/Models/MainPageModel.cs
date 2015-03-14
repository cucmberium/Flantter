using Flantter.MilkyWay.Setting;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

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
            foreach (var account in AdvancedSettingService.AdvancedSetting.Account)
            {
                this._Accounts.Add(new AccountModel(account));
            }
        }
        #endregion

        #region Constructor
        private MainPageModel()
        {
            this._Accounts = new ObservableCollection<AccountModel>();
            this._ReadOnlyAccounts = new ReadOnlyObservableCollection<AccountModel>(this._Accounts);
        }
        #endregion

        #region Destructor
        ~MainPageModel()
        {
        }
        #endregion

        #region Instance
        private static MainPageModel _Instance = new MainPageModel();
        public static MainPageModel Instance
        {
            get { return _Instance; }
        }
        #endregion
    }
}
