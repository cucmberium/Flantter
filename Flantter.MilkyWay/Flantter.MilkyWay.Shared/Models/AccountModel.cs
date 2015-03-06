using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flantter.MilkyWay.Models
{
    public class AccountModel : BindableBase
    {
        #region ProfileImageUrl変更通知プロパティ
        private string _ProfileImageUrl;
        public string ProfileImageUrl
        {
            get { return this._ProfileImageUrl; }
            private set { this.SetProperty(ref this._ProfileImageUrl, value); }
        }
        #endregion

        #region ProfileBannerUrl変更通知プロパティ
        private string _ProfileBannerUrl;
        public string ProfileBannerUrl
        {
            get { return this._ProfileBannerUrl; }
            private set { this.SetProperty(ref this._ProfileBannerUrl, value); }
        }
        #endregion

        #region Constructor
        public AccountModel()
        {
            this.ProfileImageUrl = "https://pbs.twimg.com/profile_images/3077279905/11e31fda9b6648ea0a362820ed4d7d0f.png";
        }
        #endregion
    }
}
