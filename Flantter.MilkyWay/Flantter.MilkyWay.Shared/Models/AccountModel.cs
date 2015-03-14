using Flantter.MilkyWay.Setting;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Flantter.MilkyWay.Models
{
    public class AccountModel : BindableBase
    {
        #region IsEnabled変更通知プロパティ
        private bool _IsEnabled;
        public bool IsEnabled
        {
            get { return this._IsEnabled; }
            set { this.SetProperty(ref this._IsEnabled, value); }
        }
        #endregion

        #region ProfileImageUrl変更通知プロパティ
        private string _ProfileImageUrl;
        public string ProfileImageUrl
        {
            get { return this._ProfileImageUrl; }
            set { this.SetProperty(ref this._ProfileImageUrl, value); }
        }
        #endregion

        #region ProfileBannerUrl変更通知プロパティ
        private string _ProfileBannerUrl;
        public string ProfileBannerUrl
        {
            get { return this._ProfileBannerUrl; }
            set { this.SetProperty(ref this._ProfileBannerUrl, value); }
        }
        #endregion

        #region Name変更通知プロパティ
        private string _Name;
        public string Name
        {
            get { return this._Name; }
            set { this.SetProperty(ref this._Name, value); }
        }
        #endregion

        #region ScreenName変更通知プロパティ
        private string _ScreenName;
        public string ScreenName
        {
            get { return this._ScreenName; }
            set { this.SetProperty(ref this._ScreenName, value); }
        }
        #endregion

        #region UserId変更通知プロパティ
        private long _UserId;
        public long UserId
        {
            get { return this._UserId; }
            set { this.SetProperty(ref this._UserId, value); }
        }
        #endregion

        #region IncludeFollowingsActivity変更通知プロパティ
        private bool _IncludeFollowingsActivity;
        public bool IncludeFollowingsActivity
        {
            get { return this._IncludeFollowingsActivity; }
            set { this.SetProperty(ref this._IncludeFollowingsActivity, value); }
        }
        #endregion

        #region PossiblySensitive変更通知プロパティ
        private bool _PossiblySensitive;
        public bool PossiblySensitive
        {
            get { return this._PossiblySensitive; }
            set { this.SetProperty(ref this._PossiblySensitive, value); }
        }
        #endregion

        #region Columns
        private ObservableCollection<ColumnModel> _Columns;
        private ReadOnlyObservableCollection<ColumnModel> _ReadOnlyColumns;
        public ReadOnlyObservableCollection<ColumnModel> ReadOnlyColumns
        {
            get
            {
                return _ReadOnlyColumns;
            }
        }
        #endregion

        #region Constructor
        public AccountModel()
        {
            this._Columns = new ObservableCollection<ColumnModel>();
            this._ReadOnlyColumns = new ReadOnlyObservableCollection<ColumnModel>(this._Columns);

            this.ProfileImageUrl = "https://pbs.twimg.com/profile_images/3077279905/11e31fda9b6648ea0a362820ed4d7d0f.png";
            this.IsEnabled = true;
        }

        public AccountModel(AccountSetting account)
        {
            this._Columns = new ObservableCollection<ColumnModel>();
            this._ReadOnlyColumns = new ReadOnlyObservableCollection<ColumnModel>(this._Columns);

            this.IsEnabled = account.IsEnabled;
            this.IncludeFollowingsActivity = account.IncludeFollowingsActivity;
            this.Name = account.Name;
            this.PossiblySensitive = account.PossiblySensitive;
            this.ProfileBannerUrl = account.ProfileBannerUrl;
            this.ProfileImageUrl = account.ProfileImageUrl;
            this.ScreenName = account.ScreenName;
            this.UserId = account.UserId;

            foreach (var column in account.Column)
                this._Columns.Add(new ColumnModel(column, account.ScreenName));
        }
        #endregion
    }
}
