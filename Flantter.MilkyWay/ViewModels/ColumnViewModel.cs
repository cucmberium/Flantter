using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Reactive.Concurrency;
using Flantter.MilkyWay.ViewModels.Services;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using Windows.UI.Xaml.Input;
using System.Collections;

namespace Flantter.MilkyWay.ViewModels
{
    public class ColumnViewModel : IDisposable
    {
        protected CompositeDisposable Disposable { get; private set; } = new CompositeDisposable();

        public ColumnModel Model { get; set; }
        public Services.Notice Notice { get; set; }

        public ReactiveProperty<double> Height { get; private set; }
        public ReactiveProperty<double> Width { get; private set; }
        public ReactiveProperty<double> Left { get; private set; }

		public ReactiveProperty<Symbol> ActionSymbol { get; private set; }
		public ReactiveProperty<string> Name { get; private set; }
        public ReactiveProperty<string> ScreenName { get; private set; }
        public ReactiveProperty<bool> EnableCreateFilterColumn { get; private set; }
		public ReactiveProperty<Symbol> StreamingSymbol { get; private set; }
        public ReactiveProperty<bool> IsEnabledStreaming { get; private set; }
        public ReactiveProperty<bool> Updating { get; private set; }
        public ReactiveProperty<bool> IsEnabledMultipulSelect { get; private set; }
        public ReactiveProperty<bool> IsCollectionColumn { get; private set; }

        public ReadOnlyReactiveCollection<object> Tweets { get; private set; }

        public ReactiveProperty<int> Index { get; private set; }

        public ReactiveProperty<int> SelectedIndex { get; private set; }

        public ReactiveProperty<int> UnreadCount { get; private set; }
        public ReactiveProperty<bool> IsScrollLockToTopEnabled { get; private set; }
        public ReactiveProperty<bool> IsScrollLockEnabled { get; private set; }

        public ReactiveProperty<bool> CanDeleteColumn { get; private set; }
        
        public ReactiveProperty<bool> IsMultipulSelectOpened { get; private set; }
        public ReactiveProperty<ListViewSelectionMode> ListViewSelectionMode { get; private set; }
        public ReactiveProperty<IEnumerable> SelectedItemsList { get; private set; }

        public ReactiveCommand StreamingCommand { get; private set; }

        public ReactiveCommand ScrollToTopCommand { get; private set; }

        public ReactiveCommand IncrementalLoadCommand { get; private set; }

        public ReactiveCommand RefreshCommand { get; private set; }

        public ReactiveCommand TweetDoubleTappedActionCommand { get; private set; }

        public ReactiveCommand OpenStatusMultipulSelectCommand { get; private set; }

        public ReactiveCommand CloseStatusMultipulSelectCommand { get; private set; }

        public ReactiveCommand ClearColumnCommand { get; private set; }


        #region Constructor

