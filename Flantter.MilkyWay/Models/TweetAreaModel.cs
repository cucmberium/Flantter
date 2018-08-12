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
using Flantter.MilkyWay.Models.Apis;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.Notifications;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Behaviors;
using Flantter.MilkyWay.Views.Util;
using Prism.Mvvm;
using ToriatamaText;

namespace Flantter.MilkyWay.Models
{
    public class TweetAreaModel : BindableBase
    {
        public const int MaxTweetLength = 280;
        public const int MaxTootLength = 500;
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
            IsContentWarning = false;

            TwitterCharacterCount = MaxTweetLength;
            MastodonCharacterCount = MaxTootLength;

            _extractor = new Extractor();
        }

        public void CharacterCountChanged()
        {
            var text = _text;

            // var resultReplies = this._extractor.ExtractMentionedScreenNames(text);
            // var replyScreenNames = new List<string>();
            // var hiddenPrefixLength = 0;
            // foreach (var reply in resultReplies)
            // {
            //     if (reply.StartIndex > hiddenPrefixLength + 1 || replyScreenNames.Any(x => x == text.Substring(reply.StartIndex, reply.Length)))
            //         break;
            // 
            //     replyScreenNames.Add(text.Substring(reply.StartIndex, reply.Length));
            //     hiddenPrefixLength = reply.StartIndex + reply.Length;
            // }
            // text = text.Substring(hiddenPrefixLength).TrimStart();
            
            var resultUrls = _extractor.ExtractUrls(text);
            var length = text.Count(x => !char.IsLowSurrogate(x)) - resultUrls.Sum(x => x.Length) +
                         23 * resultUrls.Count;

            /* v2.json has the following unicode code point blocks defined
             * 0x0000 (0)    - 0x10FF (4351) Basic Latin to Georgian block: Weight 100
             * 0x2000 (8192) - 0x200D (8205) Spaces in the General Punctuation Block: Weight 100
             * 0x2010 (8208) - 0x201F (8223) Hyphens &amp; Quotes in the General Punctuation Block: Weight 100
             * 0x2032 (8242) - 0x2037 (8247) Quotes in the General Punctuation Block: Weight 100
             * */

            var lightWeightCharactorCount = text.Count(x => (x >= 0 && x <= 4351) ||
                                                            (x >= 8192 && x <= 8205) ||
                                                            (x >= 8208 && x <= 8223) ||
                                                            (x >= 8242 && x <= 8247));

            TwitterCharacterCount = MaxTweetLength - length * 2 + lightWeightCharactorCount;
            MastodonCharacterCount = MaxTootLength - length;
        }

