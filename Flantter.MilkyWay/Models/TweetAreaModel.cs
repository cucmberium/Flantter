using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Flantter.MilkyWay.Models.Notifications;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Twitter;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Behaviors;
using Flantter.MilkyWay.Views.Util;
using Prism.Mvvm;
using ToriatamaText;

namespace Flantter.MilkyWay.Models
{
    public class TweetAreaModel : BindableBase
    {
        public const int MaxTweetLength = 140;
        private readonly Extractor _extractor;

        private readonly ResourceLoader _resourceLoader;
        private bool _textChanged;

        private IEnumerable<SuggestionService.SuggestionToken> _tokens;

        public TweetAreaModel()
        {
            _resourceLoader = new ResourceLoader();

            _pictures = new ObservableCollection<PictureModel>();
            _readonlyPictures = new ReadOnlyObservableCollection<PictureModel>(_pictures);
            _text = string.Empty;
            CharacterCount = 140;
            State = "Accept";
            Message = _resourceLoader.GetString("TweetArea_Message_AllSet");

            _extractor = new Extractor();
        }

        public void CharacterCountChanged()
        {
            var text = _text.Replace("\r\n", "\n");

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

            var resultUrls = _extractor.ExtractUrls(text);
            var length = text.Count(x => !char.IsLowSurrogate(x)) - resultUrls.Sum(x => x.Length) +
                         23 * resultUrls.Count;

            CharacterCount = MaxTweetLength - length;
        }

        public async Task AddPicture(StorageFile picture)
        {
            if (picture == null)
                return;

            if (SettingService.Setting.ConvertPostingImage &&
                (picture.FileType == ".jpeg" || picture.FileType == ".jpg" || picture.FileType == ".png"))
            {
                RandomAccessStreamReference newBitmap;
                var memoryStream = new InMemoryRandomAccessStream();

                using (IRandomAccessStream fileStream =
                    await RandomAccessStreamReference.CreateFromFile(picture).OpenReadAsync())
                {
                    var picDecoder = await BitmapDecoder.CreateAsync(fileStream);
                    var picDecoderPixels = await picDecoder.GetPixelDataAsync();
                    var picEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, memoryStream);
                    var data = picDecoderPixels.DetachPixelData();

                    if (SettingService.Setting.ConvertPostingImage && data[3] >= 254)
                        data[3] = 254; // 左上1pixelの透明度情報を254に設定し,Twitter側の劣化に抗う

                    picEncoder.SetPixelData(picDecoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied,
                        picDecoder.PixelWidth, picDecoder.PixelHeight, picDecoder.DpiX, picDecoder.DpiY, data);

                    await picEncoder.FlushAsync();

                    newBitmap = RandomAccessStreamReference.CreateFromStream(memoryStream);
                }

                if (memoryStream.Size > 3145728)
                {
                    _pictures.Add(new PictureModel
                    {
                        Stream = await RandomAccessStreamReference.CreateFromFile(picture).OpenReadAsync(),
                        IsGifAnimation = false,
                        IsVideo = false /*StorageFile = picture*/
                    });
                    memoryStream.Dispose();
                }
                else
                {
                    _pictures.Add(new PictureModel
                    {
                        Stream = await newBitmap.OpenReadAsync(),
                        IsVideo = false,
                        IsGifAnimation = false,
                        SourceStream = memoryStream
                    });
                }
            }
            else
            {
                _pictures.Add(new PictureModel
                {
                    Stream = await RandomAccessStreamReference.CreateFromFile(picture).OpenReadAsync(),
                    IsGifAnimation = picture.FileType == ".gif",
                    IsVideo = picture.FileType == ".mp4" || picture.FileType == ".mov",
                    StorageFile = picture
                });
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

            RandomAccessStreamReference newBitmap;
            var memoryStream = new InMemoryRandomAccessStream();

            using (IRandomAccessStream fileStream = await bitmap.OpenReadAsync())
            {
                var picDecoder = await BitmapDecoder.CreateAsync(fileStream);
                var picDecoderPixels = await picDecoder.GetPixelDataAsync();
                var picEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, memoryStream);
                var data = picDecoderPixels.DetachPixelData();

                if (SettingService.Setting.ConvertPostingImage && data[3] >= 254)
                    data[3] = 254; // 左上1pixelの透明度情報を254に設定し,Twitter側の劣化に抗う

                picEncoder.SetPixelData(picDecoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied,
                    picDecoder.PixelWidth, picDecoder.PixelHeight, picDecoder.DpiX, picDecoder.DpiY, data);

                await picEncoder.FlushAsync();

                newBitmap = RandomAccessStreamReference.CreateFromStream(memoryStream);
            }

