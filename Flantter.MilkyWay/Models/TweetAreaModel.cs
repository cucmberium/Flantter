using CoreTweet;
using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Setting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Flantter.MilkyWay.Views.Util;
using Flantter.MilkyWay.Models.Twitter;
using Flantter.MilkyWay.Views.Behaviors;
using Prism.Mvvm;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;

namespace Flantter.MilkyWay.Models
{
    public class TweetAreaModel : BindableBase
    {
        public const int MaxTweetLength = 140;

        public TweetAreaModel()
        {
            this._ResourceLoader = new ResourceLoader();

            this._Pictures = new ObservableCollection<PictureModel>();
            this._ReadonlyPictures = new ReadOnlyObservableCollection<PictureModel>(this._Pictures);
            this._Text = string.Empty;
            this.CharacterCount = 140;
            this.State = "Accept";
            this.Message = _ResourceLoader.GetString("TweetArea_Message_AllSet");

            this._Extractor = new ToriatamaText.Extractor();
        }

        private ResourceLoader _ResourceLoader;
        private ToriatamaText.Extractor _Extractor = null;
        private bool _TextChanged = false;

        #region Text変更通知プロパティ
        private string _Text;
        public string Text
        {
            get { return this._Text; }
            set
            {
                if (this._Text != value)
                {
                    this._Text = value;
                    this.OnPropertyChanged("Text");

                    this._TextChanged = true;

                    this.CharacterCountChanged();

                    this.SuggestionTokenize();
                    
                    this.SuggestionCheck();
                }
            }
        }
        #endregion

        #region SelectionStart変更通知プロパティ
        private int _SelectionStart;
        public int SelectionStart
        {
            get { return this._SelectionStart; }
            set
            {
                if (this._SelectionStart != value)
                {
                    this._SelectionStart = value;
                    this.OnPropertyChanged("SelectionStart");

                    if (!this._TextChanged)
                        this.SuggestionHide();

                    this._TextChanged = false;
                }
            }
        }
        #endregion

        #region CharacterCount変更通知プロパティ
        private int _CharacterCount;
        public int CharacterCount
        {
            get { return this._CharacterCount; }
            set { this.SetProperty(ref this._CharacterCount, value); }
        }
        #endregion

        #region LockingHashTags変更通知プロパティ
        private bool _LockingHashTags;
        public bool LockingHashTags
        {
            get { return this._LockingHashTags; }
            set { this.SetProperty(ref this._LockingHashTags, value); }
        }
        #endregion

        #region State変更通知プロパティ
        private string _State;
        public string State
        {
            get { return this._State; }
            set { this.SetProperty(ref this._State, value); }
        }
        #endregion

        #region Pictures変更通知プロパティ
        
        private ObservableCollection<PictureModel> _Pictures;
        private ReadOnlyObservableCollection<PictureModel> _ReadonlyPictures;
        public ReadOnlyObservableCollection<PictureModel> ReadonlyPictures
        {
            get { return this._ReadonlyPictures; }
            set { this.SetProperty(ref this._ReadonlyPictures, value); }
        }
        #endregion

        #region Message変更通知プロパティ
        private string _Message;
        public string Message
        {
            get { return this._Message; }
            set { this.SetProperty(ref this._Message, value); }
        }
        #endregion

        #region Updating変更通知プロパティ
        private bool _Updating;
        public bool Updating
        {
            get { return this._Updating; }
            set { this.SetProperty(ref this._Updating, value); }
        }
        #endregion

        #region SuggestionMessenger変更通知プロパティ
        private Messenger _SuggestionMessenger;
        public Messenger SuggestionMessenger
        {
            get { return this._SuggestionMessenger; }
            set { this.SetProperty(ref this._SuggestionMessenger, value); }
        }
        #endregion

        #region SelectedAccountUserId変更通知プロパティ
        private long _SelectedAccountUserId;
        public long SelectedAccountUserId
        {
            get { return this._SelectedAccountUserId; }
            set { this.SetProperty(ref this._SelectedAccountUserId, value); }
        }
        #endregion

        #region ReplyOrQuotedStatus変更通知プロパティ
        private Twitter.Objects.Status _ReplyOrQuotedStatus;
        public Twitter.Objects.Status ReplyOrQuotedStatus
        {
            get { return this._ReplyOrQuotedStatus; }
            set { this.SetProperty(ref this._ReplyOrQuotedStatus, value); }
        }
        #endregion

        #region IsQuotedRetweet変更通知プロパティ
        private bool _IsQuotedRetweet;
        public bool IsQuotedRetweet
        {
            get { return this._IsQuotedRetweet; }
            set
            {
                if (this._IsQuotedRetweet != value)
                {
                    this._IsQuotedRetweet = value;
                    this.OnPropertyChanged("IsQuotedRetweet");

                    this.CharacterCountChanged();
                }
            }
        }
        #endregion

