using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Flantter.MilkyWay.Models
{
    public class MainPageModel : BindableBase
    {
        #region Account
        private ObservableCollection<AccountModel> _Account;
        private ReadOnlyObservableCollection<AccountModel> _ReadOnlyAccount;
        public ReadOnlyObservableCollection<AccountModel> ReadOnlyAccount
        {
            get
            {
                return _ReadOnlyAccount;
            }
        }
        #endregion

        #region Initialize
        public async void Initialize()
        {
            this._Account.Add(new AccountModel());
        }
        #endregion

        #region Constructor
        private MainPageModel()
        {
            this._Account = new ObservableCollection<AccountModel>();
            this._ReadOnlyAccount = new ReadOnlyObservableCollection<AccountModel>(this._Account);
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
