using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Text;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Flantter.MilkyWay.ViewModels
{
    public class ColumnViewModel
    {
        private ColumnModel _ColumnModel { get; set; }

        public ReactiveProperty<double> Height { get; private set; }
        public ReactiveProperty<double> Width { get; private set; }
        public ReactiveProperty<double> Left { get; private set; }
        public IObservable<int> ColumnCount { get; set; }

		public ReactiveProperty<Symbol> ActionSymbol { get; private set; }
		public ReactiveProperty<string> Name { get; private set; }
        public ReactiveProperty<string> OwnerScreenName { get; private set; }
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
            this.OwnerScreenName = column.ObserveProperty(x => x.OwnerScreenName).ToReactiveProperty();
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
            this.IsScrollLockToTopEnabled = column.ObserveProperty(x => x.IsScrollLockToTopEnabled).ToReactiveProperty();

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
            this.StreamingCommand.Subscribe(_ => { this._ColumnModel.Streaming = !this._ColumnModel.Streaming; });

            this.ColumnCount = Observable.CombineLatest<double, double, int, int>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                SettingService.Setting.ObserveProperty(x => x.MinColumnSize),
                SettingService.Setting.ObserveProperty(x => x.MaxColumnCount),
                (width, minWidth, maxCount) =>
                {
                    return (int)Math.Max(Math.Min(maxCount, (width - 5.0 * 2) / (minWidth + 5.0 * 2)), 1.0);
                }).ToReactiveProperty();

            this.Height = Observable.CombineLatest<double, double, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientHeight),
                (width, height) =>
                {
                    var retheight = 0.0;
                    if (width < 352.0)
                        retheight = height - 64.0;
                    else if (width < 500.0)
                        retheight = height - 64.0 - 20.0;
                    else
                        retheight = height - 75.0 - 20.0;

                    return retheight;
                }).ToReactiveProperty();

            this.Width = Observable.CombineLatest<double, int, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                this.ColumnCount,
                (width, count) =>
                {
                    if (width < 352.0)
                        return width;
                    else
                        return (width - 5.0 * 2) / count - 10.0;
                }).ToReactiveProperty();

            this.Left = Observable.CombineLatest<double, int, double, double>(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth),
                this.Index,
                this.Width,
                (width, index, columnWidth) =>
                {
                    if (width < 352.0)
                        return index * (columnWidth + 10.0) + 352.0;
                    else
                        return 5.0 + index * (columnWidth + 10.0) + 352.0;
                }).ToReactiveProperty();

            this.Tweets = new ExtendedObservableCollection<object>();
            Observable.FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                h => (sender, e) => h(e),
                h => this._ColumnModel.Tweets.CollectionChanged += h,
                h => this._ColumnModel.Tweets.CollectionChanged -= h).Subscribe(x => 
                {
                    var e = x as NotifyCollectionChangedEventArgs;

                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        if (e.NewItems.Count > 1)
                            throw new NotImplementedException();

                        var item = e.NewItems[0];

                        if (item is Status)
                            this.Tweets.Insert(e.NewStartingIndex, new StatusViewModel((Status)item, this._ColumnModel));
                        else if (item is DirectMessage)
                            this.Tweets.Insert(e.NewStartingIndex, new DirectMessageViewModel((DirectMessage)item, this._ColumnModel));
                        else if (item is EventMessage)
                            this.Tweets.Insert(e.NewStartingIndex, new EventMessageViewModel((EventMessage)item, this._ColumnModel));
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        if (e.OldItems.Count > 1)
                            throw new NotImplementedException();

                        var item = this.Tweets[e.OldStartingIndex] as IDisposable;
                        if (item != null)
                            item.Dispose();

                        this.Tweets.RemoveAt(e.OldStartingIndex);
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Replace)
                    {
                        if (e.OldItems.Count > 1 || e.NewItems.Count > 1)
                            throw new NotImplementedException();

                        var olditem = this.Tweets[e.OldStartingIndex] as IDisposable;
                        if (olditem != null)
                            olditem.Dispose();

                        var item = e.NewItems[0];
                        if (item is Status)
                            this.Tweets.Insert(e.NewStartingIndex, new StatusViewModel((Status)item, this._ColumnModel));
                        else if (item is DirectMessage)
                            this.Tweets.Insert(e.NewStartingIndex, new DirectMessageViewModel((DirectMessage)item, this._ColumnModel));
                        else if (item is EventMessage)
                            this.Tweets.Insert(e.NewStartingIndex, new EventMessageViewModel((EventMessage)item, this._ColumnModel));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                });

            this._ColumnModel.ObserveProperty(x => x.DisableNotifyCollectionChanged).Subscribe<bool>(x => 
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
    }
}
