using Flantter.MilkyWay.Models;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;
using Flantter.MilkyWay.Views.Util;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media;
using Windows.Storage.Streams;
using Flantter.MilkyWay.ViewModels.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Objects;
using System.Reactive.Concurrency;
using Flantter.MilkyWay.Setting;
using System.Collections;

namespace Flantter.MilkyWay.ViewModels
{
    public class TweetAreaViewModel
    {
        public TweetAreaModel Model { get; set; }
        public Services.Notice Notice { get; set; }
        public Setting.SettingService Setting { get; set; }

        public ReadOnlyReactiveCollection<AccountViewModel> Accounts { get; private set; }

        public ReadOnlyReactiveCollection<PictureViewModel> Pictures { get; private set; }

        public ReactiveProperty<AccountViewModel> SelectedAccount { get; set; }

        public ReactiveProperty<StatusViewModel> ReplyOrQuotedStatus { get; set; }
        public ReactiveProperty<bool> IsQuotedRetweet { get; set; }
        public ReactiveProperty<bool> IsReply { get; set; }

        public ReactiveProperty<string> Text { get; set; }
        public ReactiveProperty<int> SelectionStart { get; set; }
        public ReactiveProperty<string> CharacterCount { get; set; }

        public ReactiveProperty<string> Message { get; set; }

        public ReactiveProperty<Symbol> LockingHashTagsSymbol { get; set; }

        public ReactiveProperty<Symbol> StateSymbol { get; set; }

        public ReactiveProperty<bool> Updating { get; set; }

        public ReactiveCommand ChangeLockHashTagsCommand { get; set; }
        public ReactiveCommand AddPictureCommand { get; set; }
        public ReactiveCommand TweetCommand { get; set; }
        public ReactiveCommand SuggestSelectedCommand { get; set; }

        public ReactiveCommand DeleteReplyOrQuotedStatusCommand { get; set; }

        public ReactiveCommand PasteClipbordPictureCommand { get; set; }

        public Messenger SuggestionMessenger { get; private set; }

        public Messenger TextBoxFocusMessenger { get; private set; }

        public TweetAreaViewModel(ReadOnlyReactiveCollection<AccountViewModel> accounts)
        {
            this.Model = new TweetAreaModel();

            this.Accounts = accounts;
            this.SelectedAccount = new ReactiveProperty<AccountViewModel>();

            this.SelectionStart = this.Model.ToReactivePropertyAsSynchronized(x => x.SelectionStart);
            this.Text = this.Model.ToReactivePropertyAsSynchronized(x => x.Text);
            this.CharacterCount = this.Model.ObserveProperty(x => x.CharacterCount).Select(x => x.ToString()).ToReactiveProperty();

            this.Message = this.Model.ObserveProperty(x => x.Message).ToReactiveProperty();

            this.LockingHashTagsSymbol = this.Model.ObserveProperty(x => x.LockingHashTags).Select(x => x ? Symbol.UnPin : Symbol.Pin).ToReactiveProperty();

            this.StateSymbol = this.Model.ObserveProperty(x => x.State).Select(x =>
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
            }).ToReactiveProperty();

            this.Updating = this.Model.ObserveProperty(x => x.Updating).ToReactiveProperty();

            this.ReplyOrQuotedStatus = new ReactiveProperty<StatusViewModel>();
            this.IsQuotedRetweet = this.Model.ObserveProperty(x => x.IsQuotedRetweet).ToReactiveProperty();
            this.IsReply = this.Model.ObserveProperty(x => x.IsReply).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;
            this.Setting = SettingService.Setting;

            this.SuggestionMessenger = new Messenger();
            this.Model.SuggestionMessenger = this.SuggestionMessenger;
            this.TextBoxFocusMessenger = new Messenger();

            this.Pictures = this.Model.ReadonlyPictures.ToReadOnlyReactiveCollection(x => new PictureViewModel(x));

