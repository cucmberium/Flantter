using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Notifications;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Twitter;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.Setting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class SearchSettingsFlyoutModel : BindableBase
    {
        private string _savedSearchesLastScreenName = "";
        private DateTime _savedSearchesLastUpdate = DateTime.Now - TimeSpan.FromDays(1.0);
        private DateTime _trendsLastUpdate = DateTime.Now - TimeSpan.FromDays(1.0);
        private long _trendsLastWoeId;

        private long _usersCursor;

        public SearchSettingsFlyoutModel()
        {
            Statuses = new ObservableCollection<Status>();
            Users = new ObservableCollection<User>();

            Trends = new ObservableCollection<Trend>();
            SavedSearches = new ObservableCollection<SearchQuery>();
        }

        public ObservableCollection<Status> Statuses { get; set; }

        public ObservableCollection<User> Users { get; set; }

        public ObservableCollection<Trend> Trends { get; set; }

        public ObservableCollection<SearchQuery> SavedSearches { get; set; }

        public bool UpdatingTrends { get; set; }

        public bool UpdatingSavedSearches { get; set; }

        public async Task UpdateStatuses(long maxid = 0, bool clear = true)
        {
            if (UpdatingStatusSearch)
                return;

            if (string.IsNullOrWhiteSpace(_statusSearchWords) || Tokens == null)
                return;

            UpdatingStatusSearch = true;

            if (maxid == 0 && clear)
                Statuses.Clear();

            IEnumerable<Status> search;

            if (SettingService.Setting.UseOfficialApi &&
                TwitterConnectionHelper.OfficialConsumerKeyList.Contains(Tokens.ConsumerKey))
            {
                var param = new Dictionary<string, object>
                {
                    {"q", _statusSearchWords},
                    {"count", 100},
                    {"result_type", "recent"},
                    {"modules", "status"},
                    {"tweet_mode", CoreTweet.TweetMode.extended}
                };
                if (maxid != 0)
                    param["q"] = param["q"] + " max_id:" + maxid;

                try
                {
                    var res = await Tokens.TwitterTokens.SendRequestAsync(CoreTweet.MethodType.Get,
                        "https://api.twitter.com/1.1/search/universal.json", param);
                    var json = await res.Source.Content.ReadAsStringAsync();
                    var jsonObject = JObject.Parse(json);
                    var modules = jsonObject["modules"].Children<JObject>();

                    var tweets = new List<CoreTweet.Status>();
                    foreach (var status in modules)
                    foreach (var prop in status.Properties())
                        if (prop.Name == "status")
                            tweets.Add(
                                CoreTweet.Core.CoreBase.Convert<CoreTweet.Status>(
                                    JsonConvert.SerializeObject(status["status"]["data"])));

                    search = tweets.Select(x => new Status(x));
                }
                catch
                {
                    if (maxid == 0 && clear)
                        Statuses.Clear();

                    UpdatingStatusSearch = false;
                    return;
                }
            }
            else
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 100},
                    {"include_entities", true},
                    {"q", _statusSearchWords},
                    {"tweet_mode", CoreTweet.TweetMode.extended}
                };
                if (maxid != 0)
                    param.Add("max_id", maxid);

                try
                {
                    search = await Tokens.Search.TweetsAsync(param);
                }
                catch
                {
                    if (maxid == 0 && clear)
                        Statuses.Clear();

                    UpdatingStatusSearch = false;
                    return;
                }
            }

            if (maxid == 0 && clear)
                Statuses.Clear();

            foreach (var status in search)
            {
                Connecter.Instance.TweetReceive_OnCommandExecute(this,
                    new TweetEventArgs(status, Tokens.UserId, new List<string> {"none://"}, false));

                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                var index = Statuses.IndexOf(
                    Statuses.FirstOrDefault(x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) == id));
                if (index == -1)
                {
                    index = Statuses.IndexOf(
                        Statuses.FirstOrDefault(x => (x.HasRetweetInformation ? x.RetweetInformation.Id : x.Id) < id));
                    if (index == -1)
                        Statuses.Add(status);
                    else
                        Statuses.Insert(index, status);
                }
            }

            UpdatingStatusSearch = false;
        }

        public async Task UpdateUsers(bool useCursor = false)
        {
            if (UpdatingUserSearch)
                return;

            if (string.IsNullOrWhiteSpace(_userSearchWords) || Tokens == null)
                return;

            UpdatingUserSearch = true;

            if (!useCursor || _usersCursor == 0)
                Users.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"count", 100},
                    {"include_entities", true},
                    {"q", _statusSearchWords},
                    {"tweet_mode", CoreTweet.TweetMode.extended}
                };
                if (useCursor && _usersCursor != 0)
                    param.Add("page", _usersCursor);

                var following = await Tokens.Users.SearchAsync(param);
                foreach (var user in following)
                    Users.Add(user);

                if (useCursor)
                    _usersCursor += 1;
                else
                    _usersCursor = 2;
            }
            catch
            {
                if (!useCursor || _usersCursor == 0)
                    Users.Clear();

                UpdatingUserSearch = false;
                return;
            }

            if (!useCursor || _usersCursor == 0)
                Users.Clear();

            UpdatingUserSearch = false;
        }

        public async Task CreateSavedSearches(string word)
        {
            try
            {
                await Tokens.SavedSearches.CreateAsync(query => word);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    new ResourceLoader().GetString("Notification_System_NotImplementedException"),
                    new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    new ResourceLoader().GetString("Notification_System_ErrorOccurred"),
                    new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                return;
            }

            Core.Instance.PopupToastNotification(PopupNotificationType.System,
                new ResourceLoader().GetString("Notification_System_SaveSearchSuccessfully"));

            await UpdateSavedSearches(true);
        }

        public async Task DestroySavedSearches(long savedSearchId)
        {
            try
            {
                await Tokens.SavedSearches.DestroyAsync(id => savedSearchId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    new ResourceLoader().GetString("Notification_System_NotImplementedException"),
                    new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    new ResourceLoader().GetString("Notification_System_ErrorOccurred"),
                    new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                return;
            }

            Core.Instance.PopupToastNotification(PopupNotificationType.System,
                new ResourceLoader().GetString("Notification_System_DestroySearchSuccessfully"));

            await UpdateSavedSearches(true);
        }

        public async Task UpdateTrends(bool forceUpdate = false)
        {
            if (UpdatingTrends)
                return;

            if (!forceUpdate && _trendsLastUpdate + TimeSpan.FromMinutes(15) > DateTime.Now && _trendsLastWoeId ==
                SettingSupport.GetTrendsWoeId(SettingService.Setting.TrendsRegion))
                return;

            UpdatingTrends = true;

            TrendsPlace = SettingSupport.GetTrendsPlaceString(SettingService.Setting.TrendsRegion);

            Trends.Clear();
            try
            {
                var trends = await Tokens.Trends.PlaceAsync(
                    id => SettingSupport.GetTrendsWoeId(SettingService.Setting.TrendsRegion));

                if (trends.Count == 0)
                {
                    UpdatingTrends = false;
                    return;
                }

                Trends.Clear();
                foreach (var trend in trends)
                    Trends.Add(trend);

                _trendsLastUpdate = DateTime.Now.ToLocalTime();
                _trendsLastWoeId = SettingSupport.GetTrendsWoeId(SettingService.Setting.TrendsRegion);
            }
            catch
            {
                UpdatingTrends = false;
                return;
            }

            UpdatingTrends = false;
        }

        public async Task UpdateSavedSearches(bool forceUpdate = false)
        {
            if (UpdatingSavedSearches)
                return;

            if (!forceUpdate && _savedSearchesLastUpdate + TimeSpan.FromMinutes(15) > DateTime.Now &&
                _savedSearchesLastScreenName == Tokens.ScreenName)
                return;

            UpdatingSavedSearches = true;

            SavedSearchesScreenName = Tokens.ScreenName;

            SavedSearches.Clear();
            try
            {
                var savedSearches = await Tokens.SavedSearches.ListAsync();

                if (savedSearches.Count == 0)
                {
                    UpdatingSavedSearches = false;
                    return;
                }

                SavedSearches.Clear();
                foreach (var savedSearch in savedSearches)
                    SavedSearches.Add(savedSearch);

                _savedSearchesLastUpdate = DateTime.Now;
                _savedSearchesLastScreenName = Tokens.ScreenName;
            }
            catch
            {
                UpdatingSavedSearches = false;
                return;
            }

            UpdatingSavedSearches = false;
        }

        #region Tokens変更通知プロパティ

        private Tokens _tokens;

        public Tokens Tokens
        {
            get => _tokens;
            set => SetProperty(ref _tokens, value);
        }

        #endregion

        #region UserSearchWords変更通知プロパティ

        private string _userSearchWords;

        public string UserSearchWords
        {
            get => _userSearchWords;
            set => SetProperty(ref _userSearchWords, value);
        }

        #endregion

        #region UpdatingUserSearch変更通知プロパティ

        private bool _updatingUserSearch;

        public bool UpdatingUserSearch
        {
            get => _updatingUserSearch;
            set => SetProperty(ref _updatingUserSearch, value);
        }

        #endregion

        #region StatusSearchWords変更通知プロパティ

        private string _statusSearchWords;

        public string StatusSearchWords
        {
            get => _statusSearchWords;
            set => SetProperty(ref _statusSearchWords, value);
        }

        #endregion

        #region UpdatingStatusSearch変更通知プロパティ

        private bool _updatingStatusSearch;

        public bool UpdatingStatusSearch
        {
            get => _updatingStatusSearch;
            set => SetProperty(ref _updatingStatusSearch, value);
        }

        #endregion

        #region SavedSearchesScreenName変更通知プロパティ

        private string _savedSearchesScreenName;

        public string SavedSearchesScreenName
        {
            get => _savedSearchesScreenName;
            set => SetProperty(ref _savedSearchesScreenName, value);
        }

        #endregion

        #region TrendsPlace変更通知プロパティ

        private string _trendsPlace;

        public string TrendsPlace
        {
            get => _trendsPlace;
            set => SetProperty(ref _trendsPlace, value);
        }

        #endregion
    }
}