using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.Notifications;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;
using ToriatamaText;

namespace Flantter.MilkyWay.Models.ShareContract
{
    public class StatusShareContractModel : BindableBase
    {
        public const int MaxTweetLength = 140;
        private readonly Extractor _extractor;

        private readonly ResourceLoader _resourceLoader;

        public StatusShareContractModel()
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
            var text = _text;

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

            var property = await picture.GetBasicPropertiesAsync();

            if (SettingService.Setting.ConvertPostingImage &&
                (picture.FileType == ".jpeg" || picture.FileType == ".jpg" || picture.FileType == ".png"))
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

                        picEncoder.SetPixelData(picDecoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied,
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
                    var memoryStream = new InMemoryRandomAccessStream();
                    using (IRandomAccessStream fileStream =
                        await RandomAccessStreamReference.CreateFromFile(picture).OpenReadAsync())
                    {
                        var picDecoder = await BitmapDecoder.CreateAsync(fileStream);
                        var picDecoderPixels = await picDecoder.GetPixelDataAsync();
                        var picEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, memoryStream);
                        picEncoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Cubic;

                        var data = picDecoderPixels.DetachPixelData();
                        picEncoder.SetPixelData(picDecoder.BitmapPixelFormat, BitmapAlphaMode.Premultiplied,
                            picDecoder.PixelWidth, picDecoder.PixelHeight, picDecoder.DpiX, picDecoder.DpiY, data);

                        await picEncoder.FlushAsync();

                        var scale = 1.0;
                        while (memoryStream.Size > 3145728)
                        {
                            scale -= 0.05;
                            picEncoder.BitmapTransform.ScaledHeight = (uint) (picDecoder.PixelHeight * scale);
                            picEncoder.BitmapTransform.ScaledWidth = (uint) (picDecoder.PixelWidth * scale);

                            await picEncoder.FlushAsync();
                        }
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

            CharacterCountChanged();
        }

        public void DeletePicture(PictureModel picture)
        {
            _pictures.Remove(picture);
            picture.Dispose();
            CharacterCountChanged();
        }

        public async Task<bool> Tweet(IEnumerable<AccountSetting> accounts)
        {
            if (!accounts.Any())
                return false;

            if (Updating)
                return false;

            ToolTipIsOpen = true;

            if (CharacterCount < 0)
            {
                State = "Cancel";
                Message = _resourceLoader.GetString("TweetArea_Message_Over140Character");
                return false;
            }
            if (_pictures.Count == 0 && string.IsNullOrWhiteSpace(Text))
            {
                State = "Cancel";
                Message = _resourceLoader.GetString("TweetArea_Message_TextIsEmptyOrWhiteSpace");
                return false;
            }
            if (_pictures.Count(x => !x.IsVideo && !x.IsGifAnimation) > 4 ||
                _pictures.Count(x => x.IsVideo || x.IsGifAnimation) > 1 ||
                _pictures.Count(x => !x.IsVideo && x.IsGifAnimation) > 1)
            {
                State = "Cancel";
                Message = _resourceLoader.GetString("TweetArea_Message_TwitterMediaOverCapacity");
                return false;
            }
            if (_pictures.Any(x => x.IsVideo || x.IsGifAnimation) &&
                _pictures.Any(x => !x.IsVideo && !x.IsGifAnimation))
            {
                State = "Cancel";
                Message = _resourceLoader.GetString("TweetArea_Message_TwitterMediaOverCapacity");
                return false;
            }
            if (_pictures.Where(x => !x.IsVideo).Any(x => x.Stream.Size > 3145728) || _pictures.Where(x => x.IsVideo)
                    .Any(x => x.Stream.Size > 536870912))
            {
                State = "Cancel";
                Message = _resourceLoader.GetString("TweetArea_Message_MediaSizeOver");
                return false;
            }

            Updating = true;

            foreach (var account in accounts)
            {
                var tokens = Tokens.Create(account.ConsumerKey, account.ConsumerSecret, account.AccessToken,
                    account.AccessTokenSecret, account.UserId, account.ScreenName, account.Instance);
                var text = Text;

                try
                {
                    var param = new Dictionary<string, object>();
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
                                    progressPercentage = e.BytesSent / (double) pic.Stream.Size * 0.5 >= 0.5
                                        ? 0.5
                                        : e.BytesSent / (double) pic.Stream.Size * 0.5;
                                else
                                    progressPercentage = 0.0;

                                progressPercentage *= 100.0;
                                Message = _resourceLoader.GetString("TweetArea_Message_UploadingMedia") + " , " +
                                          progressPercentage.ToString("#0.0") + "%";
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
                                    Message = _resourceLoader.GetString("TweetArea_Message_UploadingMedia") + " , " +
                                              progressPercentage.ToString("#0.0") + "%";
                                };

                                var pic = item.v;
                                pic.Stream.Seek(0);
                                resultList.Add(
                                    await tokens.Media.UploadAsync(pic.Stream.AsStream(), progress: progress));
                            }
                        }

                        param.Add("media_ids", resultList);
                        param.Add("possibly_sensitive", account.PossiblySensitive);
                    }

                    param.Add("status", text.Replace("\r", "\n"));

                    Message = _resourceLoader.GetString("TweetArea_Message_UpdatingStatus");
                    await tokens.Statuses.UpdateAsync(param);
                }
                catch (CoreTweet.TwitterException ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                    State = "Cancel";
                    Message = _resourceLoader.GetString("TweetArea_Message_Error");
                    return false;
                }
                catch (TootNet.Exception.MastodonException ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
                    State = "Cancel";
                    Message = _resourceLoader.GetString("TweetArea_Message_Error");
                    return false;
                }
                catch (NotImplementedException ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_NotImplementedException"),
                        _resourceLoader.GetString("Notification_System_NotImplementedException"));
                    State = "Cancel";
                    Message = _resourceLoader.GetString("TweetArea_Message_Error");
                    return false;
                }
                catch (Exception ex)
                {
                    Core.Instance.PopupToastNotification(PopupNotificationType.System,
                        _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                        ex.ToString());
                    State = "Cancel";
                    Message = _resourceLoader.GetString("TweetArea_Message_Error");
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

            Text = string.Empty;

            State = "Accept";
            Message = _resourceLoader.GetString("TweetArea_Message_AllSet");
            ToolTipIsOpen = false;

            return true;
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

                    CharacterCountChanged();
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
    }
}