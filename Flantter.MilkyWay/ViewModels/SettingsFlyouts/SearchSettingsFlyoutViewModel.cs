using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.ApplicationModel.Search.Core;
using Windows.UI.Xaml.Controls;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Apis.Objects;
using Flantter.MilkyWay.ViewModels.Services;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts
{
    public class SearchSettingsFlyoutViewModel
    {
        public SearchSettingsFlyoutViewModel()
        {
            Model = new SearchSettingsFlyoutModel();

            Tokens = Model.ToReactivePropertyAsSynchronized(x => x.Tokens);
            IconSource = new ReactiveProperty<string>("http://localhost/");

            StatusSearchWords = new ReactiveProperty<string>();
            UserSearchWords = new ReactiveProperty<string>();

            PivotSelectedIndex = new ReactiveProperty<int>(0);
            PivotSelectedIndex.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (x == 2)
                    {
                        await Model.UpdateSavedSearches();
                        await Model.UpdateTrends();
                    }
                });

            SavedSearchesScreenName = Model.ObserveProperty(x => x.SavedSearchesScreenName).ToReactiveProperty();
            TrendPlace = Model.ObserveProperty(x => x.TrendsPlace).ToReactiveProperty();

            AdvancedSearchOpen = new ReactiveProperty<bool>();
            AdvancedSearchContentOpen = new ReactiveProperty<bool>();
            AdvancedSearchEngagementOpen = new ReactiveProperty<bool>();

            AdvancedSearchContentRetweetsOption = new ReactiveProperty<int>(0);
            AdvancedSearchContentShowingOption = new ReactiveProperty<int>(0);
            AdvancedSearchContentWrittenInOption = new ReactiveProperty<int>(0);
            AdvancedSearchEngagementFavoritesCount = new ReactiveProperty<int>(0);
            AdvancedSearchEngagementRetweetsCount = new ReactiveProperty<int>(0);

            UpdatingStatusSearch = Model.ObserveProperty(x => x.UpdatingStatusSearch).ToReactiveProperty();
            UpdatingUserSearch = Model.ObserveProperty(x => x.UpdatingUserSearch).ToReactiveProperty();

            StatusSuggestion = new ReactiveCollection<string>();
            UserSuggestion = new ReactiveCollection<string>();

            ClearCommand = new ReactiveCommand();
            ClearCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (AdvancedSearchOpen.Value)
                    {
                        AdvancedSearchContentOpen.Value = false;
                        AdvancedSearchEngagementOpen.Value = false;
                        AdvancedSearchOpen.Value = false;
                    }

                    StatusSearchWords.Value = "";
                    UserSearchWords.Value = "";

                    PivotSelectedIndex.Value = 0;
                    Model.Statuses.Clear();
                    Model.Users.Clear();
                });

            UpdateStatusSearchCommand = new ReactiveCommand();
            UpdateStatusSearchCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var e = x as AutoSuggestBoxQuerySubmittedEventArgs;
                    if (!string.IsNullOrWhiteSpace(e?.QueryText))
                        StatusSearchWords.Value = e.QueryText;

                    if (string.IsNullOrWhiteSpace(StatusSearchWords.Value))
                    {
                        Model.Statuses.Clear();
                        Model.StatusSearchWords = "";
                        return;
                    }

                    var searchWords = StatusSearchWords.Value;

                    switch (AdvancedSearchContentShowingOption.Value)
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
                            searchWords += " filter:media";
                            break;
                        case 5:
                            searchWords += " filter:links";
                            break;
                    }

                    switch (AdvancedSearchContentWrittenInOption.Value)
                    {
                        case 1:
                            searchWords += " lang:en";
                            break;
                        case 2:
                            searchWords += " lang:ja";
                            break;
                    }

                    switch (AdvancedSearchContentRetweetsOption.Value)
                    {
                        case 1:
                            searchWords += " exclude:retweets";
                            break;
                    }

                    if (AdvancedSearchEngagementRetweetsCount.Value != 0)
                        searchWords += " min_retweets:" + AdvancedSearchEngagementRetweetsCount.Value.ToString();

                    if (AdvancedSearchEngagementFavoritesCount.Value != 0)
                        searchWords += " min_faves:" + AdvancedSearchEngagementFavoritesCount.Value.ToString();

                    if (Model.StatusSearchWords == searchWords)
                    {
                        await Model.UpdateStatuses(clear: false);
                    }
                    else
                    {
                        Model.StatusSearchWords = searchWords;
                        await Model.UpdateStatuses();
                    }
                });

            UpdateUserSearchCommand = new ReactiveCommand();
            UpdateUserSearchCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var e = x as AutoSuggestBoxQuerySubmittedEventArgs;
                    if (!string.IsNullOrWhiteSpace(e?.QueryText))
                        UserSearchWords.Value = e.QueryText;

                    if (string.IsNullOrWhiteSpace(UserSearchWords.Value))
                    {
                        Model.Users.Clear();
                        Model.UserSearchWords = "";
                        return;
                    }

                    Model.UserSearchWords = UserSearchWords.Value;

                    await Model.UpdateUsers();
                });

            TextChangedStatusSearchCommand = new ReactiveCommand();
            TextChangedStatusSearchCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(y =>
                {
                    var e = y as AutoSuggestBoxTextChangedEventArgs;
                    if (e == null || string.IsNullOrWhiteSpace(StatusSearchWords.Value))
                    {
                        StatusSuggestion.ClearOnScheduler();
                        return;
                    }

                    IEnumerable<string> suggestHashtags;
                    lock (Connecter.Instance.TweetCollecter[Tokens.Value.UserId + ":" + Tokens.Value.Instance].EntitiesObjectsLock)
                    {
                        suggestHashtags = Connecter.Instance.TweetCollecter[Tokens.Value.UserId + ":" + Tokens.Value.Instance]
                            .HashTagObjects.Where(x => x.StartsWith(StatusSearchWords.Value.TrimStart('#')))
                            .OrderBy(x => x).Select(x => "#" + x);
                    }
                    if (suggestHashtags.Any())
                    {
                        StatusSuggestion.ClearOnScheduler();
                        StatusSuggestion.AddRangeOnScheduler(suggestHashtags);
                    }
                    else
                    {
                        StatusSuggestion.ClearOnScheduler();
                    }
                });

            TextChangedUserSearchCommand = new ReactiveCommand();
            TextChangedUserSearchCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(y =>
                {
                    var e = y as AutoSuggestBoxTextChangedEventArgs;
                    if (e == null || string.IsNullOrWhiteSpace(UserSearchWords.Value) &&
                        UserSearchWords.Value.Length <= 1)
                    {
                        UserSuggestion.ClearOnScheduler();
                        return;
                    }

                    IEnumerable<string> suggestUsers;
                    lock (Connecter.Instance.TweetCollecter[Tokens.Value.UserId + ":" + Tokens.Value.Instance].EntitiesObjectsLock)
                    {
                        suggestUsers = Connecter.Instance.TweetCollecter[Tokens.Value.UserId + ":" + Tokens.Value.Instance]
                            .ScreenNameObjects.Where(x => x.Key.StartsWith(UserSearchWords.Value.TrimStart('@')))
                            .OrderBy(x => x.Key).Select(x => x.Key);
                    }
                    if (suggestUsers.Any())
                    {
                        UserSuggestion.ClearOnScheduler();
                        UserSuggestion.AddRangeOnScheduler(suggestUsers);
                    }
                    else
                    {
                        UserSuggestion.ClearOnScheduler();
                    }
                });

            SaveSearchCommand = new ReactiveCommand();
            SaveSearchCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (string.IsNullOrWhiteSpace(Model.StatusSearchWords))
                        return;

                    await Model.CreateSavedSearches(Model.StatusSearchWords);
                });

            DeleteHistoryCommand = new ReactiveCommand();
            DeleteHistoryCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var manager = new SearchSuggestionManager();
                    manager.ClearHistory();
                });

            StatusesIncrementalLoadCommand = new ReactiveCommand();
            StatusesIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    if (Model.Statuses.Count <= 0)
                        return;

                    var id = Model.Statuses.Last().Id;
                    var status = Model.Statuses.Last();
                    if (status.HasRetweetInformation)
                        id = status.RetweetInformation.Id;

                    await Model.UpdateStatuses(id);
                });

            UsersIncrementalLoadCommand = new ReactiveCommand();
            UsersIncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.UpdateUsers(true); });

            SavedSearchesSelectCommand = new ReactiveCommand();
            SavedSearchesSelectCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var e = x as ItemClickEventArgs;
                    if (e == null)
                        return;

                    var searchQuery = e.ClickedItem as SearchQueryViewModel;
                    StatusSearchWords.Value = searchQuery.Model.Name;

                    UpdateStatusSearchCommand.Execute();

                    PivotSelectedIndex.Value = 0;
                });

            TrendsSelectCommand = new ReactiveCommand();
            TrendsSelectCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var e = x as ItemClickEventArgs;
                    if (e == null)
                        return;

                    var trend = e.ClickedItem as TrendViewModel;
                    StatusSearchWords.Value = trend.Model.Name;

                    UpdateStatusSearchCommand.Execute();

                    PivotSelectedIndex.Value = 0;
                });

            Notice.Instance.SearchSettingsFlyoutDeleteSearchQueryCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var searchQuery = x as SearchQueryViewModel;
                    if (searchQuery == null || searchQuery.Model.Id == 0)
                        return;

                    await Model.DestroySavedSearches(searchQuery.Model.Id);
                });

            AdvancedSearchCommand = new ReactiveCommand();
            AdvancedSearchCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(_ =>
                {
                    if (AdvancedSearchOpen.Value)
                    {
                        AdvancedSearchContentOpen.Value = false;
                        AdvancedSearchEngagementOpen.Value = false;
                    }
                    AdvancedSearchOpen.Value = !AdvancedSearchOpen.Value;
                });
            AdvancedSearchContentCommand = new ReactiveCommand();
            AdvancedSearchContentCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(_ => AdvancedSearchContentOpen.Value = !AdvancedSearchContentOpen.Value);
            AdvancedSearchEngagementCommand = new ReactiveCommand();
            AdvancedSearchEngagementCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(_ => AdvancedSearchEngagementOpen.Value = !AdvancedSearchEngagementOpen.Value);

            AddColumnCommand = new ReactiveCommand();
            AddColumnCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (string.IsNullOrWhiteSpace(Model.StatusSearchWords))
                        return;

                    var columnSetting = new ColumnSetting
                    {
                        Action = SettingSupport.ColumnTypeEnum.Search,
                        AutoRefresh = false,
                        AutoRefreshTimerInterval = 180.0,
                        Filter = "()",
                        Name = "Search : " + Model.StatusSearchWords,
                        Parameter = Model.StatusSearchWords,
                        Streaming = false,
                        Index = -1,
                        DisableStartupRefresh = false,
                        FetchingNumberOfTweet = 40
                    };
                    Notice.Instance.AddColumnCommand.Execute(columnSetting);
                });

            Statuses = Model.Statuses.ToReadOnlyReactiveCollection(x => new StatusViewModel(x, Tokens.Value.UserId));
            Users = Model.Users.ToReadOnlyReactiveCollection(x => new UserViewModel(x));
            Trends = Model.Trends.ToReadOnlyReactiveCollection(x => new TrendViewModel(x));
            SavedSearches = Model.SavedSearches.ToReadOnlyReactiveCollection(x => new SearchQueryViewModel(x));

            Notice = Notice.Instance;
        }

        public SearchSettingsFlyoutModel Model { get; set; }

        public ReactiveProperty<string> StatusSearchWords { get; set; }

        public ReactiveProperty<string> UserSearchWords { get; set; }

        public ReactiveProperty<bool> UpdatingStatusSearch { get; set; }

        public ReactiveProperty<bool> UpdatingUserSearch { get; set; }

        public ReactiveProperty<Tokens> Tokens { get; set; }

        public ReactiveProperty<string> IconSource { get; set; }

        public ReactiveProperty<int> PivotSelectedIndex { get; set; }


        public ReactiveProperty<string> TrendPlace { get; set; }

        public ReactiveProperty<string> SavedSearchesScreenName { get; set; }


        public ReadOnlyReactiveCollection<StatusViewModel> Statuses { get; }

        public ReadOnlyReactiveCollection<UserViewModel> Users { get; }

        public ReadOnlyReactiveCollection<TrendViewModel> Trends { get; }

        public ReadOnlyReactiveCollection<SearchQueryViewModel> SavedSearches { get; }

        public ReactiveCollection<string> StatusSuggestion { get; }

        public ReactiveCollection<string> UserSuggestion { get; }

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

        public ReactiveCommand TextChangedStatusSearchCommand { get; set; }

        public ReactiveCommand TextChangedUserSearchCommand { get; set; }

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

        public Notice Notice { get; set; }
    }
}