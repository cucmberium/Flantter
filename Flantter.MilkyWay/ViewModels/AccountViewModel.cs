using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Flantter.MilkyWay.Views.Behaviors;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.ViewModels
{
    public class AccountViewModel : IDisposable
    {
        public AccountModel _AccountModel { get; set; }
        public Services.Notice Notice { get; set; }

        public ReadOnlyReactiveCollection<ColumnViewModel> Columns { get; private set; }
        public ObservableCollection<string> OtherColumnNames { get; private set; }
        private IDisposable OtherColumnNamesDisposable { get; set; }

        public ReactiveProperty<string> ProfileImageUrl { get; private set; }
        public ReactiveProperty<string> ProfileBannerUrl { get; private set; }
        public ReactiveProperty<string> ScreenName { get; private set; }
        public ReactiveProperty<bool> IsEnabled { get; private set; }
        public ReactiveProperty<double> PanelWidth { get; private set; }
        public ReactiveProperty<double> SnapPointsSpaceing { get; private set; }
        public ReactiveProperty<double> MaxSnapPoint { get; private set; }
		public ReactiveProperty<double> MinSnapPoint { get; private set; }

        public ReactiveProperty<int> ColumnSelectedIndex { get; private set; }
        
        #region Constructor
        /*public AccountViewModel()
        {
        }*/

        public AccountViewModel(AccountModel account)
        {
            this._AccountModel = account;
            this.ScreenName = account.ObserveProperty(x => x.ScreenName).ToReactiveProperty();
            this.ProfileImageUrl = account.ObserveProperty(x => x.ProfileImageUrl).Select(x => !string.IsNullOrWhiteSpace(x) ? x : "http://localhost/").ToReactiveProperty();
            this.ProfileBannerUrl = account.ObserveProperty(x => x.ProfileBannerUrl).Select(x => !string.IsNullOrWhiteSpace(x) ? x : "http://localhost/").ToReactiveProperty();
            this.IsEnabled = account.ObserveProperty(x => x.IsEnabled).ToReactiveProperty();

            this.Columns = this._AccountModel.ReadOnlyColumns.ToReadOnlyReactiveCollection(x => new ColumnViewModel(x));

            this.OtherColumnNames = new ObservableCollection<string>(this._AccountModel.ReadOnlyColumns.ToObservable().Where(x => x.Name != "Home" && x.Name != "Mentions" && x.Name != "DirectMessages").Select(x => x.Name).ToEnumerable());
            this.OtherColumnNamesDisposable = this._AccountModel.ReadOnlyColumns.CollectionChangedAsObservable().SubscribeOnUIDispatcher().Subscribe<NotifyCollectionChangedEventArgs>(e => 
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var obj in e.NewItems)
                        {
                            var column = obj as ColumnModel;
                            if (column.Name == "Home" || column.Name == "Mentions" || column.Name == "DirectMessages")
                                continue;

                            this.OtherColumnNames.Add(column.Name);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        foreach (var obj in e.OldItems)
                        {
                            var column = obj as ColumnModel;
                            if (column.Name == "Home" || column.Name == "Mentions" || column.Name == "DirectMessages")
                                continue;

                            this.OtherColumnNames.Remove(column.Name);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                    case NotifyCollectionChangedAction.Reset:
                        throw new NotImplementedException();
                }
            });

            this.PanelWidth = Observable.CombineLatest<double, int, double>(
                LayoutHelper.Instance.ColumnWidth,
                this.Columns.ObserveProperty(x => x.Count),
                (width, count) =>
                {
                    if (width < 352.0)
                        return width * count + 352.0 * 2;
                    else
                        return (width + 10.0) * count + 352.0 * 2;
                }).ToReactiveProperty();

            this.SnapPointsSpaceing = LayoutHelper.Instance.ColumnWidth.Select(x => x + 10.0).ToReactiveProperty();

            this.MaxSnapPoint = Observable.CombineLatest<double, double, double>(this.PanelWidth, WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth), (panelWidth, windowWidth) => (panelWidth + 10.0) - windowWidth + 352.0).ToReactiveProperty();

			this.MinSnapPoint = new ReactiveProperty<double>(352.0);

            this.ColumnSelectedIndex = new ReactiveProperty<int>(0);

            this.Notice = Services.Notice.Instance;

            #region Command

            Services.Notice.Instance.LoadMentionCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                if (statusViewModel.Model.InReplyToStatusId == 0)
                    return;

                statusViewModel.IsMentionStatusLoading = true;

                await this._AccountModel.GetMentionStatus(statusViewModel.Model);

                if (statusViewModel.Model.MentionStatus == null)
                {
                    statusViewModel.IsMentionStatusLoading = false;
                    statusViewModel.MentionStatusVisibility = false;
                    statusViewModel.OnPropertyChanged("IsMentionStatusLoading");
                    statusViewModel.OnPropertyChanged("MentionStatusVisibility");
                    return;
                }

                // この設計はメモリ使用量削減に貢献しているのだろうか・・・？

                statusViewModel.MentionStatusEntities = statusViewModel.Model.MentionStatus.Entities;
                statusViewModel.MentionStatusId = statusViewModel.Model.MentionStatus.Id;
                statusViewModel.MentionStatusName = statusViewModel.Model.MentionStatus.User.Name;
                statusViewModel.MentionStatusProfileImageUrl = string.IsNullOrWhiteSpace(statusViewModel.Model.MentionStatus.User.ProfileImageUrl) ? "http://localhost/" : statusViewModel.Model.MentionStatus.User.ProfileImageUrl;
                statusViewModel.MentionStatusScreenName = statusViewModel.Model.MentionStatus.User.ScreenName;
                statusViewModel.MentionStatusText = statusViewModel.Model.MentionStatus.Text;

                statusViewModel.OnPropertyChanged("MentionStatusEntities");
                statusViewModel.OnPropertyChanged("MentionStatusId");
                statusViewModel.OnPropertyChanged("MentionStatusName");
                statusViewModel.OnPropertyChanged("MentionStatusProfileImageUrl");
                statusViewModel.OnPropertyChanged("MentionStatusScreenName");
                statusViewModel.OnPropertyChanged("MentionStatusText");

                statusViewModel.IsMentionStatusLoading = false;
                statusViewModel.IsMentionStatusLoaded = true;

                statusViewModel.OnPropertyChanged("IsMentionStatusLoading");
                statusViewModel.OnPropertyChanged("IsMentionStatusLoaded");
            });

            Services.Notice.Instance.RetweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                if (!statusViewModel.Model.IsRetweeted)
                    await this._AccountModel.Retweet(statusViewModel.Model);
                else
                    await this._AccountModel.DestroyRetweet(statusViewModel.Model);

                statusViewModel.IsRetweeted = statusViewModel.Model.IsRetweeted;
                statusViewModel.OnPropertyChanged("IsRetweeted");

                if (!statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                    statusViewModel.FavoriteTriangleIconVisibility = true;
                else
                    statusViewModel.FavoriteTriangleIconVisibility = false;
                if (statusViewModel.Model.IsRetweeted && !statusViewModel.Model.IsFavorited)
                    statusViewModel.RetweetTriangleIconVisibility = true;
                else
                    statusViewModel.RetweetTriangleIconVisibility = false;
                if (statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                    statusViewModel.RetweetFavoriteTriangleIconVisibility = true;
                else
                    statusViewModel.RetweetFavoriteTriangleIconVisibility = false;

                statusViewModel.OnPropertyChanged("FavoriteTriangleIconVisibility");
                statusViewModel.OnPropertyChanged("RetweetTriangleIconVisibility");
                statusViewModel.OnPropertyChanged("RetweetFavoriteTriangleIconVisibility");
            });

            Services.Notice.Instance.FavoriteCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                if (!statusViewModel.Model.IsFavorited)
                    await this._AccountModel.Favorite(statusViewModel.Model);
                else
                    await this._AccountModel.DestroyFavorite(statusViewModel.Model);

                statusViewModel.IsFavorited = statusViewModel.Model.IsFavorited;
                statusViewModel.OnPropertyChanged("IsFavorited");

                if (!statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                    statusViewModel.FavoriteTriangleIconVisibility = true;
                else
                    statusViewModel.FavoriteTriangleIconVisibility = false;
                if (statusViewModel.Model.IsRetweeted && !statusViewModel.Model.IsFavorited)
                    statusViewModel.RetweetTriangleIconVisibility = true;
                else
                    statusViewModel.RetweetTriangleIconVisibility = false;
                if (statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                    statusViewModel.RetweetFavoriteTriangleIconVisibility = true;
                else
                    statusViewModel.RetweetFavoriteTriangleIconVisibility = false;

                statusViewModel.OnPropertyChanged("FavoriteTriangleIconVisibility");
                statusViewModel.OnPropertyChanged("RetweetTriangleIconVisibility");
                statusViewModel.OnPropertyChanged("RetweetFavoriteTriangleIconVisibility");
            });

            Services.Notice.Instance.DeleteRetweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;
                
                await this._AccountModel.DestroyRetweet(statusViewModel.Model);

                statusViewModel.IsRetweeted = statusViewModel.Model.IsRetweeted;
                statusViewModel.OnPropertyChanged("IsRetweeted");

                if (!statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                    statusViewModel.FavoriteTriangleIconVisibility = true;
                else
                    statusViewModel.FavoriteTriangleIconVisibility = false;
                if (statusViewModel.Model.IsRetweeted && !statusViewModel.Model.IsFavorited)
                    statusViewModel.RetweetTriangleIconVisibility = true;
                else
                    statusViewModel.RetweetTriangleIconVisibility = false;
                if (statusViewModel.Model.IsRetweeted && statusViewModel.Model.IsFavorited)
                    statusViewModel.RetweetFavoriteTriangleIconVisibility = true;
                else
                    statusViewModel.RetweetFavoriteTriangleIconVisibility = false;

                statusViewModel.OnPropertyChanged("FavoriteTriangleIconVisibility");
                statusViewModel.OnPropertyChanged("RetweetTriangleIconVisibility");
                statusViewModel.OnPropertyChanged("RetweetFavoriteTriangleIconVisibility");
            });
            
            Services.Notice.Instance.ShowUserProfileCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserProfile", Tokens = this._AccountModel._Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowConversationCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "Conversation", Tokens = this._AccountModel._Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowSearchCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "Search", Tokens = this._AccountModel._Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.AddColumnCommand.SubscribeOn(ThreadPoolScheduler.Default).Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var setting = x as ColumnSetting;
                if (setting == null)
                    return;

                this._AccountModel.AddColumn(setting);
            });

            #endregion
        }
        #endregion

        #region Destructor
        ~AccountViewModel()
        {
        }
        #endregion

        public void Dispose()
        {
            this.ScreenName.Dispose();
            this.ProfileImageUrl.Dispose();
            this.ProfileBannerUrl.Dispose();
            this.IsEnabled.Dispose();
            this.Columns.Dispose();
            this.OtherColumnNamesDisposable.Dispose();
            this.PanelWidth.Dispose();
            this.SnapPointsSpaceing.Dispose();
            this.MaxSnapPoint.Dispose();
            this.MinSnapPoint.Dispose();
            this.ColumnSelectedIndex.Dispose();
        }
    }
}