        #region IsReply変更通知プロパティ
        private bool _IsReply;
        public bool IsReply
        {
            get { return this._IsReply; }
            set { this.SetProperty(ref this._IsReply, value); }
        }
        #endregion

        public void CharacterCountChanged()
        {
            var text = this._Text.Replace("\r\n", "\n");

            /*var resultReplies = this._Extractor.ExtractMentionedScreenNames(text);
            var replyScreenNames = new List<string>();
            var hiddenPrefixLength = 0;
            foreach (var reply in resultReplies)
            {
                if (reply.StartIndex > hiddenPrefixLength + 1 || replyScreenNames.Any(x => x == text.Substring(reply.StartIndex, reply.Length)))
                    break;

                replyScreenNames.Add(text.Substring(reply.StartIndex, reply.Length));
                hiddenPrefixLength = reply.StartIndex + reply.Length;
            }

            text = text.Substring(hiddenPrefixLength).TrimStart();*/

            var resultUrls = this._Extractor.ExtractUrls(text);
            var length = text.Count(x => !char.IsLowSurrogate(x)) - resultUrls.Sum(x => x.Length) + 23 * resultUrls.Count;

            this.CharacterCount = MaxTweetLength - length;
        }

        public async Task AddPicture(StorageFile picture)
        {
            if (picture == null)
                return;
            
            if (SettingService.Setting.ConvertPostingImage && (picture.FileType == ".jpeg" || picture.FileType == ".jpg" || picture.FileType == ".png"))
            {
                RandomAccessStreamReference newBitmap;
                InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();

                using (IRandomAccessStream fileStream = await RandomAccessStreamReference.CreateFromFile(picture).OpenReadAsync())
                {
                    var picDecoder = await BitmapDecoder.CreateAsync(fileStream);
                    var picDecoderPixels = await picDecoder.GetPixelDataAsync();
                    var picEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, memoryStream);
                    var data = picDecoderPixels.DetachPixelData();

                    if (SettingService.Setting.ConvertPostingImage && data[3] >= 254)
                        data[3] = 254; // 左上1pixelの透明度情報を254に設定し,Twitter側の劣化に抗う

                    picEncoder.SetPixelData(picDecoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied, picDecoder.PixelWidth, picDecoder.PixelHeight, picDecoder.DpiX, picDecoder.DpiY, data);

                    await picEncoder.FlushAsync();

                    newBitmap = RandomAccessStreamReference.CreateFromStream(memoryStream);
                }
                
                if (memoryStream.Size > 3145728)
                {
                    this._Pictures.Add(new PictureModel() { Stream = await RandomAccessStreamReference.CreateFromFile(picture).OpenReadAsync(), IsGifAnimation = false, IsVideo = false, /*StorageFile = picture*/ });
                    memoryStream.Dispose();
                }
                else
                {
                    this._Pictures.Add(new PictureModel() { Stream = await newBitmap.OpenReadAsync(), IsVideo = false, IsGifAnimation = false, SourceStream = memoryStream });
                }
            }
            else
            {
                this._Pictures.Add(new PictureModel() { Stream = await RandomAccessStreamReference.CreateFromFile(picture).OpenReadAsync(), IsGifAnimation = (picture.FileType == ".gif"), IsVideo = (picture.FileType == ".mp4" || picture.FileType == ".mov"), StorageFile = picture });
            }

            CharacterCountChanged();
        }

        public async Task AddPictureFromClipboard()
        {
            try
            {
                var clipboardContent = Clipboard.GetContent();

                if (!clipboardContent.AvailableFormats.Contains("Bitmap"))
                    return;
            }
            catch
            {
                return;
            }

            var bitmap = await Clipboard.GetContent().GetBitmapAsync();

            BitmapImage bImage;
            using (IRandomAccessStream fileStream = await bitmap.OpenReadAsync())
            {
                bImage = new BitmapImage();
                await bImage.SetSourceAsync(fileStream);
            }

            RandomAccessStreamReference newBitmap;
            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();

            using (IRandomAccessStream fileStream = await bitmap.OpenReadAsync())
            {
                var picDecoder = await BitmapDecoder.CreateAsync(fileStream);
                var picDecoderPixels = await picDecoder.GetPixelDataAsync();
                var picEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, memoryStream);
                var data = picDecoderPixels.DetachPixelData();
                
                if (SettingService.Setting.ConvertPostingImage && data[3] >= 254)
                    data[3] = 254; // 左上1pixelの透明度情報を254に設定し,Twitter側の劣化に抗う

                picEncoder.SetPixelData(picDecoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied, picDecoder.PixelWidth, picDecoder.PixelHeight, picDecoder.DpiX, picDecoder.DpiY, data);

                await picEncoder.FlushAsync();

                newBitmap = RandomAccessStreamReference.CreateFromStream(memoryStream);
            }

