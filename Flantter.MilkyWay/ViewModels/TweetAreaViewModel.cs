using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.ViewModels
{
    public class TweetAreaViewModel
    {
        public TweetAreaViewModel(ReadOnlyReactiveCollection<AccountViewModel> accounts)
        {
            Model = new TweetAreaModel();

            Accounts = accounts;
            SelectedAccounts = Accounts.ObserveElementObservableProperty(x => x.IsTweetEnabled)
                .Select(y => accounts.Where(z => z.IsTweetEnabled.Value)).ToReactiveProperty();

            SelectionStart = Model.ToReactivePropertyAsSynchronized(x => x.SelectionStart);
            Text = Model.ToReactivePropertyAsSynchronized(x => x.Text);
            CharacterCount = Model.ObserveProperty(x => x.CharacterCount)
                .Select(x => x.ToString())
                .ToReactiveProperty();

            Message = Model.ObserveProperty(x => x.Message).ToReactiveProperty();
            ToolTipIsOpen = Model.ToReactivePropertyAsSynchronized(x => x.ToolTipIsOpen);

            LockingHashTagsSymbol = Model.ObserveProperty(x => x.LockingHashTags)
                .Select(x => x ? Symbol.UnPin : Symbol.Pin)
                .ToReactiveProperty();

            StateSymbol = Model.ObserveProperty(x => x.State)
                .Select(x =>
                {
                    switch (x)
                    {
                        case "Accept":
                            return Symbol.Accept;
                        case "Cancel":
                            return Symbol.Cancel;
                        default:
                            return Symbol.Accept;
                    }
                })
                .ToReactiveProperty();

            Updating = Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            ReplyOrQuotedStatus = new ReactiveProperty<StatusViewModel>();
            IsQuotedRetweet = Model.ObserveProperty(x => x.IsQuotedRetweet).ToReactiveProperty();
            IsReply = Model.ObserveProperty(x => x.IsReply).ToReactiveProperty();

            IsContentWarning = Model.ToReactivePropertyAsSynchronized(x => x.IsContentWarning);
            ContentWarningText = Model.ToReactivePropertyAsSynchronized(x => x.ContentWarningText);

            Notice = Notice.Instance;
            Setting = SettingService.Setting;

            SuggestionMessenger = new Messenger();
            Model.SuggestionMessenger = SuggestionMessenger;
            TextBoxFocusMessenger = new Messenger();

            Pictures = Model.ReadonlyPictures.ToReadOnlyReactiveCollection(x => new PictureViewModel(x));

            AccountImageSize = Observable.Merge(
                WindowSizeHelper.Instance.ObserveProperty(x => x.ClientWidth).Select(x => (object)null),
                SelectedAccounts.Select(x => (object)null)
                ).Select(_ =>
                {
                    if (WindowSizeHelper.Instance.ClientWidth <= 700)
                    {
                        if (SelectedAccounts.Value.Count() < 2)
                            return 40.0;
                        else
                            return 20.0;
                    }
                    else
                    {
                        if (SelectedAccounts.Value.Count() < 2)
                            return 60.0;
                        else
                            return 30.0;
                    }
                }
                ).ToReactiveProperty();

            Model.ObserveProperty(x => x.ReplyOrQuotedStatus)
                .Subscribe(x =>
                {
                    var status = x;
                    if (status == null)
                        ReplyOrQuotedStatus.Value = null;
                });

            MessageShowCommand = new ReactiveCommand();
            MessageShowCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x => { Model.ToolTipIsOpen = !Model.ToolTipIsOpen; });

            ChangeLockHashTagsCommand = new ReactiveCommand();
            ChangeLockHashTagsCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x => { Model.LockingHashTags = !Model.LockingHashTags; });

            OpenAccountSettingCommand = new ReactiveCommand();
            OpenAccountSettingCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    if (SelectedAccounts.Value.Count() == 1)
                        Notice.Instance.ShowAccountSettingCommand.Execute(SelectedAccounts.Value.First().Model.AccountSetting);
                    else
                        Notice.Instance.ShowAccountsSettingCommand.Execute();
                });
            
            AddPictureCommand = new ReactiveCommand();
            AddPictureCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var result = await Notice.ShowFilePickerMessenger.Raise(new FileOpenPickerNotification
                    {
                        FileTypeFilter = new[] {".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov"},
                        IsMultiple = true
                    });

                    Notice.Instance.TweetAreaOpenCommand.Execute(true);

                    foreach (var pic in result.Result)
                        await Model.AddPicture(pic);
                });

            TweetCommand = Model.ObserveProperty(x => x.CharacterCount).Select(x => x >= 0).ToReactiveCommand();
            TweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var result = await Model.Tweet(SelectedAccounts.Value.Select(y => y.Model));

                    if (SettingService.Setting.CloseAppBarAfterTweet && result)
                    {
                        await Task.Delay(50);
                        Notice.Instance.TweetAreaOpenCommand.Execute(false);
                    }
                    else
                    {
                        await Task.Delay(50);
                        await TextBoxFocusMessenger.Raise(new Notification());
                    }

                    if (SettingService.Setting.RefreshTimelineAfterTweet)
                    {
                        foreach (var account in accounts)
                        {
                            var column =
                                account.Columns.First(
                                    y => y.Model.Action == SettingSupport.ColumnTypeEnum.Home);
                            if (!column.Model.Streaming)
                                column.RefreshCommand.Execute();
                        }
                    }
                });

            SuggestSelectedCommand = new ReactiveCommand();
            SuggestSelectedCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x => { Model.SuggestionSelected((string) x); });

            DeleteReplyOrQuotedStatusCommand = new ReactiveCommand();
            DeleteReplyOrQuotedStatusCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    ReplyOrQuotedStatus.Value = null;

                    Model.IsQuotedRetweet = false;
                    Model.IsReply = false;
                    Model.ReplyOrQuotedStatus = null;

                    Model.Text = string.Empty;

                    await Task.Delay(50);
                    await TextBoxFocusMessenger.Raise(new Notification());
                });

            PasteClipbordPictureCommand = new ReactiveCommand();
            PasteClipbordPictureCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x => { await Model.AddPictureFromClipboard(); });

            Notice.Instance.TweetAreaDeletePictureCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(x =>
                {
                    var pictureViewModel = x as PictureViewModel;
                    Model.DeletePicture(pictureViewModel.PictureModel);
                });

            Notice.Instance.ReplyCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var statusViewModel = x as StatusViewModel;
                    var screenName = x as string;
                    if (statusViewModel != null)
                    {
                        ReplyOrQuotedStatus.Value = statusViewModel;

                        Model.IsQuotedRetweet = false;
                        Model.IsReply = true;
                        Model.ReplyOrQuotedStatus = statusViewModel.Model;
                        
                        Model.ContentWarningText = "";

                        Model.Text = "@" + statusViewModel.Model.User.ScreenName + " ";

                        Notice.Instance.TweetAreaOpenCommand.Execute(true);

                        await Task.Delay(50);

                        Model.SelectionStart = Model.Text.Length;
                    }
                    else if (!string.IsNullOrWhiteSpace(screenName))
                    {
                        Model.ContentWarningText = "";

                        Model.Text = "@" + screenName + " ";

                        Notice.Instance.TweetAreaOpenCommand.Execute(true);

                        await Task.Delay(50);

                        Model.SelectionStart = Model.Text.Length;
                    }

                    if (Setting.ResetPostingAccountBeforeTweetAreaOpening)
                        foreach (var account in Accounts)
                            account.IsTweetEnabled.Value = account.Model.IsEnabled;
                });

            Notice.Instance.ReplyToAllCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var statusViewModel = x as StatusViewModel;
                    if (statusViewModel == null)
                        return;

                    ReplyOrQuotedStatus.Value = statusViewModel;

                    Model.IsQuotedRetweet = false;
                    Model.IsReply = true;
                    Model.ReplyOrQuotedStatus = statusViewModel.Model;

                    Model.ContentWarningText = "";

                    var userList = new List<string>();

                    Model.Text = "@" + statusViewModel.Model.User.ScreenName + " ";
                    userList.Add(statusViewModel.Model.User.ScreenName);

                    if (statusViewModel.Model.Entities?.UserMentions != null)
                    {
                        foreach (var user in statusViewModel.Model.Entities.UserMentions)
                        {
                            if (userList.Contains(user.ScreenName) ||
                                SelectedAccounts.Value.Select(y => y.ScreenName.Value).Contains(user.ScreenName))
                                continue;

                            Model.Text += "@" + user.ScreenName + " ";
                            userList.Add(user.ScreenName);
                        }
                    }

                    Notice.Instance.TweetAreaOpenCommand.Execute(true);

                    await Task.Delay(50);

                    Model.SelectionStart = Model.Text.Length;
                });

            Notice.Instance.ReplyToStatusesCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var items = x as IEnumerable;
                    if (items == null)
                        return;

                    var statusViewModels = items.Cast<StatusViewModel>();
                    if (!statusViewModels.Any())
                        return;

                    var statusViewModel = statusViewModels.First();

                    ReplyOrQuotedStatus.Value = statusViewModels.First();

                    Model.IsQuotedRetweet = false;
                    Model.IsReply = true;
                    Model.ReplyOrQuotedStatus = statusViewModel.Model;

                    Model.ContentWarningText = "";

                    var userList = new List<string>();
                    foreach (var sVm in statusViewModels)
                    {
                        if (userList.Contains(sVm.ScreenName) ||
                            SelectedAccounts.Value.Select(y => y.ScreenName.Value).Contains(sVm.ScreenName))
                            continue;

                        userList.Add(sVm.Model.User.ScreenName);

                        foreach (var user in sVm.Model.Entities.UserMentions)
                        {
                            if (userList.Contains(user.ScreenName) ||
                                SelectedAccounts.Value.Select(y => y.ScreenName.Value).Contains(user.ScreenName))
                                continue;

                            Model.Text += "@" + user.ScreenName + " ";
                            userList.Add(user.ScreenName);
                        }
                    }

                    Model.Text = string.Join(" ", userList.Select(screenName => "@" + screenName)) + " ";

                    Notice.Instance.TweetAreaOpenCommand.Execute(true);

                    await Task.Delay(50);

                    Model.SelectionStart = Model.Text.Length;
                });

            Notice.Instance.UrlQuoteRetweetCommand.SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(async x =>
                {
                    var statusViewModel = x as StatusViewModel;
                    if (statusViewModel == null)
                        return;

                    ReplyOrQuotedStatus.Value = statusViewModel;

                    Model.IsQuotedRetweet = true;
                    Model.IsReply = false;
                    Model.ReplyOrQuotedStatus = statusViewModel.Model;

                    Model.ContentWarningText = "";

                    Model.Text = "";

                    Notice.Instance.TweetAreaOpenCommand.Execute(true);

                    await Task.Delay(50);

                    Model.SelectionStart = 0;
                });
        }

        public TweetAreaModel Model { get; set; }
        public Notice Notice { get; set; }
        public SettingService Setting { get; set; }

        public ReadOnlyReactiveCollection<AccountViewModel> Accounts { get; }
        public ReactiveProperty<IEnumerable<AccountViewModel>> SelectedAccounts { get; }

        public ReadOnlyReactiveCollection<PictureViewModel> Pictures { get; }
        
        public ReactiveProperty<StatusViewModel> ReplyOrQuotedStatus { get; set; }
        public ReactiveProperty<bool> IsQuotedRetweet { get; set; }
        public ReactiveProperty<bool> IsReply { get; set; }

        public ReactiveProperty<string> ContentWarningText { get; set; }
        public ReactiveProperty<bool?> IsContentWarning { get; set; }

        public ReactiveProperty<string> Text { get; set; }
        public ReactiveProperty<int> SelectionStart { get; set; }
        public ReactiveProperty<string> CharacterCount { get; set; }

        public ReactiveProperty<string> Message { get; set; }

        public ReactiveProperty<bool> ToolTipIsOpen { get; set; }

        public ReactiveProperty<Symbol> LockingHashTagsSymbol { get; set; }

        public ReactiveProperty<Symbol> StateSymbol { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveProperty<double> AccountImageSize { get; set; }

        public ReactiveCommand OpenAccountSettingCommand { get; set; }
        public ReactiveCommand ChangeLockHashTagsCommand { get; set; }
        public ReactiveCommand AddPictureCommand { get; set; }
        public ReactiveCommand TweetCommand { get; set; }
        public ReactiveCommand SuggestSelectedCommand { get; set; }

        public ReactiveCommand DeleteReplyOrQuotedStatusCommand { get; set; }

        public ReactiveCommand PasteClipbordPictureCommand { get; set; }

        public ReactiveCommand MessageShowCommand { get; set; }

        public Messenger SuggestionMessenger { get; }

        public Messenger TextBoxFocusMessenger { get; }
    }

    public class PictureViewModel : IDisposable
    {
        public PictureViewModel(PictureModel picture)
        {
            Image = picture.ObserveProperty(x => x.Stream)
                .SubscribeOnUIDispatcher()
                .Select(x =>
                {
                    if (!picture.IsVideo)
                    {
                        var bitmap = new BitmapImage();

                        var stream = x;
                        if (stream != null)
                            bitmap.SetSource(stream);

                        return (ImageSource) bitmap;
                    }
                    if (picture.StorageFile != null)
                    {
                        var thumbnailTask = picture.StorageFile.GetThumbnailAsync(ThumbnailMode.VideosView).AsTask();
                        thumbnailTask.Wait();

                        var bitmap = new BitmapImage();

                        var stream = thumbnailTask.Result as IRandomAccessStream;
                        if (stream != null)
                            bitmap.SetSource(stream);

                        return (ImageSource) bitmap;
                    }

                    return new BitmapImage();
                })
                .ToReactiveProperty();

            Notice = Notice.Instance;
            PictureModel = picture;
        }

        public ReactiveProperty<ImageSource> Image { get; set; }

        public PictureModel PictureModel { get; set; }
        public Notice Notice { get; set; }

        public void Dispose()
        {
            Image.Dispose();
        }
    }
}