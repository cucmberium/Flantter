using CoreTweet;
using CoreTweet.Core;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Twitter;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Globalization;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class SearchSettingsFlyoutModel : BindableBase
    {
        public SearchSettingsFlyoutModel()
        {
            this.Statuses = new ObservableCollection<Twitter.Objects.Status>();
            this.Users = new ObservableCollection<Twitter.Objects.User>();

            this.Trends = new ObservableCollection<Twitter.Objects.Trend>();
            this.SavedSearches = new ObservableCollection<Twitter.Objects.SearchQuery>();
        }

        #region Tokens変更通知プロパティ
        private CoreTweet.Tokens _Tokens;
        public CoreTweet.Tokens Tokens
        {
            get { return this._Tokens; }
            set { this.SetProperty(ref this._Tokens, value); }
        }
        #endregion

        #region UserSearchWords変更通知プロパティ
        private string _UserSearchWords;
        public string UserSearchWords
        {
            get { return this._UserSearchWords; }
            set { this.SetProperty(ref this._UserSearchWords, value); }
        }
        #endregion

        #region UpdatingUserSearch変更通知プロパティ
        private bool _UpdatingUserSearch;
        public bool UpdatingUserSearch
        {
            get { return this._UpdatingUserSearch; }
            set { this.SetProperty(ref this._UpdatingUserSearch, value); }
        }
        #endregion

        #region StatusSearchWords変更通知プロパティ
        private string _StatusSearchWords;
        public string StatusSearchWords
        {
            get { return this._StatusSearchWords; }
            set { this.SetProperty(ref this._StatusSearchWords, value); }
        }
        #endregion

        #region UpdatingStatusSearch変更通知プロパティ
        private bool _UpdatingStatusSearch;
        public bool UpdatingStatusSearch
        {
            get { return this._UpdatingStatusSearch; }
            set { this.SetProperty(ref this._UpdatingStatusSearch, value); }
        }
        #endregion

        #region SavedSearchesScreenName変更通知プロパティ
        private string _SavedSearchesScreenName;
        public string SavedSearchesScreenName
        {
            get { return this._SavedSearchesScreenName; }
            set { this.SetProperty(ref this._SavedSearchesScreenName, value); }
        }
        #endregion

        #region TrendsPlace変更通知プロパティ
        private string _TrendsPlace;
        public string TrendsPlace
        {
            get { return this._TrendsPlace; }
            set { this.SetProperty(ref this._TrendsPlace, value); }
        }
        #endregion

        public ObservableCollection<Twitter.Objects.Status> Statuses { get; set; }

        public ObservableCollection<Twitter.Objects.User> Users { get; set; }

        public ObservableCollection<Twitter.Objects.Trend> Trends { get; set; }

        public ObservableCollection<Twitter.Objects.SearchQuery> SavedSearches { get; set; }

        public async Task UpdateStatuses(long maxid = 0, bool clear = true)
        {
            if (this.UpdatingStatusSearch)
                return;

            if (string.IsNullOrWhiteSpace(this._StatusSearchWords) || this.Tokens == null)
                return;

            this.UpdatingStatusSearch = true;

            if (maxid == 0 && clear)
                this.Statuses.Clear();

            IEnumerable<CoreTweet.Status> search = null;

            if (SettingService.Setting.UseOfficialApi && TwitterConnectionHelper.OfficialConsumerKeyList.Contains(this.Tokens.ConsumerKey))
            {
                var param = new Dictionary<string, object>() { { "q", this._StatusSearchWords }, { "count", 100 }, { "result_type", "recent" }, { "modules", "status" } };
                if (maxid != 0)
                    param["q"] = param["q"] + " max_id:" + maxid;

                try
                {
                    var res = await this.Tokens.SendRequestAsync(MethodType.Get, "https://api.twitter.com/1.1/search/universal.json", param);
                    var json = await res.Source.Content.ReadAsStringAsync();
                    var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(json);
                    var modules = jsonObject["modules"].Children<Newtonsoft.Json.Linq.JObject>();

                    var tweets = new List<CoreTweet.Status>();
                    foreach (var status in modules)
                    {
                        foreach (Newtonsoft.Json.Linq.JProperty prop in status.Properties())
                        {
                            if (prop.Name == "status")
                                tweets.Add(CoreBase.Convert<CoreTweet.Status>(Newtonsoft.Json.JsonConvert.SerializeObject(status["status"]["data"])));
                        }
                    }

                    search = tweets;
                }
                catch
                {
                    if (maxid == 0 && clear)
                        this.Statuses.Clear();

                    this.UpdatingStatusSearch = false;
                    return;
                }
            }
            else
            {
                var param = new Dictionary<string, object>() { { "count", 100 }, { "include_entities", true }, { "q", this._StatusSearchWords } };
                if (maxid != 0)
                    param.Add("max_id", maxid);

                try
                {
                    search = await this.Tokens.Search.TweetsAsync(param);
                }
                catch
                {
                    if (maxid == 0 && clear)
                        this.Statuses.Clear();

                    this.UpdatingStatusSearch = false;
                    return;
                }
            }
            
            if (maxid == 0 && clear)
                this.Statuses.Clear();

            foreach (var item in search)
            {
                var status = new Twitter.Objects.Status(item);

                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = this.Statuses.IndexOf(this.Statuses.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) == id));
                if (index == -1)
                {
                    index = this.Statuses.IndexOf(this.Statuses.FirstOrDefault(x => x is Twitter.Objects.Status && (((Twitter.Objects.Status)x).HasRetweetInformation ? ((Twitter.Objects.Status)x).RetweetInformation.Id : ((Twitter.Objects.Status)x).Id) < id));
                    if (index == -1)
                        this.Statuses.Add(status);
                    else
                        this.Statuses.Insert(index, status);
                }
            }

            // Todo : 受信したツイートをデータベースに登録

            this.UpdatingStatusSearch = false;
        }

        private long usersCursor = 0;
        public async Task UpdateUsers(bool useCursor = false)
        {
            if (this.UpdatingUserSearch)
                return;

            if (string.IsNullOrWhiteSpace(this._UserSearchWords) || this.Tokens == null)
                return;

            this.UpdatingUserSearch = true;

            if (!useCursor || usersCursor == 0)
                this.Users.Clear();

            ListedResponse<CoreTweet.User> following;
            try
            {
                if (useCursor && usersCursor != 0)
                    following = await Tokens.Users.SearchAsync(q => this._UserSearchWords, count => 20, page => usersCursor);
                else
                    following = await Tokens.Users.SearchAsync(q => this._UserSearchWords, count => 20);
            }
            catch
            {
                if (!useCursor || usersCursor == 0)
                    this.Users.Clear();

                this.UpdatingUserSearch = false;
                return;
            }

            if (!useCursor || usersCursor == 0)
                this.Users.Clear();

            foreach (var item in following)
            {
                var user = new Twitter.Objects.User(item);
                this.Users.Add(user);
            }

            if (useCursor)
                usersCursor += 1;
            else
                usersCursor = 2;

            this.UpdatingUserSearch = false;
        }

        public async Task CreateSavedSearches(string word)
        {
            try
            {
                await this.Tokens.SavedSearches.CreateAsync(query => word);
            }
            catch (TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                return;
            }

            Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_SaveSearchSuccessfully"));

            await this.UpdateSavedSearches(true);
        }

        public async Task DestroySavedSearches(long savedSearchId)
        {
            try
            {
                await this.Tokens.SavedSearches.DestroyAsync(id => savedSearchId);
            }
            catch (TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                return;
            }

            Notifications.Core.Instance.PopupToastNotification(Notifications.NotificationType.System, new ResourceLoader().GetString("Notification_System_DestroySearchSuccessfully"));

            await this.UpdateSavedSearches(true);
        }

        public bool UpdatingTrends { get; set; } = false;
        private long trendsLastWoeId = 0;
        private DateTime trendsLastUpdate = DateTime.Now - TimeSpan.FromDays(1.0);
        public async Task UpdateTrends(bool forceUpdate = false)
        {
            if (this.UpdatingTrends)
                return;

            if (!forceUpdate && trendsLastUpdate + TimeSpan.FromMinutes(15) > DateTime.Now && trendsLastWoeId == SettingSupport.GetTrendsWoeId(SettingService.Setting.TrendsRegion))
                return;

            this.UpdatingTrends = true;
            
            this.TrendsPlace = SettingSupport.GetTrendsPlaceString(SettingService.Setting.TrendsRegion);

            this.Trends.Clear();
            try
            {
                var trends = await this.Tokens.Trends.PlaceAsync(id => SettingSupport.GetTrendsWoeId(SettingService.Setting.TrendsRegion));

                if (trends.Count == 0)
                {
                    this.UpdatingTrends = false;
                    return;
                }

                this.Trends.Clear();
                foreach (var trend in trends.First().Trends)
                    this.Trends.Add(new Twitter.Objects.Trend(trend));

                trendsLastUpdate = trends.First().CreatedAt.DateTime.ToLocalTime();

                trendsLastWoeId = SettingSupport.GetTrendsWoeId(SettingService.Setting.TrendsRegion);
            }
            catch
            {
                this.UpdatingTrends = false;
                return;
            }

            this.UpdatingTrends = false;
        }

        public bool UpdatingSavedSearches { get; set; } = false;
        private DateTime savedSearchesLastUpdate = DateTime.Now - TimeSpan.FromDays(1.0);
        private string savedSearchesLastScreenName = "";
        public async Task UpdateSavedSearches(bool forceUpdate = false)
        {
            if (this.UpdatingSavedSearches)
                return;

            if (!forceUpdate && savedSearchesLastUpdate + TimeSpan.FromMinutes(15) > DateTime.Now && savedSearchesLastScreenName == this.Tokens.ScreenName)
                return;

            this.UpdatingSavedSearches = true;

            this.SavedSearchesScreenName = this.Tokens.ScreenName;
            
            this.SavedSearches.Clear();
            try
            {
                var savedSearches = await this.Tokens.SavedSearches.ListAsync();

                if (savedSearches.Count == 0)
                {
                    this.UpdatingSavedSearches = false;
                    return;
                }

                this.SavedSearches.Clear();
                foreach (var savedSearch in savedSearches)
                    this.SavedSearches.Add(new Twitter.Objects.SearchQuery(savedSearch));

                savedSearchesLastUpdate = DateTime.Now;
                savedSearchesLastScreenName = this.Tokens.ScreenName;
            }
            catch
            {
                this.UpdatingSavedSearches = false;
                return;
            }

            this.UpdatingSavedSearches = false;
        }
    }
}