        public ColumnViewModel(ColumnModel column)
        {
            this.Model = column;

            this.Notice = Services.Notice.Instance;

            this.ActionSymbol = column.ObserveProperty(x => x.Action).Select(x =>
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
					default:
						return Symbol.Help;
				}
			}).ToReactiveProperty().AddTo(this.Disposable);
			this.EnableCreateFilterColumn = column.ObserveProperty(x => x.Action).Select(x => x == SettingSupport.ColumnTypeEnum.Home).ToReactiveProperty().AddTo(this.Disposable);
            this.Name = column.ObserveProperty(x => x.Name).ToReactiveProperty().AddTo(this.Disposable);
            this.ScreenName = column.ObserveProperty(x => x.ScreenName).ToReactiveProperty().AddTo(this.Disposable);
            this.StreamingSymbol = column.ObserveProperty(x => x.Streaming).Select(x => x ? Symbol.Pause : Symbol.Play).ToReactiveProperty().AddTo(this.Disposable);
            this.Index = column.ObserveProperty(x => x.Index).ToReactiveProperty().AddTo(this.Disposable);
            this.IsEnabledStreaming = column.ObserveProperty(x => x.Action).Select(x =>
            {
                switch (x)
                {
                    case SettingSupport.ColumnTypeEnum.Search:
                        return true;
                    case SettingSupport.ColumnTypeEnum.List:
                        return true;
                    case SettingSupport.ColumnTypeEnum.Home:
                        return true;
                    default:
                        return false;
                }
            }).ToReactiveProperty().AddTo(this.Disposable);
            this.Updating = column.ObserveProperty(x => x.Updating).ToReactiveProperty().AddTo(this.Disposable);
            this.CanDeleteColumn = column.ObserveProperty(x => x.Action).Select(x => x == SettingSupport.ColumnTypeEnum.Filter || x == SettingSupport.ColumnTypeEnum.List || x == SettingSupport.ColumnTypeEnum.Search || x == SettingSupport.ColumnTypeEnum.UserTimeline || x == SettingSupport.ColumnTypeEnum.Collection).ToReactiveProperty().AddTo(this.Disposable);
            this.IsEnabledMultipulSelect = column.ObserveProperty(x => x.Action).Select(x => x != SettingSupport.ColumnTypeEnum.Events && x != SettingSupport.ColumnTypeEnum.DirectMessages).ToReactiveProperty().AddTo(this.Disposable);
            this.IsCollectionColumn = column.ObserveProperty(x => x.Action).Select(x => x == SettingSupport.ColumnTypeEnum.Collection).ToReactiveProperty().AddTo(this.Disposable);

            this.IsMultipulSelectOpened = new ReactiveProperty<bool>().AddTo(this.Disposable);
            this.ListViewSelectionMode = new ReactiveProperty<ListViewSelectionMode>(Windows.UI.Xaml.Controls.ListViewSelectionMode.Single).AddTo(this.Disposable);
            this.SelectedItemsList = new ReactiveProperty<IEnumerable>().AddTo(this.Disposable);

            this.SelectedIndex = column.ToReactivePropertyAsSynchronized(x => x.SelectedIndex).AddTo(this.Disposable);

            this.UnreadCount = column.ToReactivePropertyAsSynchronized(x => x.UnreadCount).AddTo(this.Disposable);
            this.IsScrollLockToTopEnabled = new ReactiveProperty<bool>().AddTo(this.Disposable);
            this.IsScrollLockEnabled = column.ToReactivePropertyAsSynchronized(x => x.IsScrollLockEnabled).AddTo(this.Disposable);

