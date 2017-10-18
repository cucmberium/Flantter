using System;
using System.Collections;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Apis.Objects;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels
{
    public class ColumnViewModel : IDisposable
    {
        #region Constructor

        public ColumnViewModel(ColumnModel column)
        {
            Model = column;

            Notice = Notice.Instance;

            ActionSymbol = column.ObserveProperty(x => x.Action)
                .Select(x =>
                {
                    switch (x)
                    {
                        case SettingSupport.ColumnTypeEnum.Home:
                            return Symbol.Home;
                        case SettingSupport.ColumnTypeEnum.Mentions:
                            return Symbol.Account;
                        case SettingSupport.ColumnTypeEnum.DirectMessages:
                            return Symbol.Mail;
                        case SettingSupport.ColumnTypeEnum.Favorites:
                            return Symbol.Favorite;
                        case SettingSupport.ColumnTypeEnum.Events:
                            return Symbol.Important;
                        case SettingSupport.ColumnTypeEnum.Search:
                            return Symbol.Find;
                        case SettingSupport.ColumnTypeEnum.List:
                            return Symbol.Bullets;
                        case SettingSupport.ColumnTypeEnum.UserTimeline:
                            return Symbol.Contact;
                        case SettingSupport.ColumnTypeEnum.Filter:
                            return Symbol.Repair;
                        case SettingSupport.ColumnTypeEnum.Collection:
                            return Symbol.SlideShow;
                        case SettingSupport.ColumnTypeEnum.Federated:
                            return Symbol.World;
                        case SettingSupport.ColumnTypeEnum.Local:
                            return Symbol.Street;
                        default:
                            return Symbol.Help;
                    }
                })
                .ToReactiveProperty()
                .AddTo(Disposable);
            EnableCreateFilterColumn = column.ObserveProperty(x => x.Action)
                .Select(x => x == SettingSupport.ColumnTypeEnum.Home)
                .ToReactiveProperty()
                .AddTo(Disposable);
            Name = column.ObserveProperty(x => x.Name).ToReactiveProperty().AddTo(Disposable);
            AccountName = Observable.CombineLatest(
                    column.ObserveProperty(x => x.ScreenName),
                    column.ObserveProperty(x => x.Instance),
                    (screenName, instance) => string.IsNullOrWhiteSpace(instance)
                        ? screenName
                        : screenName + "@" + instance
                )
                .ToReactiveProperty()
                .AddTo(Disposable);
            StreamingSymbol = column.ObserveProperty(x => x.Streaming)
                .Select(x => x ? Symbol.Pause : Symbol.Play)
                .ToReactiveProperty()
                .AddTo(Disposable);
            Index = column.ObserveProperty(x => x.Index).ToReactiveProperty().AddTo(Disposable);
            IsEnabledStreaming = column.ObserveProperty(x => x.Action)
                .Select(x =>
                {
                    switch (x)
                    {
                        case SettingSupport.ColumnTypeEnum.Search:
                            return true;
                        case SettingSupport.ColumnTypeEnum.List:
                            return true;
                        case SettingSupport.ColumnTypeEnum.Home:
                            return true;
                        case SettingSupport.ColumnTypeEnum.Federated:
                            return true;
                        case SettingSupport.ColumnTypeEnum.Local:
                            return true;
                        default:
                            return false;
                    }
                })
                .ToReactiveProperty()
                .AddTo(Disposable);
            Updating = column.ObserveProperty(x => x.Updating).ToReactiveProperty().AddTo(Disposable);
            CanDeleteColumn = column.ObserveProperty(x => x.Action)
                .Select(x => x == SettingSupport.ColumnTypeEnum.Filter ||
                             x == SettingSupport.ColumnTypeEnum.List ||
                             x == SettingSupport.ColumnTypeEnum.Search ||
                             x == SettingSupport.ColumnTypeEnum.UserTimeline ||
                             x == SettingSupport.ColumnTypeEnum.Collection ||
                             x == SettingSupport.ColumnTypeEnum.Federated ||
                             x == SettingSupport.ColumnTypeEnum.Local)
                .ToReactiveProperty()
                .AddTo(Disposable);
            IsEnabledMultipulSelect = column.ObserveProperty(x => x.Action)
                .Select(x => x != SettingSupport.ColumnTypeEnum.Events &&
                             x != SettingSupport.ColumnTypeEnum.DirectMessages)
                .ToReactiveProperty()
                .AddTo(Disposable);
            IsCollectionColumn = column.ObserveProperty(x => x.Action)
                .Select(x => x == SettingSupport.ColumnTypeEnum.Collection)
                .ToReactiveProperty()
                .AddTo(Disposable);

            IsMultipulSelectOpened = new ReactiveProperty<bool>().AddTo(Disposable);
            ListViewSelectionMode =
                new ReactiveProperty<ListViewSelectionMode>(Windows.UI.Xaml.Controls.ListViewSelectionMode.Single)
                    .AddTo(Disposable);
            SelectedItemsList = new ReactiveProperty<IEnumerable>().AddTo(Disposable);

            SelectedIndex = column.ToReactivePropertyAsSynchronized(x => x.SelectedIndex).AddTo(Disposable);

            UnreadCount = column.ToReactivePropertyAsSynchronized(x => x.UnreadCount).AddTo(Disposable);
            UnreadButtonVisibility = column.ObserveProperty(x => x.UnreadCount).Select(x => x >= 0).ToReactiveProperty()
                .AddTo(Disposable);
            IsScrollLockToTopEnabled = new ReactiveProperty<bool>().AddTo(Disposable);
            IsScrollLockEnabled = column.ToReactivePropertyAsSynchronized(x => x.IsScrollLockEnabled).AddTo(Disposable);

            StreamingCommand = column.ObserveProperty(x => x.Action)
                .Select(x =>
                {
                    switch (x)
                    {
                        case SettingSupport.ColumnTypeEnum.Home:
                            return true;
                        case SettingSupport.ColumnTypeEnum.List:
                            return true;
                        case SettingSupport.ColumnTypeEnum.Search:
                            return true;
                        case SettingSupport.ColumnTypeEnum.Federated:
                            return true;
                        case SettingSupport.ColumnTypeEnum.Local:
                            return true;
                        default:
                            return false;
                    }
                })
                .ToReactiveCommand()
                .AddTo(Disposable);
            StreamingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async _ =>
                {
                    Model.Streaming = !Model.Streaming;
                    Model.ColumnSetting.Streaming = Model.Streaming;
                    await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
                })
                .AddTo(Disposable);

            ScrollToTopCommand = new ReactiveCommand().AddTo(Disposable);
            ScrollToTopCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async _ =>
                {
                    IsScrollLockToTopEnabled.Value = true;
                    await Task.Delay(100);
                    IsScrollLockToTopEnabled.Value = false;
                })
                .AddTo(Disposable);

            IncrementalLoadCommand = new ReactiveCommand().AddTo(Disposable);
            IncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async _ =>
                {
                    var id = Model.Tweets.Last().Id;
                    if (Model.Tweets.Last() is Status status && status.HasRetweetInformation)
                        id = status.RetweetInformation.Id;

                    await Model.Update(id);
                })
                .AddTo(Disposable);

            RefreshCommand = new ReactiveCommand().AddTo(Disposable);
            RefreshCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async _ =>
                {
                    Model.IsScrollLockEnabled = true;
                    await Task.Delay(50);
                    await Model.Update();
                    await Task.Delay(200);
                    Model.IsScrollLockEnabled = false;
                })
                .AddTo(Disposable);

            TweetDoubleTappedActionCommand = new ReactiveCommand().AddTo(Disposable);
            TweetDoubleTappedActionCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(_ =>
                {
                    if (SelectedIndex.Value == -1)
                        return;

                    var tweet = Tweets[SelectedIndex.Value];

                    var screenName = string.Empty;
                    if (tweet is StatusViewModel statusViewModel)
                        screenName = statusViewModel.ScreenName;
                    if (tweet is DirectMessageViewModel directMessageViewModel)
                        screenName = directMessageViewModel.ScreenName;
                    if (tweet is EventMessageViewModel eventMessageViewModel)
                        screenName = eventMessageViewModel.ScreenName;

                    var status = Tweets[SelectedIndex.Value] as StatusViewModel;

                    switch (SettingService.Setting.DoubleTappedAction)
                    {
                        case SettingSupport.DoubleTappedActionEnum.None:
                            break;
                        case SettingSupport.DoubleTappedActionEnum.StatusDetail:
                            if (status == null)
                                break;

                            Notice.Instance.ShowStatusDetailCommand.Execute(status.Model);
                            break;
                        case SettingSupport.DoubleTappedActionEnum.UserProfile:
                            if (string.IsNullOrWhiteSpace(screenName))
                                break;

                            Notice.Instance.ShowUserProfileCommand.Execute(screenName);
                            break;
                        case SettingSupport.DoubleTappedActionEnum.Favorite:
                            if (status == null)
                                break;

                            Notice.Instance.FavoriteCommand.Execute(status);
                            break;
                        case SettingSupport.DoubleTappedActionEnum.Reply:
                            if (status != null)
                                Notice.Instance.ReplyCommand.Execute(status);
                            else if (string.IsNullOrWhiteSpace(screenName))
                                Notice.Instance.ReplyCommand.Execute(screenName);

                            break;
                        case SettingSupport.DoubleTappedActionEnum.Retweet:
                            if (status == null)
                                break;

                            Notice.Instance.RetweetCommand.Execute(status);
                            break;
                    }
                })
                .AddTo(Disposable);

            OpenStatusMultipulSelectCommand = new ReactiveCommand().AddTo(Disposable);
            OpenStatusMultipulSelectCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    ListViewSelectionMode.Value = Windows.UI.Xaml.Controls.ListViewSelectionMode.Multiple;
                    IsMultipulSelectOpened.Value = true;
                })
                .AddTo(Disposable);

            CloseStatusMultipulSelectCommand = new ReactiveCommand().AddTo(Disposable);
            CloseStatusMultipulSelectCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    IsMultipulSelectOpened.Value = false;
                    ListViewSelectionMode.Value = Windows.UI.Xaml.Controls.ListViewSelectionMode.Single;
                })
                .AddTo(Disposable);

            ClearColumnCommand = new ReactiveCommand().AddTo(Disposable);
            ClearColumnCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(y =>
                {
                    Model.ClearColumn();
                    RefreshCommand.Execute();
                })
                .AddTo(Disposable);

            Height = LayoutHelper.Instance.ColumnHeight;

            Width = LayoutHelper.Instance.ColumnWidth;

            Left = Index.CombineLatest(LayoutHelper.Instance.ColumnWidth,
                    WindowSizeHelper.Instance.ObserveProperty(x => x.WindowHeight),
                    (index, columnWidth, winHeight) =>
                    {
                        if (winHeight >= 500)
                            if (WindowSizeHelper.Instance.ClientWidth < 384.0)
                                return index * (columnWidth + 10.0) + 352.0;
                            else
                                return 5.0 + index * (columnWidth + 10.0) + 352.0;
                        return index * (columnWidth + 10.0) + 352.0;
                    })
                .ToReactiveProperty()
                .AddTo(Disposable);

            Tweets = Model.Tweets.ToReadOnlyReactiveCollection(item =>
                {
                    if (item is Status status)
                        return new StatusViewModel(status, Model.AccountSetting.UserId) as object;
                    if (item is DirectMessage directMessage)
                        return new DirectMessageViewModel(directMessage, Model.AccountSetting.UserId) as object;
                    if (item is EventMessage eventMessage)
                        return new EventMessageViewModel(eventMessage, Model.AccountSetting.UserId) as object;
                    if (item is CollectionEntry collectionEntry)
                        return new StatusViewModel(collectionEntry.Status, Model.AccountSetting.UserId,
                            Model.Parameter) as object;
                    return new GapViewModel((Gap) item) as object;
                })
                .AddTo(Disposable);
        }

        #endregion

        protected CompositeDisposable Disposable { get; } = new CompositeDisposable();

        public ColumnModel Model { get; set; }
        public Notice Notice { get; set; }

        public ReactiveProperty<double> Height { get; }
        public ReactiveProperty<double> Width { get; }
        public ReactiveProperty<double> Left { get; }

        public ReactiveProperty<Symbol> ActionSymbol { get; }
        public ReactiveProperty<string> Name { get; }
        public ReactiveProperty<string> AccountName { get; }
        public ReactiveProperty<bool> EnableCreateFilterColumn { get; }
        public ReactiveProperty<Symbol> StreamingSymbol { get; }
        public ReactiveProperty<bool> IsEnabledStreaming { get; }
        public ReactiveProperty<bool> Updating { get; }
        public ReactiveProperty<bool> IsEnabledMultipulSelect { get; }
        public ReactiveProperty<bool> IsCollectionColumn { get; }

        public ReadOnlyReactiveCollection<object> Tweets { get; }

        public ReactiveProperty<int> Index { get; }

        public ReactiveProperty<int> SelectedIndex { get; }

        public ReactiveProperty<int> UnreadCount { get; }
        public ReactiveProperty<bool> UnreadButtonVisibility { get; }
        public ReactiveProperty<bool> IsScrollLockToTopEnabled { get; }
        public ReactiveProperty<bool> IsScrollLockEnabled { get; }

        public ReactiveProperty<bool> CanDeleteColumn { get; }

        public ReactiveProperty<bool> IsMultipulSelectOpened { get; }
        public ReactiveProperty<ListViewSelectionMode> ListViewSelectionMode { get; }
        public ReactiveProperty<IEnumerable> SelectedItemsList { get; }

        public ReactiveCommand StreamingCommand { get; }

        public ReactiveCommand ScrollToTopCommand { get; }

        public ReactiveCommand IncrementalLoadCommand { get; }

        public ReactiveCommand RefreshCommand { get; }

        public ReactiveCommand TweetDoubleTappedActionCommand { get; }

        public ReactiveCommand OpenStatusMultipulSelectCommand { get; }

        public ReactiveCommand CloseStatusMultipulSelectCommand { get; }

        public ReactiveCommand ClearColumnCommand { get; }

        public void Dispose()
        {
            Disposable.Dispose();
        }
    }
}