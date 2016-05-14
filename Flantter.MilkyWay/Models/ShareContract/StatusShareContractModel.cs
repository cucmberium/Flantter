using CoreTweet;
using Flantter.MilkyWay.Models.Twitter;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Flantter.MilkyWay.Models.ShareContract
{
    public class StatusShareContractModel : BindableBase
    {
        public const int MaxTweetLength = 140;

        public StatusShareContractModel()
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

                    this.CharacterCountChanged();
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

        public void CharacterCountChanged()
        {
            var text = this._Text.Replace("\r\n", "\n");
            var result = this._Extractor.ExtractUrls(text);
            var length = text.Count(x => !char.IsLowSurrogate(x)) - result.Sum(x => x.Length) + 23 * result.Count;
            
            if (this._Pictures.Count > 0)
                length += 24;

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
        
        public void DeletePicture(PictureModel picture)
        {
            this._Pictures.Remove(picture);
            picture.Dispose();
            CharacterCountChanged();
        }

        public async Task<bool> Tweet(AccountSetting account)
        {
            if (this.Updating)
                return false;

            if (this.CharacterCount < 0)
            {
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_Over140Character");
                return false;
            }
            else if (this._Pictures.Count == 0 && string.IsNullOrWhiteSpace(this.Text))
            {
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_TextIsEmptyOrWhiteSpace");
                return false;
            }
            else if (this._Pictures.Where(x => !x.IsVideo && !x.IsGifAnimation).Count() > 4 || this._Pictures.Where(x => x.IsVideo || x.IsGifAnimation).Count() > 1 || this._Pictures.Where(x => !x.IsVideo && x.IsGifAnimation).Count() > 1)
            {
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_TwitterMediaOverCapacity");
                return false;
            }
            else if (this._Pictures.Where(x => x.IsVideo || x.IsGifAnimation).Count() > 0 && this._Pictures.Where(x => !x.IsVideo && !x.IsGifAnimation).Count() > 0)
            {
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_TwitterMediaOverCapacity");
                return false;
            }
            else if (this._Pictures.Where(x => !x.IsVideo).Any(x => x.Stream.Size > 3145728) || this._Pictures.Where(x => x.IsVideo).Any(x => x.Stream.Size > 15728640))
            {
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_Error");
                return false;
            }

            this.Updating = true;

            var tokens = Tokens.Create(account.ConsumerKey, account.ConsumerSecret, account.AccessToken, account.AccessTokenSecret, account.UserId, account.ScreenName);
            tokens.ConnectionOptions.UserAgent = TwitterConnectionHelper.GetUserAgent(tokens);
            
            var text = this.Text;

            try
            {
                var param = new Dictionary<string, object>() { };

                // Upload Media

                if (this._Pictures.Count > 0)
                {
                    this.Message = _ResourceLoader.GetString("TweetArea_Message_UploadingMedia") + " , " + "0.0%";

                    var resultList = new List<MediaUploadResult>();

                    foreach (var item in this._Pictures.Select((v, i) => new { v, i }))
                    {
                        var progress = new Progress<UploadProgressInfo>();
                        progress.ProgressChanged += (s, e) =>
                        {
                            var progressPercentage = (item.i / (double)this._Pictures.Count + (((e.BytesSent / (double)item.v.Stream.Size) > 1.0 ? 1.0 : (e.BytesSent / (double)item.v.Stream.Size)) / this._Pictures.Count)) * 100.0;

                            this.Message = _ResourceLoader.GetString("TweetArea_Message_UploadingMedia") + " , " + progressPercentage.ToString("#0.0") + "%";
                        };

                        var pic = item.v;
                        pic.Stream.Seek(0);
                        if (pic.IsVideo)
                            resultList.Add(await tokens.Media.UploadChunkedAsync(pic.Stream.AsStream(), UploadMediaType.Video, (IEnumerable<long>)null, default(System.Threading.CancellationToken), progress));
                        else
                            resultList.Add(await tokens.Media.UploadAsync(pic.Stream.AsStream(), (IEnumerable<long>)null, default(System.Threading.CancellationToken), progress));
                    }

                    param.Add("media_ids", resultList.Select(x => x.MediaId));
                    param.Add("possibly_sensitive", account.PossiblySensitive);
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
                return false;
            }
            catch (Exception ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_Error");
                return false;
            }
            finally
            {
                this.Updating = false;
            }

            foreach (var pic in this._Pictures)
                pic.Dispose();

            this._Pictures.Clear();

            this.Text = string.Empty;
            
            this.State = "Accept";
            this.Message = _ResourceLoader.GetString("TweetArea_Message_AllSet");

            return true;
        }
    }
}
