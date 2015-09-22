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

namespace Flantter.MilkyWay.Models
{
    public class TweetAreaModel : BindableBase
    {
        ResourceLoader _ResourceLoader;

        public TweetAreaModel()
        {
            this._ResourceLoader = new ResourceLoader();

            this._Pictures = new ObservableCollection<PictureModel>();
            this._ReadonlyPictures = new ReadOnlyObservableCollection<PictureModel>(this._Pictures);
            this._Text = string.Empty;
            this.CharacterCount = 140;
            this.State = "Accept";
            this.Message = _ResourceLoader.GetString("TweetArea_Message_AllSet");
        }

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
            StringInfo textInfo = new StringInfo(this._Text.Replace("\r\n", "\n"));
            int count = 140 - textInfo.LengthInTextElements;

            foreach (Match m in TweetRegexPatterns.ValidUrl.Matches(this._Text))
                count += m.Value.Length - (m.Value.ToLower().StartsWith("https://") ? 23 : 22);

            if (this._Pictures.Count > 0)
                count -= 23;

            if (this._IsQuotedRetweet)
                count -= 23;

            this.CharacterCount = count;
        }

        public async void AddPicture(StorageFile picture)
        {
            this._Pictures.Add(new PictureModel() { Stream = await RandomAccessStreamReference.CreateFromFile(picture).OpenReadAsync(), IsVideo = (picture.FileType == ".mp4" || picture.FileType == ".mov") });
            CharacterCountChanged();
        }

        public void DeletePicture(PictureModel picture)
        {
            this._Pictures.Remove(picture);
            picture.Dispose();
            CharacterCountChanged();
        }

        public async void Tweet(AccountModel account)
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
            else if (this._Pictures.Where(x => !x.IsVideo).Count() > 4 || this._Pictures.Where(x => x.IsVideo).Count() > 1)
            {
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_TwitterMediaOverCapacity");
                return;
            }
            else if (this._Pictures.Where(x => x.IsVideo).Count() > 0 && this._Pictures.Where(x => !x.IsVideo).Count() > 0)
            {
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_TwitterMediaOverCapacity");
                return;
            }

            this.Updating = true;
            var tokens = account._Tokens;
            var text = this.Text;

            try
            {
                var param = new Dictionary<string, object>() { };
                if (this.ReplyOrQuotedStatus != null)
                    param.Add("in_reply_to_status_id", this.ReplyOrQuotedStatus.Id);
                if (this._IsQuotedRetweet)
                    text += " https://twitter.com/" + this.ReplyOrQuotedStatus.User.ScreenName + "/status/" + this.ReplyOrQuotedStatus.Id;

                // Upload Media

                if (this._Pictures.Count > 0)
                {
                    this.Message = _ResourceLoader.GetString("TweetArea_Message_UploadingImage");

                    var taskList = new List<Task<MediaUploadResult>>();

                    foreach (var pic in this._Pictures.Where(x => !x.IsVideo))
                    {
                        var st = pic.Stream;
                        st.Seek(0);
                        if (st.Size > 3145728)
                            throw new NotImplementedException("Image size is too big."); // Todo : Exceptionをそれ専用のものにする
                        taskList.Add(tokens.Media.UploadAsync(media => st.AsStream()));
                    }
                    foreach (var pic in this._Pictures.Where(x => x.IsVideo))
                    {
                        var st = pic.Stream;
                        st.Seek(0);
                        taskList.Add(tokens.Media.UploadChunkedAsync(st.AsStream(), UploadMediaType.Video));
                    }

                    MediaUploadResult[] result = await Task.WhenAll(taskList);

                    param.Add("media_ids", result.Select(x => x.MediaId));
                    param.Add("possibly_sensitive", account.PossiblySensitive);
                }

                param.Add("status", text);

                this.Message = _ResourceLoader.GetString("TweetArea_Message_UpdatingStatus");
                await tokens.Statuses.UpdateAsync(param);
            }
            catch (TwitterException ex)
            {
                // Todo : 通知システムに渡す
                this.State = "Cancel";
                this.Message = _ResourceLoader.GetString("TweetArea_Message_Error");
                return;
            }
            catch (Exception ex)
            {
                // Todo : 通知システムに渡す
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

            this.Text = string.Empty;

            this.ReplyOrQuotedStatus = null;
            this.IsQuotedRetweet = false;
            this.IsReply = false;

            this.State = "Accept";
            this.Message = _ResourceLoader.GetString("TweetArea_Message_AllSet");

            // Todo : フォーカスをテキストボックスに戻す or ツイート部分を閉じる (設定によって変える)
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
            if (tokens == null)
                return;

            if (!Services.Connecter.Instance.TweetCollecter.ContainsKey(this.SelectedAccountUserId))
                return;

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

        public void Dispose()
        {
            if (this._Stream != null)
            {
                var stream = this._Stream;
                this._Stream = null;
                stream.Dispose();
            }
                
        }
    }
}