            this._Pictures.Add(new PictureModel() { Stream = await newBitmap.OpenReadAsync(), IsVideo = false, IsGifAnimation = false, SourceStream = memoryStream });
            CharacterCountChanged();
        }

        public void DeletePicture(PictureModel picture)
        {
            this._Pictures.Remove(picture);
            picture.Dispose();
            CharacterCountChanged();
        }

        public async Task Tweet(AccountModel account)
        {
            if (this.Updating)
                return;

            if (this.CharacterCount < 0)
            {
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_Over140Character");
                return;
            }
            else if (this._Pictures.Count == 0 && string.IsNullOrWhiteSpace(this.Text))
            {
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_TextIsEmptyOrWhiteSpace");
                return;
            }
            else if (this._Pictures.Where(x => !x.IsVideo && !x.IsGifAnimation).Count() > 4 || this._Pictures.Where(x => x.IsVideo || x.IsGifAnimation).Count() > 1 || this._Pictures.Where(x => !x.IsVideo && x.IsGifAnimation).Count() > 1)
            {
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_TwitterMediaOverCapacity");
                return;
            }
            else if (this._Pictures.Where(x => x.IsVideo || x.IsGifAnimation).Count() > 0 && this._Pictures.Where(x => !x.IsVideo && !x.IsGifAnimation).Count() > 0)
            {
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_TwitterMediaOverCapacity");
                return;
            }
            else if (this._Pictures.Where(x => !x.IsVideo).Any(x => x.Stream.Size > 3145728) || this._Pictures.Where(x => x.IsVideo).Any(x => x.Stream.Size > 15728640))
            {
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_Error");
                return;
            }

            this.Updating = true;
            var tokens = account.Tokens;
            var text = this.Text;

            try
            {
                var param = new Dictionary<string, object>() { };
                if (this.ReplyOrQuotedStatus != null)
                {
                    if (!this._IsQuotedRetweet)
                        param.Add("in_reply_to_status_id", this.ReplyOrQuotedStatus.Id);
                    else
                        text += " https://twitter.com/" + this.ReplyOrQuotedStatus.User.ScreenName + "/status/" + this.ReplyOrQuotedStatus.Id;
                }
                    

                // Upload Media

                if (this._Pictures.Count > 0)
                {
                    this.Message = _ResourceLoader.GetString("TweetArea_Message_UploadingMedia") + " , " + "0.0%";

                    var resultList = new List<MediaUploadResult>();

                    foreach (var item in this._Pictures.Select((v, i) => new { v, i }))
                    {
                        var pic = item.v;
                        pic.Stream.Seek(0);
                        if (pic.IsVideo)
                            resultList.Add(await tokens.Media.UploadChunkedAsync(pic.Stream.AsStream(), UploadMediaType.Video, (IEnumerable<long>)null, default(System.Threading.CancellationToken)));
                        else
                            resultList.Add(await tokens.Media.UploadAsync(pic.Stream.AsStream(), (IEnumerable<long>)null, default(System.Threading.CancellationToken)));

                        var progressPercentage = (item.i / (double)this._Pictures.Count) * 100.0;
                        this.Message = _ResourceLoader.GetString("TweetArea_Message_UploadingMedia") + " , " + progressPercentage.ToString("#0.0") + "%";
                    }
                    
                    param.Add("media_ids", resultList.Select(x => x.MediaId));
                    param.Add("possibly_sensitive", account.AccountSetting.PossiblySensitive);
                }

                param.Add("status", text);

                this.Message = _ResourceLoader.GetString("TweetArea_Message_UpdatingStatus");
                await tokens.Statuses.UpdateAsync(param);
            }
            catch (TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_Error");
                return;
            }
            catch (Exception ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_Error");
                return;
            }
            finally
            {
                this.Updating = false;
            }

            foreach (var pic in this._Pictures)
                pic.Dispose();

            this._Pictures.Clear();

            text = string.Empty;
            if (this.LockingHashTags)
            {
                foreach (var token in this.tokens.Where(x => x.Type == SuggestionService.SuggestionToken.SuggestionTokenId.HashTag))
                    text += " #" + token.Value;
            }
            this.Text = text;

            this.ReplyOrQuotedStatus = null;
            this.IsQuotedRetweet = false;
            this.IsReply = false;

            this.State = "Accept";
            this.Message = _ResourceLoader.GetString("TweetArea_Message_AllSet");
        }

        private IEnumerable<SuggestionService.SuggestionToken> tokens;
        private void SuggestionTokenize()
        {
            if (string.IsNullOrWhiteSpace(this._Text))
            {
                tokens = null;
                return;
            }

            try
            {
                tokens = SuggestionService.Tokenize(this._Text);
            }
            catch
            {
                tokens = null;
            }
        }
        private void SuggestionCheck()
        {
            if (tokens == null || !Services.Connecter.Instance.TweetCollecter.ContainsKey(this.SelectedAccountUserId))
            {
                SuggestionMessenger.Raise(new SuggestionNotification() { SuggestWords = null, IsOpen = false });
                return;
            }

            try
            {
                var token = SuggestionService.GetTokenFromPosition(tokens, this._SelectionStart);
                IEnumerable<string> words = null;

                switch (token.Type)
                {
                    case SuggestionService.SuggestionToken.SuggestionTokenId.HashTag:
                        lock (Services.Connecter.Instance.TweetCollecter[this.SelectedAccountUserId].EntitiesObjectsLock)
                        {
                            words = Services.Connecter.Instance.TweetCollecter[this.SelectedAccountUserId].HashTagObjects.Where(x => x.StartsWith(token.Value)).OrderBy(x => x);
                        }
                        break;
                    case SuggestionService.SuggestionToken.SuggestionTokenId.ScreenName:
                        if (!string.IsNullOrEmpty(token.Value))
                        {
                            lock (Services.Connecter.Instance.TweetCollecter[this.SelectedAccountUserId].EntitiesObjectsLock)
                            {
                                words = Services.Connecter.Instance.TweetCollecter[this.SelectedAccountUserId].ScreenNameObjects.Where(x => x.StartsWith(token.Value)).OrderBy(x => x);
                            }
                        }
                        break;
                    default:
                        break;
                }

                SuggestionMessenger.Raise(new SuggestionNotification() { SuggestWords = words, IsOpen = (words != null && words.Count() != 0) });
            }
            catch
            {
                SuggestionMessenger.Raise(new SuggestionNotification() { SuggestWords = null, IsOpen = false });
            }
        }
        private void SuggestionHide()
        {
            SuggestionMessenger.Raise(new SuggestionNotification() { SuggestWords = null, IsOpen = false });
        }
        public void SuggestionSelected(string word)
        {
            var token = SuggestionService.GetTokenFromPosition(tokens, this._SelectionStart);
            var text = Text.Replace("\r\n", "\n");

            var startText = text.Replace("\r\n", "\n").Substring(0, token.Pos);
            var endText = text.Replace("\r\n", "\n").Substring(token.Pos + token.Length, text.Replace("\r\n", "\n").Length - (token.Pos + token.Length));
            text = (startText + (token.Type == SuggestionService.SuggestionToken.SuggestionTokenId.HashTag ? "#" : "@") + word + " " + endText).Replace("\n", "\r\n");

            this.Text = text;
            this.SelectionStart = text.Replace("\r\n", "\n").Length - endText.Replace("\r\n", "\n").Length;
        }
    }

    public class PictureModel : BindableBase, IDisposable
    {
        public PictureModel()
        {

        }

        #region Stream変更通知プロパティ
        private IRandomAccessStream _Stream;
        public IRandomAccessStream Stream
        {
            get { return this._Stream; }
            set { this.SetProperty(ref this._Stream, value); }
        }
        #endregion

        #region IsVideo変更通知プロパティ
        private bool _IsVideo;
        public bool IsVideo
        {
            get { return this._IsVideo; }
            set { this.SetProperty(ref this._IsVideo, value); }
        }
        #endregion

        #region IsGifAnimation変更通知プロパティ
        private bool _IsGifAnimation;
        public bool IsGifAnimation
        {
            get { return this._IsGifAnimation; }
            set { this.SetProperty(ref this._IsGifAnimation, value); }
        }
        #endregion

        #region SourceStream変更通知プロパティ
        private IRandomAccessStream _SourceStream;
        public IRandomAccessStream SourceStream
        {
            get { return this._SourceStream; }
            set { this.SetProperty(ref this._SourceStream, value); }
        }
        #endregion

        #region StorageFile変更通知プロパティ
        private StorageFile _StorageFile;
        public StorageFile StorageFile
        {
            get { return this._StorageFile; }
            set { this.SetProperty(ref this._StorageFile, value); }
        }
        #endregion

        public void Dispose()
        {
            if (this._Stream != null)
            {
                this._Stream.Dispose();
                this._Stream = null;
            }

            if (this._SourceStream != null)
            {
                this._SourceStream.Dispose();
                this._SourceStream = null;
            }

        }
    }
}