            this.Model.ObserveProperty(x => x.ReplyOrQuotedStatus).Subscribe(x => 
            {
                var status = x as Status;
                if (status == null)
                    this.ReplyOrQuotedStatus.Value = null;
            });

            this.ChangeLockHashTagsCommand = new ReactiveCommand();
            this.ChangeLockHashTagsCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x => 
            {
                this.Model.LockingHashTags = !this.Model.LockingHashTags;
            });

            this.AddPictureCommand = new ReactiveCommand();
            this.AddPictureCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var result = await Notice.ShowFilePickerMessenger.Raise(new FileOpenPickerNotification
                {
                    FileTypeFilter = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".mov" },
                    IsMultiple = true,
                });

                Services.Notice.Instance.TweetAreaOpenCommand.Execute(true);

                foreach (var pic in result.Result)
                    await this.Model.AddPicture(pic);
            });

            this.TweetCommand = this.Model.ObserveProperty(x => x.CharacterCount).Select(x => x >= 0).ToReactiveCommand();
            this.TweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => 
            {
                await this.Model.Tweet(this.SelectedAccount.Value.Model);
                
                if (SettingService.Setting.CloseBottomAppBarAfterTweet)
                {
                    Services.Notice.Instance.TweetAreaOpenCommand.Execute(false);
                }
                else
                {
                    await Task.Delay(50);
                    await this.TextBoxFocusMessenger.Raise(new Notification());
                }
            });

            this.SuggestSelectedCommand = new ReactiveCommand();
            this.SuggestSelectedCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x => 
            {
                this.Model.SuggestionSelected((string)x);
            });

            this.DeleteReplyOrQuotedStatusCommand = new ReactiveCommand();
            this.DeleteReplyOrQuotedStatusCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => 
            {
                this.ReplyOrQuotedStatus.Value = null;

                this.Model.IsQuotedRetweet = false;
                this.Model.IsReply = false;
                this.Model.ReplyOrQuotedStatus = null;

                this.Model.Text = string.Empty;

                await Task.Delay(50);
                await this.TextBoxFocusMessenger.Raise(new Notification());
            });

            this.PasteClipbordPictureCommand = new ReactiveCommand();
            this.PasteClipbordPictureCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => 
            {
                await this.Model.AddPictureFromClipboard();
            });

            Services.Notice.Instance.TweetAreaAccountChangeCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x => 
            {
                var accountVM = x as AccountViewModel;
                this.SelectedAccount.Value = accountVM;
                this.Model.SelectedAccountUserId = accountVM.Model.UserId;
            });

            Services.Notice.Instance.TweetAreaDeletePictureCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(x =>
            {
                var pictureViewModel = x as PictureViewModel;
                this.Model.DeletePicture(pictureViewModel._PictureModel);
            });

            Services.Notice.Instance.ReplyCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => 
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel != null)
                {
                    this.ReplyOrQuotedStatus.Value = statusViewModel;

                    this.Model.IsQuotedRetweet = false;
                    this.Model.IsReply = true;
                    this.Model.ReplyOrQuotedStatus = statusViewModel.Model;

                    this.Model.Text = "@" + statusViewModel.Model.User.ScreenName + " ";

                    Services.Notice.Instance.TweetAreaOpenCommand.Execute(true);

                    await Task.Delay(50);

                    this.Model.SelectionStart = this.Model.Text.Length;

                    return;
                }

                var screenName = x as string;
                if (!string.IsNullOrWhiteSpace(screenName))
                {
                    this.Model.Text = "@" + screenName + " ";

                    Services.Notice.Instance.TweetAreaOpenCommand.Execute(true);

                    await Task.Delay(50);

                    this.Model.SelectionStart = this.Model.Text.Length;

                    return;
                }
            });

            Services.Notice.Instance.ReplyToAllCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                this.ReplyOrQuotedStatus.Value = statusViewModel;

                this.Model.IsQuotedRetweet = false;
                this.Model.IsReply = true;
                this.Model.ReplyOrQuotedStatus = statusViewModel.Model;

                var userList = new List<string>();

                this.Model.Text = "@" + statusViewModel.Model.User.ScreenName + " ";
                userList.Add(statusViewModel.Model.User.ScreenName);

                foreach (var user in statusViewModel.Model.Entities.UserMentions)
                {
                    if (userList.Contains(user.ScreenName) || user.ScreenName == this.SelectedAccount.Value.ScreenName.Value)
                        continue;

                    this.Model.Text += "@" + user.ScreenName + " ";
                    userList.Add(user.ScreenName);
                }

                Services.Notice.Instance.TweetAreaOpenCommand.Execute(true);

                await Task.Delay(50);

                this.Model.SelectionStart = this.Model.Text.Length;
            });

            Services.Notice.Instance.ReplyToStatusesCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x =>
            {
                var items = x as IEnumerable;
                if (items == null)
                    return;

                var statusViewModels = items.Cast<StatusViewModel>();
                if (statusViewModels.Count() == 0)
                    return;
                
                var statusViewModel = statusViewModels.First();

                this.ReplyOrQuotedStatus.Value = statusViewModels.First();

                this.Model.IsQuotedRetweet = false;
                this.Model.IsReply = true;
                this.Model.ReplyOrQuotedStatus = statusViewModel.Model;

                var userList = new List<string>();
                foreach (var sVM in statusViewModels)
                {
                    if (userList.Contains(sVM.ScreenName) || sVM.ScreenName == this.SelectedAccount.Value.ScreenName.Value)
                        continue;

                    userList.Add(sVM.Model.User.ScreenName);

                    foreach (var user in sVM.Model.Entities.UserMentions)
                    {
                        if (userList.Contains(user.ScreenName) || user.ScreenName == this.SelectedAccount.Value.ScreenName.Value)
                            continue;

                        this.Model.Text += "@" + user.ScreenName + " ";
                        userList.Add(user.ScreenName);
                    }
                }

                this.Model.Text = string.Join(" ", userList.Select(screenName => "@" + screenName)) + " ";
                
                Services.Notice.Instance.TweetAreaOpenCommand.Execute(true);

                await Task.Delay(50);

                this.Model.SelectionStart = this.Model.Text.Length;
            });

            Services.Notice.Instance.UrlQuoteRetweetCommand.SubscribeOn(ThreadPoolScheduler.Default).Subscribe(async x => 
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                this.ReplyOrQuotedStatus.Value = statusViewModel;

                this.Model.IsQuotedRetweet = true;
                this.Model.IsReply = false;
                this.Model.ReplyOrQuotedStatus = statusViewModel.Model;

                this.Model.Text = "";

                Services.Notice.Instance.TweetAreaOpenCommand.Execute(true);

                await Task.Delay(50);

                this.Model.SelectionStart = 0;
            });
        }
    }

    public class PictureViewModel : IDisposable
    {
        public PictureViewModel(PictureModel picture)
        {
            this.Image = picture.ObserveProperty(x => x.Stream).SubscribeOnUIDispatcher().Select(x => 
            {
                if (!picture.IsVideo)
                { 
                    BitmapImage bitmap = new BitmapImage();

                    var stream = x as IRandomAccessStream;
                    if (stream != null)
                        bitmap.SetSource(stream);

                    return (ImageSource)bitmap;
                }
                else if (picture.StorageFile != null)
                {
                    var thumbnailTask = picture.StorageFile.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.VideosView).AsTask();
                    thumbnailTask.Wait();

                    BitmapImage bitmap = new BitmapImage();

                    var stream = thumbnailTask.Result as IRandomAccessStream;
                    if (stream != null)
                        bitmap.SetSource(stream);

                    return (ImageSource)bitmap;
                }

                return new BitmapImage();

            }).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;
            this._PictureModel = picture;
        }

        public ReactiveProperty<ImageSource> Image { get; set; }

        public PictureModel _PictureModel { get; set; }
        public Services.Notice Notice { get; set; }

        public void Dispose()
        {
            this.Image.Dispose();
        }
    }
}
