using Flantter.MilkyWay.Models.SettingsFlyouts;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            this.ClearCommand = new ReactiveCommand();
            this.ClearCommand.Subscribe(x =>
            {
                this.PivotSelectedIndex.Value = 0;
                this.Model.Statuses.Clear();
                this.Model.Users.Clear();
            });

            this.UpdateStatusSearchCommand = new ReactiveCommand();
            this.UpdateStatusSearchCommand.Subscribe(async x =>
            {
                if (string.IsNullOrWhiteSpace(StatusSearchWords.Value))
                {
                    this.Model.Statuses.Clear();
                    return;
                }
                    

                this.Model.StatusSearchWords = StatusSearchWords.Value;

                await this.Model.UpdateStatuses();
            });

            this.UpdateUserSearchCommand = new ReactiveCommand();
            this.UpdateUserSearchCommand.Subscribe(async x =>
            {
                if (string.IsNullOrWhiteSpace(UserSearchWords.Value))
                {
                    this.Model.Users.Clear();
                    return;
                }
                
                this.Model.UserSearchWords = UserSearchWords.Value;

                await this.Model.UpdateUsers();
            });
            
            this.SuggestionsRequestedStatusSearchCommand = new ReactiveCommand();
            this.SuggestionsRequestedStatusSearchCommand.Subscribe(async x =>
            {
                var e = x as SearchBoxSuggestionsRequestedEventArgs;
                if (e == null)
                    return;

                var deferral = e.Request.GetDeferral();

                e.Request.SearchSuggestionCollection.AppendSearchSeparator("HashTag");
                e.Request.SearchSuggestionCollection.AppendQuerySuggestion("#" + "ふらんちゃん");

                deferral.Complete();
            });

            this.Statuses = this.Model.Statuses.ToReadOnlyReactiveCollection(x => new StatusViewModel(x));
            this.Users = this.Model.Users.ToReadOnlyReactiveCollection(x => new UserViewModel(x));

            this.UpdatingStatusSearch = this.Model.ObserveProperty(x => x.UpdatingStatusSearch).ToReactiveProperty();
            this.UpdatingUserSearch = this.Model.ObserveProperty(x => x.UpdatingUserSearch).ToReactiveProperty();

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

        public ReadOnlyReactiveCollection<StatusViewModel> Statuses { get; private set; }

        public ReadOnlyReactiveCollection<UserViewModel> Users { get; private set; }


        public ReactiveCommand ClearCommand { get; set; }

        public ReactiveCommand UpdateStatusSearchCommand { get; set; }

        public ReactiveCommand UpdateUserSearchCommand { get; set; }

        public ReactiveCommand SuggestionsRequestedStatusSearchCommand { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
