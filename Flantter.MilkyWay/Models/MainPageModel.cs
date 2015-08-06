using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Setting;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public async void Initialize()
        {
            await Connection.Instance.Initialize();
            
            foreach (var account in AdvancedSettingService.AdvancedSetting.Account)
            {
                var message = new ValueSet();
                message.Add("Command", "AddAccountInfo");

                message.Add("Name", account.Name);
                message.Add("ScreenName", account.ScreenName);
                message.Add("UserId", account.UserId);
                message.Add("ConsumerKey", account.ConsumerKey);
                message.Add("ConsumerSecret", account.ConsumerSecret);
                message.Add("AccessToken", account.AccessToken);
                message.Add("AccessTokenSecret", account.AccessTokenSecret);
                message.Add("IncludeFollowingsActivity", account.IncludeFollowingsActivity);
                message.Add("PossiblySensitive", account.PossiblySensitive);

                await Connection.Instance.AppServiceConnection.SendMessageAsync(message);
            }

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
