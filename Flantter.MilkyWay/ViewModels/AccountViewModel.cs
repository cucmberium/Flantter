using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Flantter.MilkyWay.Views.Behaviors;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
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
        public ReadOnlyReactiveCollection<string> AdditionalColumnsName { get; private set; }

        public ReactiveProperty<string> ProfileImageUrl { get; private set; }
        public ReactiveProperty<string> ProfileBannerUrl { get; private set; }
        public ReactiveProperty<string> ScreenName { get; private set; }
        public ReactiveProperty<bool> IsEnabled { get; private set; }
        public ReactiveProperty<double> PanelWidth { get; private set; }
        public ReactiveProperty<int> ColumnCount { get; private set; }
        public ReactiveProperty<double> ColumnWidth { get; private set; }
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
            this.AdditionalColumnsName = this._AccountModel.ReadOnlyColumns.ToObservable().Where(x => x.Name != "Home" && x.Name != "Mentions" && x.Name != "DirectMessages").Select(x => x.Name).ToReadOnlyReactiveCollection();
            
            this.ColumnCount = Observable.CombineLatest<double, double, int, int>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                SettingService.Setting.ObserveProperty(x => x.MinColumnSize),
                SettingService.Setting.ObserveProperty(x => x.MaxColumnCount),
                (width, minWidth, maxCount) =>
                {
                    return (int)Math.Max(Math.Min(maxCount, (width - 5.0 * 2) / (minWidth + 5.0 * 2)), 1.0);
                }).ToReactiveProperty();

            this.ColumnWidth = Observable.CombineLatest<double, int, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                this.ColumnCount,
                (width, count) =>
                {
                    if (width < 352.0)
                        return width;
                    else
                        return (width - 5.0 * 2) / count - 10.0;
                }).ToReactiveProperty();

            this.PanelWidth = Observable.CombineLatest<double, int, double>(
                this.ColumnWidth,
                this.Columns.ObserveProperty(x => x.Count),
                (width, count) =>
                {
                    if (width < 352.0)
                        return width * count + 352.0 * 2;
                    else
                        return (width + 10.0) * count + 352.0 * 2;
                }).ToReactiveProperty();

            this.SnapPointsSpaceing = this.ColumnWidth.Select(x => {
                return x + 10.0;
            }).ToReactiveProperty();

            this.MaxSnapPoint = Observable.CombineLatest<double, double, double>(this.PanelWidth, WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth), (panelWidth, windowWidth) => (panelWidth + 10.0) - windowWidth + 352.0).ToReactiveProperty();

			this.MinSnapPoint = new ReactiveProperty<double>(352.0);

            this.ColumnSelectedIndex = new ReactiveProperty<int>(0);

            this.Notice = Services.Notice.Instance;

            #region Command

            Services.Notice.Instance.LoadMentionCommand.Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
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

            Services.Notice.Instance.RetweetCommand.Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
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

            Services.Notice.Instance.FavoriteCommand.Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
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

            Services.Notice.Instance.DeleteRetweetCommand.Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
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

            Services.Notice.Instance.UrlClickCommand.Where(_ => this._AccountModel.IsEnabled).Subscribe(async x =>
            {
                var linkUrl = x as string;
                if (string.IsNullOrWhiteSpace(linkUrl))
                    return;


                if (linkUrl.StartsWith("@"))
                {
                    var userMention = linkUrl.Replace("@", "");
                    ViewModels.Services.Notice.Instance.ShowUserProfileCommand.Execute(userMention.Replace("@", ""));
                    return;
                }
                else if (linkUrl.StartsWith("#"))
                {
                    var hashTag = linkUrl.Replace("#", "");
                    return;
                }

                var statusMatch = TweetRegexPatterns.StatusUrl.Match(linkUrl);
                var userMatch = TweetRegexPatterns.UserUrl.Match(linkUrl);
                if (statusMatch.Success)
                { }
                else if (userMatch.Success)
                { }
                else
                {
                    await Launcher.LaunchUriAsync(new Uri(linkUrl));
                }
            });

            Services.Notice.Instance.CopyTweetCommand.Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var status = x as Status;
                if (status != null)
                {
                    try
                    {
                        var textPackage = new DataPackage();
                        textPackage.SetText(status.Text);
                        Clipboard.SetContent(textPackage);
                    }
                    catch
                    {
                    }

                    return;
                }

                var directMessage = x as DirectMessage;
                if (directMessage != null)
                {
                    try
                    {
                        var textPackage = new DataPackage();
                        textPackage.SetText(directMessage.Text);
                        Clipboard.SetContent(textPackage);
                    }
                    catch
                    {
                    }

                    return;
                }
            });

            Services.Notice.Instance.ShowUserProfileCommand.Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "UserProfile", Tokens = this._AccountModel._Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
            });

            Services.Notice.Instance.ShowConversationCommand.Where(_ => this._AccountModel.IsEnabled).Subscribe(x =>
            {
                var notification = new ShowSettingsFlyoutNotification() { SettingsFlyoutType = "Conversation", Tokens = this._AccountModel._Tokens, UserIcon = this.ProfileImageUrl.Value, Content = x };
                Services.Notice.Instance.ShowSettingsFlyoutCommand.Execute(notification);
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
            this.AdditionalColumnsName.Dispose();
            this.ColumnCount.Dispose();
            this.ColumnWidth.Dispose();
            this.PanelWidth.Dispose();
            this.SnapPointsSpaceing.Dispose();
            this.MaxSnapPoint.Dispose();
            this.MinSnapPoint.Dispose();
            this.ColumnSelectedIndex.Dispose();
        }
    }
}