            this.StreamingCommand = column.ObserveProperty(x => x.Action).Select(x =>
            {
                switch (x)
                {
                    case SettingSupport.ColumnTypeEnum.Home:
                        return true;
                    case SettingSupport.ColumnTypeEnum.List:
                        return true;
                    case SettingSupport.ColumnTypeEnum.Search:
                        return true;
                    default:
                        return false;
                }
            }).ToReactiveCommand().AddTo(this.Disposable);
            this.StreamingCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async _ => 
            {
                this.Model.Streaming = !this.Model.Streaming;
                this.Model.ColumnSetting.Streaming = this.Model.Streaming;
                await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();
            }).AddTo(this.Disposable);

            this.ScrollToTopCommand = new ReactiveCommand().AddTo(this.Disposable);
            this.ScrollToTopCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async _ => 
            {
                this.IsScrollLockToTopEnabled.Value = true;
                await Task.Delay(100);
                this.IsScrollLockToTopEnabled.Value = false;
            }).AddTo(this.Disposable);

            this.IncrementalLoadCommand = new ReactiveCommand().AddTo(this.Disposable);
            this.IncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async _ => 
            {
                var id = this.Model.Tweets.Last().Id;
                var status = this.Model.Tweets.Last() as Status;
                if (status != null && status.HasRetweetInformation)
                    id = status.RetweetInformation.Id;

                await this.Model.Update(id);
            }).AddTo(this.Disposable);

            this.RefreshCommand = new ReactiveCommand().AddTo(this.Disposable);
            this.RefreshCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async _ =>
            {
                this.Model.IsScrollLockEnabled = true;
                await Task.Delay(50);
                await this.Model.Update();
                await Task.Delay(200);
                this.Model.IsScrollLockEnabled = false;
            }).AddTo(this.Disposable);

            this.TweetDoubleTappedActionCommand = new ReactiveCommand().AddTo(this.Disposable);
            this.TweetDoubleTappedActionCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(_ =>
            {
                if (this.SelectedIndex.Value == -1)
                    return;

                var tweet = this.Tweets[this.SelectedIndex.Value];

                string screenName = string.Empty;
                if (tweet is StatusViewModel)
                    screenName = ((StatusViewModel)tweet).ScreenName;
                if (tweet is DirectMessageViewModel)
                    screenName = ((DirectMessageViewModel)tweet).ScreenName;
                if (tweet is EventMessageViewModel)
                    screenName = ((EventMessageViewModel)tweet).ScreenName;

                var status = this.Tweets[this.SelectedIndex.Value] as StatusViewModel;

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
                    default:
                        break;
                }

            }).AddTo(this.Disposable);

            this.OpenStatusMultipulSelectCommand = new ReactiveCommand().AddTo(this.Disposable);
            this.OpenStatusMultipulSelectCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.ListViewSelectionMode.Value = Windows.UI.Xaml.Controls.ListViewSelectionMode.Multiple;
                this.IsMultipulSelectOpened.Value = true;
            }).AddTo(this.Disposable);

            this.CloseStatusMultipulSelectCommand = new ReactiveCommand().AddTo(this.Disposable);
            this.CloseStatusMultipulSelectCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                this.IsMultipulSelectOpened.Value = false;
                this.ListViewSelectionMode.Value = Windows.UI.Xaml.Controls.ListViewSelectionMode.Single;
            }).AddTo(this.Disposable);
            
            this.ClearColumnCommand = new ReactiveCommand().AddTo(this.Disposable);
            this.ClearColumnCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(y =>
            {
                this.Model.ClearColumn();
                this.RefreshCommand.Execute();
            }).AddTo(this.Disposable);

            this.Height = LayoutHelper.Instance.ColumnHeight;

            this.Width = LayoutHelper.Instance.ColumnWidth;

            this.Left = Observable.CombineLatest<int, double, double, double>(
                this.Index,
                LayoutHelper.Instance.ColumnWidth,
                WindowSizeHelper.Instance.ObserveProperty(x => x.WindowHeight),
                (index, columnWidth, winHeight) =>
                {
                    if (winHeight >= 500)
                    {
                        if (WindowSizeHelper.Instance.ClientWidth < 384.0)
                            return index * (columnWidth + 10.0) + 352.0;
                        else
                            return 5.0 + index * (columnWidth + 10.0) + 352.0;
                    }
                    else
                    {
                        return index * (columnWidth + 10.0) + 352.0;
                    }
                    
                }).ToReactiveProperty().AddTo(this.Disposable);
            
            this.Tweets = this.Model.Tweets.ToReadOnlyReactiveCollection(item => 
            {
                if (item is Status)
                    return new StatusViewModel((Status)item, this.Model.Tokens.UserId) as object;
                else if (item is DirectMessage)
                    return new DirectMessageViewModel((DirectMessage)item, this.Model.Tokens.UserId) as object;
                else if (item is EventMessage)
                    return new EventMessageViewModel((EventMessage)item, this.Model.Tokens.UserId) as object;
                else if (item is CollectionEntry)
                    return new StatusViewModel(((CollectionEntry)item).Status, this.Model.Tokens.UserId, this.Model.Parameter) as object;
                else
                    return new GapViewModel((Gap)item) as object;

            }).AddTo(this.Disposable);
        }
        #endregion

        #region Destructor
        ~ColumnViewModel()
        {
        }
        #endregion

        public void Dispose()
        {
            this.Disposable.Dispose();
        }
    }
}
