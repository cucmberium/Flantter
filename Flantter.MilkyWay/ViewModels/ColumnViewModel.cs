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

namespace Flantter.MilkyWay.ViewModels
{
    public class ColumnViewModel : IDisposable
    {
        public ColumnModel _ColumnModel { get; set; }

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

        public ExtendedObservableCollection<object> Tweets { get; private set; }

        public ReactiveProperty<int> Index { get; private set; }

        public ReactiveProperty<int> SelectedIndex { get; private set; }

        public ReactiveProperty<int> UnreadCount { get; private set; }
        public ReactiveProperty<bool> UnreadCountIncrementalTrigger { get; private set; }
        public ReactiveProperty<bool> IsScrollControlEnabled { get; private set; }
        public ReactiveProperty<bool> IsScrollLockEnabled { get; private set; }
        public ReactiveProperty<bool> IsScrollLockToTopEnabled { get; private set; }

        public ReactiveCommand StreamingCommand { get; private set; }

        public ReactiveCommand ScrollToTopCommand { get; private set; }

        public ReactiveCommand IncrementalLoadCommand { get; private set; }

        public ReactiveCommand RefreshCommand { get; private set; }

        public ReactiveCommand TweetDoubleTappedActionCommand { get; private set; }

        public IDisposable TweetsCollectionChangedDisposable { get; private set; }
        public IDisposable DisableNotifyCollectionChangedDisposable { get; private set; }

        #region Constructor
        /*public ColumnViewModel()
        {
        }*/

