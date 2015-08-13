using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Service;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class StatusViewModel
    {
        public StatusViewModel(Status status, ColumnModel column)
        {
            this.Model = status;

            this.CreatedAt = status.ObserveProperty(x => x.CreatedAt).Select(x => x.ToLocalTime().ToString()).ToReactiveProperty();
            this.Source = status.ObserveProperty(x => x.Source).ToReactiveProperty();
            this.Text = status.ObserveProperty(x => x.Text).ToReactiveProperty();
            this.ScreenName = status.User.ObserveProperty(x => x.ScreenName).ToReactiveProperty();
            this.Name = status.User.ObserveProperty(x => x.Name).ToReactiveProperty();
            this.ProfileImageUrl = status.User.ObserveProperty(x => x.ProfileImageUrl).Select(x => !string.IsNullOrWhiteSpace(x) ? x : "http://localhost/").ToReactiveProperty();
            this.Entities = status.ObserveProperty(x => x.Entities).ToReactiveProperty();

            // Todo : 軽量化, バグ修正
            this.BackgroundBrush = Observable.CombineLatest(
                SettingService.Setting.ObserveProperty(x => x.TweetBackgroundBrushAlpha),
                status.ObserveProperty(x => x.HasRetweetInformation),
                status.ObserveProperty(x => x.InReplyToUserId),
                status.ObserveProperty(x => x.IsFavorited),
                status.User.ObserveProperty(x => x.Id),
                (brushAlpha, isRetweet, inReplyToUserId, isFavorited, userId) =>
                {
                    SolidColorBrush backgroundBrush = null;
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (isRetweet)
                            backgroundBrush = (SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"];
                        else if (inReplyToUserId == column.OwnerUserId)
                            backgroundBrush = (SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"];
                        else if (isFavorited == true)
                            backgroundBrush = (SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"];
                        else if (userId == column.OwnerUserId)
                            backgroundBrush = (SolidColorBrush)Application.Current.Resources["TweetMyStatusBackgroundBrush"];
                        else
                            backgroundBrush = (SolidColorBrush)Application.Current.Resources["TweetDefaultBackgroundBrush"];
                    }).AsTask().Wait();
                    return backgroundBrush;
                }).ToReactiveProperty();

            this.RetweetInformationVisibility = status.ObserveProperty(x => x.HasRetweetInformation).Select(x => x ? Visibility.Visible : Visibility.Collapsed).ToReactiveProperty();
            this.MediaVisibility = new ReactiveProperty<Visibility>(status.Entities.Media.Count == 0 ? Visibility.Collapsed : Visibility.Visible);
            this.MediaEntities = this.Model.Entities.Media.ToReadOnlyReactiveCollection(x => new MediaEntityViewModel(x));

            this.RetweetInformationText = Observable.CombineLatest(
                status.ObserveProperty(x => x.RetweetCount),
                status.ObserveProperty(x => x.RetweetInformation),
                (retweetCount, retweetInformation) =>
                {
                    if (retweetInformation == null)
                        return "";

                    if (retweetCount >= 2)
                        return "Retweet by " + retweetInformation.User.ScreenName + " ( " + retweetInformation.User.Name + " ) and " + retweetCount.ToString() + " others";
                    else
                        return "Retweet by " + retweetInformation.User.ScreenName + " ( " + retweetInformation.User.Name + " )";
                }).ToReactiveProperty();
            this.RetweetInformationProfileImageUrl = status.ObserveProperty(x => x.RetweetInformation).Select(x => (x != null && !string.IsNullOrWhiteSpace(x.User.ProfileImageUrl)) ? x.User.ProfileImageUrl : "http://localhost/").ToReactiveProperty();
            this.RetweetInformationScreenName = status.ObserveProperty(x => x.RetweetInformation).Select(x => x != null ? x.User.ScreenName : "").ToReactiveProperty();

            this.RetweetCounterVisibility = Observable.CombineLatest(
                status.ObserveProperty(x => x.RetweetCount),
                status.ObserveProperty(x => x.HasRetweetInformation),
                (retweetCount, hasRetweetInformation) =>
                {
                    if (!hasRetweetInformation && retweetCount > 0)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }).ToReactiveProperty();
            this.RetweetCounterText = Observable.CombineLatest(
                status.ObserveProperty(x => x.RetweetCount),
                status.ObserveProperty(x => x.HasRetweetInformation),
                (retweetCount, hasRetweetInformation) =>
                {
                    if (!hasRetweetInformation && retweetCount > 0)
                        return "Retweeted " + retweetCount.ToString() + " times";
                    else
                        return "";
                }).ToReactiveProperty();

            this.RetweetTriangleIconVisibility = Observable.CombineLatest(
                status.ObserveProperty(x => x.IsRetweeted),
                status.ObserveProperty(x => x.IsFavorited),
                (isRetweeted, isFavorited) =>
                {
                    if (isRetweeted && !isFavorited)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }).ToReactiveProperty();
            this.FavoriteTriangleIconVisibility = Observable.CombineLatest(
                status.ObserveProperty(x => x.IsRetweeted),
                status.ObserveProperty(x => x.IsFavorited),
                (isRetweeted, isFavorited) =>
                {
                    if (!isRetweeted && isFavorited)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }).ToReactiveProperty();
            this.RetweetFavoriteTriangleIconVisibility = Observable.CombineLatest(
                status.ObserveProperty(x => x.IsRetweeted),
                status.ObserveProperty(x => x.IsFavorited),
                (isRetweeted, isFavorited) =>
                {
                    if (isRetweeted && isFavorited)
                        return Visibility.Visible;
                    else
                        return Visibility.Collapsed;
                }).ToReactiveProperty();

            this.QuotedStatusVisibility = status.ObserveProperty(x => x.QuotedStatusId).Select(x => x != 0 ? Visibility.Visible : Visibility.Collapsed).ToReactiveProperty();
            this.QuotedStatusName = status.ObserveProperty(x => x.QuotedStatus).Select(x => x != null ? x.User.Name : "").ToReactiveProperty();
            this.QuotedStatusScreenName = status.ObserveProperty(x => x.QuotedStatus).Select(x => x != null ? x.User.ScreenName : "").ToReactiveProperty();
            this.QuotedStatusText = status.ObserveProperty(x => x.QuotedStatus).Select(x => x != null ? x.Text : "").ToReactiveProperty();
            this.QuotedStatusProfileImageUrl = status.ObserveProperty(x => x.QuotedStatus).Select(x => (x != null && !string.IsNullOrWhiteSpace(x.User.ProfileImageUrl)) ? x.User.ProfileImageUrl : "http://localhost/").ToReactiveProperty();
            this.QuotedStatusId = status.ObserveProperty(x => x.QuotedStatusId).ToReactiveProperty();
            this.QuotedStatusEntities = status.ObserveProperty(x => x.QuotedStatus).Select(x => x != null ? x.Entities : null).ToReactiveProperty();

            this.Notice = new ReactiveProperty<Service.Notice>(Service.Notice.Instance);
        }

        public Status Model { get; private set; }

        public ReactiveProperty<string> CreatedAt { get; set; }

        public ReactiveProperty<string> Source { get; set; }

        public ReactiveProperty<string> Text { get; set; }

        public ReactiveProperty<string> ScreenName { get; set; }

        public ReactiveProperty<string> Name { get; set; }
        
        public ReactiveProperty<string> ProfileImageUrl { get; set; }

        public ReactiveProperty<Entities> Entities { get; set; }

        public ReactiveProperty<SolidColorBrush> BackgroundBrush { get; set; }

        public ReactiveProperty<Visibility> RetweetInformationVisibility { get; set; }
        
        public ReactiveProperty<Visibility> MediaVisibility { get; set; }

        public ReadOnlyReactiveCollection<MediaEntityViewModel> MediaEntities { get; private set; }

        public ReactiveProperty<string> RetweetInformationText { get; set; }

        public ReactiveProperty<string> RetweetInformationProfileImageUrl { get; set; }

        public ReactiveProperty<string> RetweetInformationScreenName { get; set; }

        public ReactiveProperty<Visibility> RetweetCounterVisibility { get; set; }

        public ReactiveProperty<string> RetweetCounterText { get; set; }

        public ReactiveProperty<Visibility> RetweetTriangleIconVisibility { get; set; }

        public ReactiveProperty<Visibility> FavoriteTriangleIconVisibility { get; set; }

        public ReactiveProperty<Visibility> RetweetFavoriteTriangleIconVisibility { get; set; }

        public ReactiveProperty<Visibility> QuotedStatusVisibility { get; set; }

        public ReactiveProperty<string> QuotedStatusScreenName { get; set; }

        public ReactiveProperty<string> QuotedStatusName { get; set; }

        public ReactiveProperty<string> QuotedStatusText { get; set; }

        public ReactiveProperty<string> QuotedStatusProfileImageUrl { get; set; }

        public ReactiveProperty<long> QuotedStatusId { get; set; }

        public ReactiveProperty<Entities> QuotedStatusEntities { get; set; }

        public ReactiveProperty<Service.Notice> Notice { get; set; }
    }
}
