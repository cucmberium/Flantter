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

namespace Flantter.MilkyWay.ViewModels
{
    public class TweetAreaViewModel
    {
        public MainPageModel _MainPageModel { get; set; }
        public TweetAreaModel _TweetAreaModel { get; set; }
        public Services.Notice Notice { get; set; }

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

        public Messenger ShowFilePickerMessenger { get; private set; }

        public Messenger SuggestionMessenger { get; private set; }

        public Messenger TextBoxFocusMessenger { get; private set; }

        public TweetAreaViewModel(ReadOnlyReactiveCollection<AccountViewModel> accounts)
        {
            this._MainPageModel = MainPageModel.Instance;
            this._TweetAreaModel = new TweetAreaModel();

            this.Accounts = accounts;
            this.SelectedAccount = new ReactiveProperty<AccountViewModel>(Accounts.First());

            this.SelectionStart = this._TweetAreaModel.ToReactivePropertyAsSynchronized(x => x.SelectionStart);
            this.Text = this._TweetAreaModel.ToReactivePropertyAsSynchronized(x => x.Text);
            this.CharacterCount = this._TweetAreaModel.ObserveProperty(x => x.CharacterCount).Select(x => x.ToString()).ToReactiveProperty();

            this.Message = this._TweetAreaModel.ObserveProperty(x => x.Message).ToReactiveProperty();

            this.LockingHashTagsSymbol = this._TweetAreaModel.ObserveProperty(x => x.LockingHashTags).Select(x => x ? Symbol.UnPin : Symbol.Pin).ToReactiveProperty();

            this.StateSymbol = this._TweetAreaModel.ObserveProperty(x => x.State).Select(x =>
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

            this.Updating = this._TweetAreaModel.ObserveProperty(x => x.Updating).ToReactiveProperty();

            this.ReplyOrQuotedStatus = new ReactiveProperty<StatusViewModel>();
            this.IsQuotedRetweet = this._TweetAreaModel.ObserveProperty(x => x.IsQuotedRetweet).ToReactiveProperty();
            this.IsReply = this._TweetAreaModel.ObserveProperty(x => x.IsReply).ToReactiveProperty();

            this.Notice = Services.Notice.Instance;

            this.ShowFilePickerMessenger = new Messenger();
            this.SuggestionMessenger = new Messenger();
            this._TweetAreaModel.SuggestionMessenger = this.SuggestionMessenger;
            this.TextBoxFocusMessenger = new Messenger();

            this.Pictures = this._TweetAreaModel.ReadonlyPictures.ToReadOnlyReactiveCollection(x => new PictureViewModel(x));

            this._TweetAreaModel.ObserveProperty(x => x.ReplyOrQuotedStatus).Subscribe(x => 
            {
                var status = x as Status;
                if (status == null)
                    this.ReplyOrQuotedStatus.Value = null;
            });

            this.ChangeLockHashTagsCommand = new ReactiveCommand();
            this.ChangeLockHashTagsCommand.Subscribe(x => 
            {
                this._TweetAreaModel.LockingHashTags = !this._TweetAreaModel.LockingHashTags;
            });

            this.AddPictureCommand = new ReactiveCommand();
            this.AddPictureCommand.Subscribe(async x =>
            {
                var result = await this.ShowFilePickerMessenger.Raise(new FileOpenPickerNotification
                {
                    FileTypeFilter = new[] { ".jpg", ".jpeg", ".png", ".gif", ".mp4", },
                    IsMultiple = true,
                });

                Services.Notice.Instance.TweetAreaOpenCommand.Execute(true);

                foreach (var pic in result.Result)
                    this._TweetAreaModel.AddPicture(pic);
            });

            this.TweetCommand = this._TweetAreaModel.ObserveProperty(x => x.CharacterCount).Select(x => x >= 0).ToReactiveCommand();
            this.TweetCommand.Subscribe(async x => 
            {
                await this._TweetAreaModel.Tweet(this.SelectedAccount.Value._AccountModel);
                
                if (Setting.SettingService.Setting.CloseBottomAppBarAfterTweet)
                    Services.Notice.Instance.TweetAreaOpenCommand.Execute(false);
                else
                    await this.TextBoxFocusMessenger.Raise(new Notification());
            });

            this.SuggestSelectedCommand = new ReactiveCommand();
            this.SuggestSelectedCommand.Subscribe(x => 
            {
                this._TweetAreaModel.SuggestionSelected((string)x);
            });

            this.DeleteReplyOrQuotedStatusCommand = new ReactiveCommand();
            this.DeleteReplyOrQuotedStatusCommand.Subscribe(async x => 
            {
                this.ReplyOrQuotedStatus.Value = null;

                this._TweetAreaModel.IsQuotedRetweet = false;
                this._TweetAreaModel.IsReply = false;
                this._TweetAreaModel.ReplyOrQuotedStatus = null;

                this._TweetAreaModel.Text = string.Empty;

                await Task.Delay(50);
                await this.TextBoxFocusMessenger.Raise(new Notification());
            });

            Services.Notice.Instance.TweetAreaAccountChangeCommand.Subscribe(x => 
            {
                var accountVM = x as AccountViewModel;
                this.SelectedAccount.Value = accountVM;
                this._TweetAreaModel.SelectedAccountUserId = accountVM._AccountModel.UserId;
            });

            Services.Notice.Instance.TweetAreaDeletePictureCommand.Subscribe(x =>
            {
                var pictureViewModel = x as PictureViewModel;
                this._TweetAreaModel.DeletePicture(pictureViewModel._PictureModel);
            });

            Services.Notice.Instance.ReplyCommand.Subscribe(x => 
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                this.ReplyOrQuotedStatus.Value = statusViewModel;

                this._TweetAreaModel.IsQuotedRetweet = false;
                this._TweetAreaModel.IsReply = true;
                this._TweetAreaModel.ReplyOrQuotedStatus = statusViewModel.Model;

                this._TweetAreaModel.Text = "@" + statusViewModel.Model.User.ScreenName + " ";
                this._TweetAreaModel.SelectionStart = this._TweetAreaModel.Text.Length;

                Services.Notice.Instance.TweetAreaOpenCommand.Execute(true);
            });

            Services.Notice.Instance.UrlQuoteRetweetCommand.Subscribe(x => 
            {
                var statusViewModel = x as StatusViewModel;
                if (statusViewModel == null)
                    return;

                this.ReplyOrQuotedStatus.Value = statusViewModel;

                this._TweetAreaModel.IsQuotedRetweet = true;
                this._TweetAreaModel.IsReply = false;
                this._TweetAreaModel.ReplyOrQuotedStatus = statusViewModel.Model;

                this._TweetAreaModel.Text = "";
                this._TweetAreaModel.SelectionStart = 0;

                Services.Notice.Instance.TweetAreaOpenCommand.Execute(true);
            });
        }
    }

    public class PictureViewModel : IDisposable
    {
        public PictureViewModel(PictureModel picture)
        {
            this.Image = picture.ObserveProperty(x => x.Stream).SubscribeOnUIDispatcher().Select(x => 
            {
                BitmapImage bitmap = new BitmapImage();

                var stream = x as IRandomAccessStream;

                if (stream != null)
                {
                    bitmap.SetSource(x);
                }

                return (ImageSource)bitmap;


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
