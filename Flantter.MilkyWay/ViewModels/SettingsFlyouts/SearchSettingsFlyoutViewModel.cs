using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Search.Core;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class SearchSettingsFlyoutViewModel
    {
        public SearchSettingsFlyoutViewModel()
        {
            this.Model = new SearchSettingsFlyoutModel();

            this.Tokens = this.Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            this.IconSource = new ReactiveProperty<string>("http://localhost/");

            this.StatusSearchWords = new ReactiveProperty<string>();
            this.UserSearchWords = new ReactiveProperty<string>();

            this.PivotSelectedIndex = new ReactiveProperty<int>(0);
            this.PivotSelectedIndex.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (x == 2)
                {
                    await this.Model.UpdateSavedSearches();
                    await this.Model.UpdateTrends();
                }
            });

            this.SavedSearchesScreenName = this.Model.ObserveProperty(x => x.SavedSearchesScreenName).ToReactiveProperty();
            this.TrendPlace = this.Model.ObserveProperty(x => x.TrendsPlace).ToReactiveProperty();

            this.AdvancedSearchOpen = new ReactiveProperty<bool>();
            this.AdvancedSearchContentOpen = new ReactiveProperty<bool>();
            this.AdvancedSearchEngagementOpen = new ReactiveProperty<bool>();

            this.AdvancedSearchContentRetweetsOption = new ReactiveProperty<int>(0);
            this.AdvancedSearchContentShowingOption = new ReactiveProperty<int>(0);
            this.AdvancedSearchContentWrittenInOption = new ReactiveProperty<int>(0);
            this.AdvancedSearchEngagementFavoritesCount = new ReactiveProperty<int>(0);
            this.AdvancedSearchEngagementRetweetsCount = new ReactiveProperty<int>(0);

            this.UpdatingStatusSearch = this.Model.ObserveProperty(x => x.UpdatingStatusSearch).ToReactiveProperty();
            this.UpdatingUserSearch = this.Model.ObserveProperty(x => x.UpdatingUserSearch).ToReactiveProperty();

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                if (this.AdvancedSearchOpen.Value)
                {
                    this.AdvancedSearchContentOpen.Value = false;
                    this.AdvancedSearchEngagementOpen.Value = false;
                    this.AdvancedSearchOpen.Value = false;
                }

                this.StatusSearchWords.Value = "";
                this.UserSearchWords.Value = "";

                this.PivotSelectedIndex.Value = 0;
                this.Model.Statuses.Clear();
                this.Model.Users.Clear();
            });

            this.UpdateStatusSearchCommand = new ReactiveCommand();
            this.UpdateStatusSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (string.IsNullOrWhiteSpace(this.StatusSearchWords.Value))
                {
                    this.Model.Statuses.Clear();
                    this.Model.StatusSearchWords = "";
                    return;
                }

                var searchWords = this.StatusSearchWords.Value;

                switch (this.AdvancedSearchContentShowingOption.Value)
                {
                    case 1:
                        searchWords += " filter:images";
                        break;
                    case 2:
                        searchWords += " filter:videos";
                        break;
                    case 3:
                        searchWords += " filter:vine";
                        break;
                    case 4:
                        searchWords += " (filter:videos OR filter:images)";
                        break;
                    case 5:
                        searchWords += " filter:links";
                        break;
                }

                switch (this.AdvancedSearchContentWrittenInOption.Value)
                {
                    case 1:
                        searchWords += " lang:en";
                        break;
                    case 2:
                        searchWords += " lang:ja";
                        break;
                }

                switch (this.AdvancedSearchContentRetweetsOption.Value)
                {
                    case 1:
                        searchWords += " exclude:retweets";
                        break;
                }

                if (this.AdvancedSearchEngagementRetweetsCount.Value != 0)
                    searchWords += " min_retweets:" + this.AdvancedSearchEngagementRetweetsCount.Value.ToString();

                if (this.AdvancedSearchEngagementRetweetsCount.Value != 0)
                    searchWords += " min_faves:" + this.AdvancedSearchEngagementRetweetsCount.Value.ToString();

                if (this.Model.StatusSearchWords == searchWords)
                {
                    await this.Model.UpdateStatuses(clear: false);
                }
                else
                {
                    this.Model.StatusSearchWords = searchWords;
                    await this.Model.UpdateStatuses();
                }
                
            });

            this.UpdateUserSearchCommand = new ReactiveCommand();
            this.UpdateUserSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (string.IsNullOrWhiteSpace(this.UserSearchWords.Value))
                {
                    this.Model.UserSearchWords = "";
                    this.Model.Users.Clear();
                    return;
                }
                
                this.Model.UserSearchWords = this.UserSearchWords.Value;

                await this.Model.UpdateUsers();
            });
            
            this.SuggestionsRequestedStatusSearchCommand = new ReactiveCommand();
            this.SuggestionsRequestedStatusSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(y =>
            {
                var e = y as SearchBoxSuggestionsRequestedEventArgs;
                if (e == null || string.IsNullOrWhiteSpace(this.StatusSearchWords.Value))
                    return;

                var deferral = e.Request.GetDeferral();
                
                IEnumerable<string> suggestHashtags = null;
                lock (Models.Services.Connecter.Instance.TweetCollecter[this.Tokens.Value.UserId].EntitiesObjectsLock)
                {
                    suggestHashtags = Models.Services.Connecter.Instance.TweetCollecter[this.Tokens.Value.UserId].HashTagObjects.Where(x => x.StartsWith(this.StatusSearchWords.Value.TrimStart(new char[] { '#' }))).OrderBy(x => x);
                }
                if (suggestHashtags.Count() > 0)
                {
                    e.Request.SearchSuggestionCollection.AppendSearchSeparator("HashTag");
                    foreach (var hashtag in suggestHashtags)
                        e.Request.SearchSuggestionCollection.AppendQuerySuggestion("#" + hashtag);
                }

                deferral.Complete();
            });

            this.SuggestionsRequestedUserSearchCommand = new ReactiveCommand();
            this.SuggestionsRequestedUserSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(y =>
            {
                var e = y as SearchBoxSuggestionsRequestedEventArgs;
                if (e == null || string.IsNullOrWhiteSpace(this.UserSearchWords.Value) && this.UserSearchWords.Value.Length <= 1)
                    return;

                var deferral = e.Request.GetDeferral();

                IEnumerable<string> suggestUsers = null;
                lock (Models.Services.Connecter.Instance.TweetCollecter[this.Tokens.Value.UserId].EntitiesObjectsLock)
                {
                    suggestUsers = Models.Services.Connecter.Instance.TweetCollecter[this.Tokens.Value.UserId].ScreenNameObjects.Where(x => x.StartsWith(this.UserSearchWords.Value.TrimStart(new char[] { '@' }))).OrderBy(x => x);
                }
                if (suggestUsers.Count() > 0)
                {
                    e.Request.SearchSuggestionCollection.AppendSearchSeparator("User");
                    foreach (var user in suggestUsers)
                        e.Request.SearchSuggestionCollection.AppendQuerySuggestion(user);
                }

                deferral.Complete();
            });

            this.SaveSearchCommand = new ReactiveCommand();
            this.SaveSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => 
            {
                if (string.IsNullOrWhiteSpace(this.Model.StatusSearchWords))
                    return;

                await this.Model.CreateSavedSearches(this.Model.StatusSearchWords);
            });

            this.DeleteHistoryCommand = new ReactiveCommand();
            this.DeleteHistoryCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var manager = new SearchSuggestionManager();
                manager.ClearHistory();
            });

            this.StatusesIncrementalLoadCommand = new ReactiveCommand();
            this.StatusesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (this.Model.Statuses.Count <= 0)
                    return;

                var id = this.Model.Statuses.Last().Id;
                var status = this.Model.Statuses.Last();
                if (status.HasRetweetInformation)
                    id = status.RetweetInformation.Id;

                await this.Model.UpdateStatuses(id);
            });

            this.UsersIncrementalLoadCommand = new ReactiveCommand();
            this.UsersIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                await this.Model.UpdateUsers(true);
            });

            this.SavedSearchesSelectCommand = new ReactiveCommand();
            this.SavedSearchesSelectCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var e = x as ItemClickEventArgs;
                if (e == null)
                    return;

                var searchQuery = e.ClickedItem as SearchQueryViewModel;
                this.StatusSearchWords.Value = searchQuery.Model.Name;

                this.UpdateStatusSearchCommand.Execute();

                this.PivotSelectedIndex.Value = 0;
            });

            this.TrendsSelectCommand = new ReactiveCommand();
            this.TrendsSelectCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var e = x as ItemClickEventArgs;
                if (e == null)
                    return;

                var trend = e.ClickedItem as TrendViewModel;
                this.StatusSearchWords.Value = trend.Model.Name;

                this.UpdateStatusSearchCommand.Execute();

                this.PivotSelectedIndex.Value = 0;
            });

            Services.Notice.Instance.SearchSettingsFlyoutDeleteSearchQueryCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => 
            {
                var searchQuery = x as SearchQueryViewModel;
                if (searchQuery == null || searchQuery.Model.Id == 0)
                    return;

                await this.Model.DestroySavedSearches(searchQuery.Model.Id);
            });

            this.AdvancedSearchCommand = new ReactiveCommand();
            this.AdvancedSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(_ => 
            {
                if (this.AdvancedSearchOpen.Value)
                {
                    this.AdvancedSearchContentOpen.Value = false;
                    this.AdvancedSearchEngagementOpen.Value = false;
                }
                this.AdvancedSearchOpen.Value = !this.AdvancedSearchOpen.Value;
            });
            this.AdvancedSearchContentCommand = new ReactiveCommand();
            this.AdvancedSearchContentCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(_ => this.AdvancedSearchContentOpen.Value = !this.AdvancedSearchContentOpen.Value);
            this.AdvancedSearchEngagementCommand = new ReactiveCommand();
            this.AdvancedSearchEngagementCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(_ => this.AdvancedSearchEngagementOpen.Value = !this.AdvancedSearchEngagementOpen.Value);

            this.AddColumnCommand = new ReactiveCommand();
            this.AddColumnCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                if (string.IsNullOrWhiteSpace(this.Model.StatusSearchWords))
                    return;

                // Taboo : 禁忌
                bool result = false;
                Windows.UI.Popups.MessageDialog msg = new Windows.UI.Popups.MessageDialog(new ResourceLoader().GetString("ConfirmDialog_Retweet"), "Confirmation");
                msg.Commands.Add(new Windows.UI.Popups.UICommand("Yes", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = true; })));
                msg.Commands.Add(new Windows.UI.Popups.UICommand("No", new Windows.UI.Popups.UICommandInvokedHandler(_ => { result = false; })));
                await msg.ShowAsync();

                if (result)
                    return;

                var columnSetting = new ColumnSetting() { Action = SettingSupport.ColumnTypeEnum.Search, AutoRefresh = false, AutoRefreshTimerInterval = 180.0, Filter = "()", Name = ("Search : " + this.Model.StatusSearchWords), Parameter = this.Model.StatusSearchWords, Streaming = false, Index = -1, DisableStartupRefresh = false, FetchingNumberOfTweet = 40 };
                Services.Notice.Instance.AddColumnCommand.Execute(columnSetting);
            });

            this.Statuses = this.Model.Statuses.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, this.Tokens.Value.UserId));
            this.Users = this.Model.Users.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            this.Trends = this.Model.Trends.ToReadOnlyReactiveCollection(x => new TrendViewModel(x));
            this.SavedSearches = this.Model.SavedSearches.ToReadOnlyReactiveCollection(x => new SearchQueryViewModel(x));

            this.Notice = Services.Notice.Instance;
        }

        public SearchSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<string> StatusSearchWords { get; set; }

        public ReactiveProperty<string> UserSearchWords { get; set; }

        public ReactiveProperty<bool> UpdatingStatusSearch { get; set; }

        public ReactiveProperty<bool> UpdatingUserSearch { get; set; }

        public ReactiveProperty<CoreTweet.Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<int> PivotSelectedIndex { get; set; }


        public ReactiveProperty<string> TrendPlace { get; set; }

        public ReactiveProperty<string> SavedSearchesScreenName { get; set; }


        public ReadOnlyReactiveCollection<StatusViewModel> Statuses { get; private set; }

        public ReadOnlyReactiveCollection<UserViewModel> Users { get; private set; }

        public ReadOnlyReactiveCollection<TrendViewModel> Trends { get; private set; }

        public ReadOnlyReactiveCollection<SearchQueryViewModel> SavedSearches { get; private set; }

        public ReactiveProperty<bool> AdvancedSearchOpen { get; set; }
        public ReactiveProperty<bool> AdvancedSearchContentOpen { get; set; }
        public ReactiveProperty<bool> AdvancedSearchEngagementOpen { get; set; }

        public ReactiveProperty<int> AdvancedSearchContentShowingOption { get; set; }
        public ReactiveProperty<int> AdvancedSearchContentWrittenInOption { get; set; }
        public ReactiveProperty<int> AdvancedSearchContentRetweetsOption { get; set; }

        public ReactiveProperty<int> AdvancedSearchEngagementFavoritesCount { get; set; }
        public ReactiveProperty<int> AdvancedSearchEngagementRetweetsCount { get; set; }

        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateStatusSearchCommand { get; set; }

        public ReactiveCommand UpdateUserSearchCommand { get; set; }

        public ReactiveCommand SuggestionsRequestedStatusSearchCommand { get; set; }
        
        public ReactiveCommand SuggestionsRequestedUserSearchCommand { get; set; }

        public ReactiveCommand SaveSearchCommand { get; set; }

        public ReactiveCommand DeleteHistoryCommand { get; set; }

        public ReactiveCommand StatusesIncrementalLoadCommand { get; set; }

        public ReactiveCommand UsersIncrementalLoadCommand { get; set; }

        public ReactiveCommand SavedSearchesSelectCommand { get; set; }

        public ReactiveCommand TrendsSelectCommand { get; set; }

        public ReactiveCommand AdvancedSearchCommand { get; set; }
        public ReactiveCommand AdvancedSearchContentCommand { get; set; }
        public ReactiveCommand AdvancedSearchEngagementCommand { get; set; }


        public ReactiveCommand AddColumnCommand { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