        public async Task AddPicture(StorageFile picture)
        {
            if (picture == null)
                return;

            var property = await picture.GetBasicPropertiesAsync();

            try
            {
                if (picture.FileType == ".jpeg" || picture.FileType == ".jpg" || picture.FileType == ".png")
                {
                    if (property.Size <= 3145728 && SettingService.Setting.ConvertPostingImage)
                    {
                        var memoryStream = new InMemoryRandomAccessStream();
                        using (IRandomAccessStream fileStream =
                            await RandomAccessStreamReference.CreateFromFile(picture).OpenReadAsync())
                        {
                            var picDecoder = await BitmapDecoder.CreateAsync(fileStream);
                            var picDecoderPixels = await picDecoder.GetPixelDataAsync();
                            var picEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, memoryStream);
                            var data = picDecoderPixels.DetachPixelData();

                            if (data[3] > 254)
                                data[3] = 254; // 左上1pixelの透明度情報を254に設定し,Twitter側の劣化に抗う

                            picEncoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
                                picDecoder.PixelWidth, picDecoder.PixelHeight, picDecoder.DpiX, picDecoder.DpiY, data);

                            await picEncoder.FlushAsync();
                        }

                        if (memoryStream.Size <= 3145728)
                        {
                            var newBitmap = RandomAccessStreamReference.CreateFromStream(memoryStream);
                            _pictures.Add(new PictureModel
                            {
                                Stream = await newBitmap.OpenReadAsync(),
                                IsVideo = false,
                                IsGifAnimation = false,
                                SourceStream = memoryStream
                            });
                        }
                        else
                        {
                            _pictures.Add(new PictureModel
                            {
                                Stream = await RandomAccessStreamReference.CreateFromFile(picture).OpenReadAsync(),
                                IsGifAnimation = false,
                                IsVideo = false
                            });
                            memoryStream.Dispose();
                        }
                    }
                    else if (property.Size > 3145728 && SettingService.Setting.ScalePostingImage)
                    {
                        InMemoryRandomAccessStream memoryStream;
                        using (IRandomAccessStream fileStream =
                            await RandomAccessStreamReference.CreateFromFile(picture).OpenReadAsync())
                        {
                            var picDecoder = await BitmapDecoder.CreateAsync(fileStream);
                            
                            var scale = 1.0;
                            do
                            {
                                memoryStream = new InMemoryRandomAccessStream();

                                var picEncoder = await BitmapEncoder.CreateForTranscodingAsync(memoryStream, picDecoder);
                                picEncoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                                picEncoder.BitmapTransform.ScaledHeight = (uint) Math.Floor(scale * picDecoder.PixelHeight);
                                picEncoder.BitmapTransform.ScaledWidth = (uint) Math.Floor(scale * picDecoder.PixelWidth);
                                await picEncoder.FlushAsync();

                                scale -= 0.05;
                            } while (memoryStream.Size > 3145728);
                        }

                        var newBitmap = RandomAccessStreamReference.CreateFromStream(memoryStream);
                        _pictures.Add(new PictureModel
                        {
                            Stream = await newBitmap.OpenReadAsync(),
                            IsVideo = false,
                            IsGifAnimation = false,
                            SourceStream = memoryStream
                        });
                    }
                    else
                    {
                        _pictures.Add(new PictureModel
                        {
                            Stream = await RandomAccessStreamReference.CreateFromFile(picture).OpenReadAsync(),
                            IsGifAnimation = false,
                            IsVideo = false
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
            }
            catch
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

            var memoryStream = new InMemoryRandomAccessStream();
            using (IRandomAccessStream fileStream = await bitmap.OpenReadAsync())
            {
                var picDecoder = await BitmapDecoder.CreateAsync(fileStream);
                var picDecoderPixels = await picDecoder.GetPixelDataAsync();
                var picEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, memoryStream);
                var data = picDecoderPixels.DetachPixelData();

                if (SettingService.Setting.ConvertPostingImage && data[3] >= 254)
                    data[3] = 254;

                picEncoder.SetPixelData(picDecoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied,
                    picDecoder.PixelWidth, picDecoder.PixelHeight, picDecoder.DpiX, picDecoder.DpiY, data);

                await picEncoder.FlushAsync();
            }

            var newBitmap = RandomAccessStreamReference.CreateFromStream(memoryStream);
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

        public async Task<bool> Tweet(IEnumerable<AccountModel> accounts)
        {
            if (!accounts.Any())
                return false;

            if (Updating)
                return false;
            
            if (TwitterCharacterCount < 0 && accounts.Any(x => x.Platform == "Twitter"))
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("TweetArea_Message_OverMaxTweetLength"));
                return false;
            }
            if (MastodonCharacterCount < 0 && accounts.All(x => x.Platform == "Mastodon"))
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("TweetArea_Message_OverMaxTweetLength"));
                return false;
            }
            if (IsContentWarning == true && accounts.Any(x => x.Platform == "Twitter"))
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("TweetArea_Message_ContentWarningIsAvailableOnlyMastodon"));
                return false;
            }
            if (IsContentWarning == true && string.IsNullOrWhiteSpace(ContentWarningText))
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("TweetArea_Message_ContentWarningIsEmpty"));
                return false;
            }
            if (_pictures.Count == 0 && string.IsNullOrWhiteSpace(Text))
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("TweetArea_Message_TextIsEmptyOrWhiteSpace"));
                return false;
            }
            if (_pictures.Count(x => !x.IsVideo && !x.IsGifAnimation) > 4 ||
                _pictures.Count(x => x.IsVideo || x.IsGifAnimation) > 1 ||
                _pictures.Count(x => !x.IsVideo && x.IsGifAnimation) > 1)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("TweetArea_Message_TwitterMediaOverCapacity"));
                return false;
            }
            if (_pictures.Any(x => x.IsVideo || x.IsGifAnimation) &&
                _pictures.Any(x => !x.IsVideo && !x.IsGifAnimation))
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("TweetArea_Message_TwitterMediaOverCapacity"));
                return false;
            }
            if (_pictures.Where(x => !x.IsVideo).Any(x => x.Stream.Size > 3145728) || _pictures.Where(x => x.IsVideo)
                    .Any(x => x.Stream.Size > 536870912))
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("TweetArea_Message_MediaSizeOver"));
                return false;
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
                                    progressPercentage = e.BytesSent / (double) pic.Stream.Size * 0.5 >= 0.5
                                        ? 0.5
                                        : e.BytesSent / (double) pic.Stream.Size * 0.5;
                                else
                                    progressPercentage = 0.0;

                                progressPercentage *= 100.0;
                            };

                            pic.Stream.Seek(0);
                            resultList.Add(await tokens.Media.UploadChunkedAsync(pic.Stream.AsStream(),
                                Apis.Wrapper.Media.UploadMediaTypeEnum.Video, "tweet_video", progress: progress));
                        }
                        else
                        {
                            foreach (var item in _pictures.Select((v, i) => new {v, i}))
                            {
                                var progress = new Progress<CoreTweet.UploadProgressInfo>();
                                progress.ProgressChanged += (s, e) =>
                                {
                                    var progressPercentage = (item.i / (double) _pictures.Count +
                                                              (e.BytesSent / (double) item.v.Stream.Size > 1.0
                                                                  ? 1.0
                                                                  : e.BytesSent / (double) item.v.Stream.Size) /
                                                              _pictures.Count) * 100.0;
                                };

                                var pic = item.v;
                                pic.Stream.Seek(0);
                                resultList.Add(
                                    await tokens.Media.UploadAsync(pic.Stream.AsStream(), progress: progress));
                            }
                        }

                        param.Add("media_ids", resultList);
                        if (account.AccountSetting.PossiblySensitive)
                            param.Add("possibly_sensitive", true);
                    }

                    param.Add("status", text.Replace("\r", "\n"));
                    if (account.AccountSetting.Platform == SettingSupport.PlatformEnum.Mastodon)
                    {
                        param.Add("visibility", account.AccountSetting.StatusPrivacy.ToString().ToLower());
                        if (IsContentWarning == true && !string.IsNullOrWhiteSpace(ContentWarningText))
                            param.Add("spoiler_text", ContentWarningText.Replace("\r", "\n"));
                    }

                    var status = await tokens.Statuses.UpdateAsync(param);

                    if (account.ReadOnlyColumns.Any(x =>
                            x.Action == SettingSupport.ColumnTypeEnum.Home && x.Streaming) &&
                        account.AccountSetting.Platform == SettingSupport.PlatformEnum.Twitter)
                    {
                        var paramList = new List<string>();
                        paramList.Add("home://");
                        paramList.Add("filter://");
                        if (status.Entities.UserMentions != null &&
                            status.Entities.UserMentions.Any(x => x.Id == account.AccountSetting.UserId))
                            paramList.Add("mentions://");
                        else if (SettingService.Setting.ShowRetweetInMentionColumn &&
                                 status.HasRetweetInformation && status.User.Id == account.AccountSetting.UserId)
                            paramList.Add("mentions://");
                        Connecter.Instance.TweetReceive_OnCommandExecute(this,
                            new TweetEventArgs(status, account.AccountSetting.UserId, account.AccountSetting.Instance,
                                paramList, true));
                    }
                }
                catch (CoreTweet.TwitterException ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("TweetArea_Message_Error"), ex.Errors.First().Message);
                    return false;
                }
                catch (TootNet.Exception.MastodonException ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("TweetArea_Message_Error"), ex.Message);
                    return false;
                }
                catch (NotImplementedException ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_NotImplementedException"),
                        _resourceLoader.GetString("Notification_System_NotImplementedException"));
                    return false;
                }
                catch (Exception ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                        ex.ToString());
                    return false;
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

            ContentWarningText = "";

            return true;
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
                                        .ScreenNameObjects.Where(x => x.Key.StartsWith(token.Value))
                                        .OrderBy(x => x.Key).Select(x => x.Key));
                                }
                            }
                        }
                        break;
                    case SuggestionService.SuggestionToken.SuggestionTokenId.Emoji:
                        if (!string.IsNullOrEmpty(token.Value))
                        {
                            words.AddRange(EmojiPatterns.EmojiDictionary
                                .Where(x => x.Key.StartsWith(token.Value))
                                .OrderBy(x => x.Key)
                                .Select(x => x.Value + "\t" + x.Key));
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
            var text = Text;

            var startText = text.Substring(0, token.Pos);
            var endText = text
                .Substring(token.Pos + token.Length, text.Length - (token.Pos + token.Length));

            switch (token.Type)
            {
                case SuggestionService.SuggestionToken.SuggestionTokenId.HashTag:
                    text = startText + "#" + word + " " + endText;
                    break;
                case SuggestionService.SuggestionToken.SuggestionTokenId.ScreenName:
                    text = startText + "@" + word + " " + endText;
                    break;
                case SuggestionService.SuggestionToken.SuggestionTokenId.Emoji:
                    text = startText + word.Split('\t').First().Trim() + endText;
                    break;
                default:
                    text = startText + endText;
                    break;
            }


            Text = text;
            SelectionStart = text.Length - endText.Length;
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

        #region TwitterCharacterCount変更通知プロパティ

        private int _twitterCharacterCount;

        public int TwitterCharacterCount
        {
            get => _twitterCharacterCount;
            set => SetProperty(ref _twitterCharacterCount, value);
        }

        #endregion

        #region MastodonCharacterCount変更通知プロパティ

        private int _mastodonCharacterCount;

        public int MastodonCharacterCount
        {
            get => _mastodonCharacterCount;
            set => SetProperty(ref _mastodonCharacterCount, value);
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
        
        #region Pictures変更通知プロパティ

        private readonly ObservableCollection<PictureModel> _pictures;
        private ReadOnlyObservableCollection<PictureModel> _readonlyPictures;

        public ReadOnlyObservableCollection<PictureModel> ReadonlyPictures
        {
            get => _readonlyPictures;
            set => SetProperty(ref _readonlyPictures, value);
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

        #region IsContentWarning変更通知プロパティ

        private bool? _isContentWarning;

        public bool? IsContentWarning
        {
            get { return _isContentWarning; }
            set
            {
                if (_isContentWarning != value)
                {
                    _isContentWarning = value;
                    RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region ContentWarningText変更通知プロパティ

        private string _contentWarningText;

        public string ContentWarningText
        {
            get => _contentWarningText;
            set => SetProperty(ref _contentWarningText, value);
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