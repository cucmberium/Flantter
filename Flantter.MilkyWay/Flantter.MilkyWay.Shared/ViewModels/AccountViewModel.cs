using Flantter.MilkyWay.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flantter.MilkyWay.ViewModels
{
    public class AccountViewModel
    {
        private AccountModel _AccountModel { get; set; }

        public ReactiveProperty<string> ProfileImageUrl { get; private set; }
        public ReactiveProperty<string> ProfileBannerUrl { get; private set; }

        #region Constructor
        public AccountViewModel()
        {
        }

        public AccountViewModel(AccountModel account)
        {
            this._AccountModel = account;

            this.ProfileImageUrl = account.ObserveProperty(x => x.ProfileImageUrl).ToReactiveProperty();
            this.ProfileBannerUrl = account.ObserveProperty(x => x.ProfileBannerUrl).ToReactiveProperty();
        }
        #endregion

        #region Destructor
        ~AccountViewModel()
        {
        }
        #endregion
    }
}