        public ColumnViewModel(ColumnModel column)
        {
            this._ColumnModel = column;
            
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
					default:
						return Symbol.Help;
				}
			}).ToReactiveProperty();
			this.EnableCreateFilterColumn = column.ObserveProperty(x => x.Action).Select(x => x == SettingSupport.ColumnTypeEnum.Home).ToReactiveProperty();
            this.Name = column.ObserveProperty(x => x.Name).ToReactiveProperty();
            this.ScreenName = column.ObserveProperty(x => x.ScreenName).ToReactiveProperty();
			this.StreamingSymbol = column.ObserveProperty(x => x.Streaming).Select(x => x ? Symbol.Pause : Symbol.Play).ToReactiveProperty();
            this.Index = column.ObserveProperty(x => x.Index).ToReactiveProperty();
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
            }).ToReactiveProperty();
            this.Updating = column.ObserveProperty(x => x.Updating).ToReactiveProperty();

            this.SelectedIndex = column.ToReactivePropertyAsSynchronized(x => x.SelectedIndex);

            this.UnreadCount = column.ToReactivePropertyAsSynchronized(x => x.UnreadCount);
            this.UnreadCountIncrementalTrigger = column.ToReactivePropertyAsSynchronized(x => x.UnreadCountIncrementalTrigger);
            this.IsScrollControlEnabled = column.ObserveProperty(x => x.IsScrollControlEnabled).ToReactiveProperty();
            this.IsScrollLockEnabled = column.ObserveProperty(x => x.IsScrollLockEnabled).ToReactiveProperty();
            this.IsScrollLockToTopEnabled = new ReactiveProperty<bool>();

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
            }).ToReactiveCommand();
            this.StreamingCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(_ => { this._ColumnModel.Streaming = !this._ColumnModel.Streaming; });

            this.ScrollToTopCommand = column.ObserveProperty(x => x.UnreadCount).Select(x => x != 0).ToReactiveCommand();
            this.ScrollToTopCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async _ => 
            {
                this.IsScrollLockToTopEnabled.Value = true;
                await Task.Delay(100);
                this.IsScrollLockToTopEnabled.Value = false;
            });

            this.IncrementalLoadCommand = new ReactiveCommand();
            this.IncrementalLoadCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async _ => 
            {
                var id = this._ColumnModel.Tweets.Last().Id;
                var status = this._ColumnModel.Tweets.Last() as Status;
                if (status != null && status.HasRetweetInformation)
                    id = status.RetweetInformation.Id;
                
                await this._ColumnModel.Update(id);
            });

            this.RefreshCommand = new ReactiveCommand();
            this.RefreshCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async _ =>
            {
                await this._ColumnModel.Update();
            });

            this.TweetDoubleTappedActionCommand = new ReactiveCommand();
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

            });

            this.Height = LayoutHelper.Instance.ColumnHeight;

            this.Width = LayoutHelper.Instance.ColumnWidth;

            this.Left = Observable.CombineLatest<double, int, double, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                this.Index,
                LayoutHelper.Instance.ColumnWidth,
                (width, index, columnWidth) =>
                {
                    if (width < 352.0)
                        return index * (columnWidth + 10.0) + 352.0;
                    else
                        return 5.0 + index * (columnWidth + 10.0) + 352.0;
                }).ToReactiveProperty();
            
            this.Tweets = new ExtendedObservableCollection<object>();
            this.TweetsCollectionChangedDisposable = Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => (sender, e) => h(e),
                h => this._ColumnModel.Tweets.CollectionChanged += h,
                h => this._ColumnModel.Tweets.CollectionChanged -= h).SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x => 
                {
                    var e = x as NotifyCollectionChangedEventArgs;

                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        var item = e.NewItems[0];

                        if (item is Status)
                            this.Tweets.Insert(e.NewStartingIndex, new StatusViewModel((Status)item, this._ColumnModel.Tokens.UserId));
                        else if (item is DirectMessage)
                            this.Tweets.Insert(e.NewStartingIndex, new DirectMessageViewModel((DirectMessage)item, this._ColumnModel.Tokens.UserId));
                        else if (item is EventMessage)
                            this.Tweets.Insert(e.NewStartingIndex, new EventMessageViewModel((EventMessage)item, this._ColumnModel.Tokens.UserId));
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        var item = this.Tweets[e.OldStartingIndex] as IDisposable;
                        if (item != null)
                            item.Dispose();

                        this.Tweets.RemoveAt(e.OldStartingIndex);
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Replace)
                    {
                        var olditem = this.Tweets[e.OldStartingIndex] as IDisposable;
                        if (olditem != null)
                            olditem.Dispose();

                        var item = e.NewItems[0];
                        if (item is Status)
                            this.Tweets.Insert(e.NewStartingIndex, new StatusViewModel((Status)item, this._ColumnModel.Tokens.UserId));
                        else if (item is DirectMessage)
                            this.Tweets.Insert(e.NewStartingIndex, new DirectMessageViewModel((DirectMessage)item, this._ColumnModel.Tokens.UserId));
                        else if (item is EventMessage)
                            this.Tweets.Insert(e.NewStartingIndex, new EventMessageViewModel((EventMessage)item, this._ColumnModel.Tokens.UserId));
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        foreach (var tweet in this.Tweets)
                        {
                            var item = tweet as IDisposable;
                            if (item != null)
                                item.Dispose();
                        }

                        this.Tweets.Clear();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                });

            this.DisableNotifyCollectionChangedDisposable = this._ColumnModel.ObserveProperty(x => x.DisableNotifyCollectionChanged).SubscribeOn(ThreadPoolScheduler.Default).Subscribe<bool>(x => 
            {
                this.Tweets.DisableNotifyCollectionChanged = x;
                if (!x)
                    this.Tweets.InvokeCollectionChanged(NotifyCollectionChangedAction.Reset);
            });
        }
        #endregion

        #region Destructor
        ~ColumnViewModel()
        {
        }
        #endregion

        public void Dispose()
        {
            this.ActionSymbol.Dispose();
            this.EnableCreateFilterColumn.Dispose();
            this.Name.Dispose();
            this.ScreenName.Dispose();
            this.StreamingSymbol.Dispose();
            this.Index.Dispose();
            this.IsEnabledStreaming.Dispose();
            this.Updating.Dispose();
            this.SelectedIndex.Dispose();
            this.UnreadCount.Dispose();
            this.UnreadCountIncrementalTrigger.Dispose();
            this.IsScrollControlEnabled.Dispose();
            this.IsScrollLockEnabled.Dispose();
            this.IsScrollLockToTopEnabled.Dispose();
            this.StreamingCommand.Dispose();
            this.ScrollToTopCommand.Dispose();
            this.TweetDoubleTappedActionCommand.Dispose();

            this.Left.Dispose();

            this.TweetsCollectionChangedDisposable.Dispose();
            this.DisableNotifyCollectionChangedDisposable.Dispose();
        }
    }
}