            _pictures.Add(new PictureModel
            {
                Stream = await newBitmap.OpenReadAsync(),
                IsVideo = false,
                IsGifAnimation = false,
                SourceStream = memoryStream
            });
            CharacterCountChanged();
        }

        public void DeletePicture(PictureModel picture)
        {
            _pictures.Remove(picture);
            picture.Dispose();
            CharacterCountChanged();
        }

        public async Task Tweet(IEnumerable<AccountModel> accounts)
        {
            if (!accounts.Any())
                return;

            if (Updating)
                return;

            ToolTipIsOpen = true;

            if (CharacterCount < 0)
            {
                State = "Cancel";
                Message = _resourceLoader.GetString("TweetArea_Message_Over140Character");
                return;
            }
            if (_pictures.Count == 0 && string.IsNullOrWhiteSpace(Text))
            {
                State = "Cancel";
                Message = _resourceLoader.GetString("TweetArea_Message_TextIsEmptyOrWhiteSpace");
                return;
            }
            if (_pictures.Count(x => !x.IsVideo && !x.IsGifAnimation) > 4 ||
                _pictures.Count(x => x.IsVideo || x.IsGifAnimation) > 1 ||
                _pictures.Count(x => !x.IsVideo && x.IsGifAnimation) > 1)
            {
                State = "Cancel";
                Message = _resourceLoader.GetString("TweetArea_Message_TwitterMediaOverCapacity");
                return;
            }
            if (_pictures.Any(x => x.IsVideo || x.IsGifAnimation) &&
                _pictures.Any(x => !x.IsVideo && !x.IsGifAnimation))
            {
                State = "Cancel";
                Message = _resourceLoader.GetString("TweetArea_Message_TwitterMediaOverCapacity");
                return;
            }
            if (_pictures.Where(x => !x.IsVideo).Any(x => x.Stream.Size > 3145728) || _pictures.Where(x => x.IsVideo)
                    .Any(x => x.Stream.Size > 536870912))
            {
                State = "Cancel";
                Message = _resourceLoader.GetString("TweetArea_Message_MediaSizeOver");
                return;
            }

            Updating = true;

            foreach (var account in accounts)
            {
                var tokens = account.Tokens;
                var text = Text;

                try
                {
                    var param = new Dictionary<string, object>();
                    if (ReplyOrQuotedStatus != null)
                    {
                        if (!_isQuotedRetweet)
                        {
                            param.Add("in_reply_to_status_id", ReplyOrQuotedStatus.Id);
                        }
                        else
                        {
                            if (tokens.Platform == Tokens.PlatformEnum.Twitter)
                                param.Add("attachment_url", ReplyOrQuotedStatus.Url);
                            else
                                text += " " + ReplyOrQuotedStatus.Url;
                        }
                    }

                    // Upload Media

                    if (_pictures.Count > 0)
                    {
                        Message = _resourceLoader.GetString("TweetArea_Message_UploadingMedia") + " , " + "0.0%";

                        var resultList = new List<long>();

                        if (_pictures.First().IsVideo)
                        {
                            var pic = _pictures.First();
                            var progress = new Progress<CoreTweet.UploadChunkedProgressInfo>();
                            progress.ProgressChanged += (s, e) =>
                            {
                                double progressPercentage;
                                if (e.Stage == CoreTweet.UploadChunkedProgressStage.InProgress)
                                    progressPercentage = 0.5 + e.ProcessingProgressPercent / 100.0 / 2.0;
                                else if (e.Stage == CoreTweet.UploadChunkedProgressStage.Pending)
                                    progressPercentage = 0.5;
                                else if (e.Stage == CoreTweet.UploadChunkedProgressStage.SendingContent)
                                    progressPercentage = e.BytesSent / (double)pic.Stream.Size * 0.5 >= 0.5
                                        ? 0.5
                                        : e.BytesSent / (double)pic.Stream.Size * 0.5;
                                else
                                    progressPercentage = 0.0;

                                progressPercentage *= 100.0;
                                Message = _resourceLoader.GetString("TweetArea_Message_UploadingMedia") + " , " +
                                          progressPercentage.ToString("#0.0") + "%";
                            };

                            pic.Stream.Seek(0);
                            resultList.Add(await tokens.Media.UploadChunkedAsync(pic.Stream.AsStream(),
                                Twitter.Wrapper.Media.UploadMediaTypeEnum.Video, "tweet_video", progress: progress));
                        }
                        else
                        {
                            foreach (var item in _pictures.Select((v, i) => new { v, i }))
                            {
                                var progress = new Progress<CoreTweet.UploadProgressInfo>();
                                progress.ProgressChanged += (s, e) =>
                                {
                                    var progressPercentage = (item.i / (double)_pictures.Count +
                                                              (e.BytesSent / (double)item.v.Stream.Size > 1.0
                                                                  ? 1.0
                                                                  : e.BytesSent / (double)item.v.Stream.Size) /
                                                              _pictures.Count) * 100.0;
                                    Message = _resourceLoader.GetString("TweetArea_Message_UploadingMedia") + " , " +
                                              progressPercentage.ToString("#0.0") + "%";
                                };

                                var pic = item.v;
                                pic.Stream.Seek(0);
                                resultList.Add(await tokens.Media.UploadAsync(pic.Stream.AsStream(), progress: progress));
                            }
                        }

                        param.Add("media_ids", resultList);
                        param.Add("possibly_sensitive", account.AccountSetting.PossiblySensitive);
                    }

                    param.Add("status", text);

                    Message = _resourceLoader.GetString("TweetArea_Message_UpdatingStatus");
                    await tokens.Statuses.UpdateAsync(param);
                }
                catch (CoreTweet.TwitterException ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                    State = "Cancel";
                    Message = _resourceLoader.GetString("TweetArea_Message_Error");
                    return;
                }
                catch (NotImplementedException ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_NotImplementedException"),
                        _resourceLoader.GetString("Notification_System_NotImplementedException"));
                    State = "Cancel";
                    Message = _resourceLoader.GetString("TweetArea_Message_Error");
                    return;
                }
                catch (Exception ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                        _resourceLoader.GetString("Notification_System_CheckNetwork"));
                    State = "Cancel";
                    Message = _resourceLoader.GetString("TweetArea_Message_Error");
                    return;
                }
                finally
                {
                    Updating = false;
                }
            }

            Core.Instance.PopupToastNotification(PopupNotificationType.TweetCompleted,
                _resourceLoader.GetString("Notification_TweetCompleted_TweetCompleted"), Text);

            foreach (var pic in _pictures)
                pic.Dispose();

            _pictures.Clear();

            var newtext = string.Empty;
            if (LockingHashTags)
                foreach (var token in _tokens.Where(
                    x => x.Type == SuggestionService.SuggestionToken.SuggestionTokenId.HashTag))
                    newtext += " #" + token.Value;
            Text = newtext;

            ReplyOrQuotedStatus = null;
            IsQuotedRetweet = false;
            IsReply = false;

            State = "Accept";
            Message = _resourceLoader.GetString("TweetArea_Message_AllSet");
            ToolTipIsOpen = false;
        }

        private void SuggestionTokenize()
        {
            if (string.IsNullOrWhiteSpace(_text))
            {
                _tokens = null;
                return;
            }

            try
            {
                _tokens = SuggestionService.Tokenize(_text);
            }
            catch
            {
                _tokens = null;
            }
        }

        private void SuggestionCheck()
        {
            if (_tokens == null)
            {
                SuggestionMessenger.Raise(new SuggestionNotification {SuggestWords = null, IsOpen = false});
                return;
            }

            try
            {
                var token = SuggestionService.GetTokenFromPosition(_tokens, _selectionStart);
                var words = new List<string>();

                switch (token.Type)
                {
                    case SuggestionService.SuggestionToken.SuggestionTokenId.HashTag:
                        foreach (var userId in Connecter.Instance.TweetCollecter.Keys)
                        {
                            lock (Connecter.Instance.TweetCollecter[userId].EntitiesObjectsLock)
                            {
                                words.AddRange(Connecter.Instance.TweetCollecter[userId]
                                    .HashTagObjects.Where(x => x.StartsWith(token.Value))
                                    .OrderBy(x => x));
                            }
                        }
                        break;
                    case SuggestionService.SuggestionToken.SuggestionTokenId.ScreenName:
                        if (!string.IsNullOrEmpty(token.Value))
                        {
                            foreach (var userId in Connecter.Instance.TweetCollecter.Keys)
                            {
                                lock (Connecter.Instance.TweetCollecter[userId].EntitiesObjectsLock)
                                {
                                    words.AddRange(Connecter.Instance.TweetCollecter[userId]
                                        .ScreenNameObjects.Where(x => x.StartsWith(token.Value))
                                        .OrderBy(x => x));
                                }
                            }
                        }
                        break;
                }

                SuggestionMessenger.Raise(
                    new SuggestionNotification {SuggestWords = words, IsOpen = words.Count > 0});
            }
            catch
            {
                SuggestionMessenger.Raise(new SuggestionNotification {SuggestWords = null, IsOpen = false});
            }
        }

        private void SuggestionHide()
        {
            SuggestionMessenger.Raise(new SuggestionNotification {SuggestWords = null, IsOpen = false});
        }

        public void SuggestionSelected(string word)
        {
            var token = SuggestionService.GetTokenFromPosition(_tokens, _selectionStart);
            var text = Text.Replace("\r\n", "\n");

            var startText = text.Replace("\r\n", "\n").Substring(0, token.Pos);
            var endText = text.Replace("\r\n", "\n")
                .Substring(token.Pos + token.Length, text.Replace("\r\n", "\n").Length - (token.Pos + token.Length));
            text =
            (startText + (token.Type == SuggestionService.SuggestionToken.SuggestionTokenId.HashTag ? "#" : "@") +
             word + " " + endText).Replace("\n", "\r\n");

            Text = text;
            SelectionStart = text.Replace("\r\n", "\n").Length - endText.Replace("\r\n", "\n").Length;
        }

        #region Text変更通知プロパティ

        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    RaisePropertyChanged();

                    _textChanged = true;

                    CharacterCountChanged();

                    SuggestionTokenize();

                    SuggestionCheck();
                }
            }
        }

        #endregion

        #region SelectionStart変更通知プロパティ

        private int _selectionStart;

        public int SelectionStart
        {
            get { return _selectionStart; }
            set
            {
                if (_selectionStart != value)
                {
                    _selectionStart = value;
                    RaisePropertyChanged();

                    if (!_textChanged)
                        SuggestionHide();

                    _textChanged = false;
                }
            }
        }

        #endregion

        #region CharacterCount変更通知プロパティ

        private int _characterCount;

        public int CharacterCount
        {
            get => _characterCount;
            set => SetProperty(ref _characterCount, value);
        }

        #endregion

        #region LockingHashTags変更通知プロパティ

        private bool _lockingHashTags;

        public bool LockingHashTags
        {
            get => _lockingHashTags;
            set => SetProperty(ref _lockingHashTags, value);
        }

        #endregion

        #region State変更通知プロパティ

        private string _state;

        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        #endregion

        #region Pictures変更通知プロパティ

        private readonly ObservableCollection<PictureModel> _pictures;
        private ReadOnlyObservableCollection<PictureModel> _readonlyPictures;

        public ReadOnlyObservableCollection<PictureModel> ReadonlyPictures
        {
            get => _readonlyPictures;
            set => SetProperty(ref _readonlyPictures, value);
        }

        #endregion

        #region Message変更通知プロパティ

        private string _message;

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        #endregion

        #region ToolTipIsOpen変更通知プロパティ

        private bool _toolTipIsOpen;

        public bool ToolTipIsOpen
        {
            get => _toolTipIsOpen;
            set => SetProperty(ref _toolTipIsOpen, value);
        }

        #endregion

        #region Updating変更通知プロパティ

        private bool _updating;

        public bool Updating
        {
            get => _updating;
            set => SetProperty(ref _updating, value);
        }

        #endregion

        #region SuggestionMessenger変更通知プロパティ

        private Messenger _suggestionMessenger;

        public Messenger SuggestionMessenger
        {
            get => _suggestionMessenger;
            set => SetProperty(ref _suggestionMessenger, value);
        }

        #endregion

        #region ReplyOrQuotedStatus変更通知プロパティ

        private Status _replyOrQuotedStatus;

        public Status ReplyOrQuotedStatus
        {
            get => _replyOrQuotedStatus;
            set => SetProperty(ref _replyOrQuotedStatus, value);
        }

        #endregion

        #region IsQuotedRetweet変更通知プロパティ

        private bool _isQuotedRetweet;

        public bool IsQuotedRetweet
        {
            get { return _isQuotedRetweet; }
            set
            {
                if (_isQuotedRetweet != value)
                {
                    _isQuotedRetweet = value;
                    RaisePropertyChanged();

                    CharacterCountChanged();
                }
            }
        }

        #endregion

        #region IsReply変更通知プロパティ

        private bool _isReply;

        public bool IsReply
        {
            get => _isReply;
            set => SetProperty(ref _isReply, value);
        }

        #endregion
    }

    public class PictureModel : BindableBase, IDisposable
    {
        public void Dispose()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            if (_sourceStream != null)
            {
                _sourceStream.Dispose();
                _sourceStream = null;
            }
        }

        #region Stream変更通知プロパティ

        private IRandomAccessStream _stream;

        public IRandomAccessStream Stream
        {
            get => _stream;
            set => SetProperty(ref _stream, value);
        }

        #endregion

        #region IsVideo変更通知プロパティ

        private bool _isVideo;

        public bool IsVideo
        {
            get => _isVideo;
            set => SetProperty(ref _isVideo, value);
        }

        #endregion

        #region IsGifAnimation変更通知プロパティ

        private bool _isGifAnimation;

        public bool IsGifAnimation
        {
            get => _isGifAnimation;
            set => SetProperty(ref _isGifAnimation, value);
        }

        #endregion

        #region SourceStream変更通知プロパティ

        private IRandomAccessStream _sourceStream;

        public IRandomAccessStream SourceStream
        {
            get => _sourceStream;
            set => SetProperty(ref _sourceStream, value);
        }

        #endregion

        #region StorageFile変更通知プロパティ

        private StorageFile _storageFile;

        public StorageFile StorageFile
        {
            get => _storageFile;
            set => SetProperty(ref _storageFile, value);
        }

        #endregion
    }
}