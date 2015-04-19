using Flantter.MilkyWay.Setting;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text;

namespace Flantter.MilkyWay.Models
{
    public class ColumnModel : BindableBase
    {
        #region Action変更通知プロパティ
        private SettingSupport.ColumnTypeEnum _Action;
        public SettingSupport.ColumnTypeEnum Action
        {
            get { return this._Action; }
            set { this.SetProperty(ref this._Action, value); }
        }
        #endregion

        #region AutoRefresh変更通知プロパティ
        private bool _AutoRefresh;
        public bool AutoRefresh
        {
            get { return this._AutoRefresh; }
            set { this.SetProperty(ref this._AutoRefresh, value); }
        }
        #endregion

        #region AutoRefreshTimerInterval変更通知プロパティ
        private double _AutoRefreshTimerInterval;
        public double AutoRefreshTimerInterval
        {
            get { return this._AutoRefreshTimerInterval; }
            set { this.SetProperty(ref this._AutoRefreshTimerInterval, value); }
        }
        #endregion

        #region DisableStartupRefresh変更通知プロパティ
        private bool _DisableStartupRefresh;
        public bool DisableStartupRefresh
        {
            get { return this._DisableStartupRefresh; }
            set { this.SetProperty(ref this._DisableStartupRefresh, value); }
        }
        #endregion

        #region Filter変更通知プロパティ
        private string _Filter;
        public string Filter
        {
            get { return this._Filter; }
            set { this.SetProperty(ref this._Filter, value); }
        }
        #endregion

        #region Index変更通知プロパティ
        private int _Index;
        public int Index
        {
            get { return this._Index; }
            set { this.SetProperty(ref this._Index, value); }
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

        #region OwnerScreenName変更通知プロパティ
        private string _OwnerScreenName;
        public string OwnerScreenName
        {
            get { return this._OwnerScreenName; }
            set { this.SetProperty(ref this._OwnerScreenName, value); }
        }
        #endregion

        #region Parameter変更通知プロパティ
        private string _Parameter;
        public string Parameter
        {
            get { return this._Parameter; }
            set { this.SetProperty(ref this._Parameter, value); }
        }
		#endregion

		#region Streaming変更通知プロパティ
		private bool _Streaming;
        public bool Streaming
		{
            get { return this._Streaming; }
            set { this.SetProperty(ref this._Streaming, value); }
        }
        #endregion

        #region FetchingNumberOfTweet変更通知プロパティ
        private int _FetchingNumberOfTweet;
        public int FetchingNumberOfTweet
        {
            get { return this._FetchingNumberOfTweet; }
            set { this.SetProperty(ref this._FetchingNumberOfTweet, value); }
        }
        #endregion

        #region Columns
        private ObservableCollection<TweetModel> _Tweets;
        private ReadOnlyObservableCollection<TweetModel> _ReadOnlyTweets;
        public ReadOnlyObservableCollection<TweetModel> ReadOnlyTweets
        {
            get
            {
                return _ReadOnlyTweets;
            }
        }
        #endregion

        #region Constructor

        public ColumnModel(ColumnSetting column, string screenName)
        {
            this.Action = column.Action;
            this.AutoRefresh = column.AutoRefresh;
            this.AutoRefreshTimerInterval = column.AutoRefreshTimerInterval;
            this.DisableStartupRefresh = column.DisableStartupRefresh;
            this.Filter = column.Filter;
            this.Index = column.Index;
            this.Name = column.Name;
            this.Parameter = column.Parameter;
            this.Streaming = column.Streaming;
            this.FetchingNumberOfTweet = column.FetchingNumberOfTweet;
            this.OwnerScreenName = screenName;

            this._Tweets = new ObservableCollection<TweetModel>();
            this._ReadOnlyTweets = new ReadOnlyObservableCollection<TweetModel>(this._Tweets);
        }
        #endregion
    }
}
