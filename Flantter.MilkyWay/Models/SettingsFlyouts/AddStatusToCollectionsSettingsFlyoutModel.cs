using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Services;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Flantter.MilkyWay.Setting;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class AddStatusToCollectionSettingsFlyoutModel : BindableBase
    {
        public AddStatusToCollectionSettingsFlyoutModel()
        {
            this.UserCollections = new ObservableCollection<Twitter.Objects.Collection>();
        }

        #region Tokens変更通知プロパティ
        private Tokens _Tokens;
        public Tokens Tokens
        {
            get { return this._Tokens; }
            set { this.SetProperty(ref this._Tokens, value); }
        }
        #endregion
        
        #region ScreenName変更通知プロパティ
        private string _ScreenName;
        public string ScreenName
        {
            get { return this._ScreenName; }
            set { this.SetProperty(ref this._ScreenName, value); }
        }
        #endregion

        #region UpdatingUserCollections変更通知プロパティ
        private bool _UpdatingUserCollections;
        public bool UpdatingUserCollections
        {
            get { return this._UpdatingUserCollections; }
            set { this.SetProperty(ref this._UpdatingUserCollections, value); }
        }
        #endregion

        public ObservableCollection<Twitter.Objects.Collection> UserCollections { get; set; }

        private string userCollectionsCursor = "";
        public async Task UpdateUserCollections(bool useCursor = false)
        {
            if (this.UpdatingUserCollections)
                return;

            if (this._ScreenName == null || this.Tokens == null)
                return;

            if (useCursor && string.IsNullOrWhiteSpace(userCollectionsCursor))
                return;

            this.UpdatingUserCollections = true;

            if (!useCursor || string.IsNullOrWhiteSpace(userCollectionsCursor))
                this.UserCollections.Clear();
            
            try
            {
                var param = new Dictionary<string, object>()
                {
                    {"screen_name", this._ScreenName},
                    {"count", 20},
                };
                if (useCursor && !string.IsNullOrWhiteSpace(userCollectionsCursor))
                    param.Add("cursor", userCollectionsCursor);
                
                var userCollections = await Tokens.Collections.ListAsync(screen_name => this._ScreenName, count => 20, cursor => userCollectionsCursor);
                if (!useCursor || string.IsNullOrWhiteSpace(userCollectionsCursor))
                    this.UserCollections.Clear();

                foreach (var item in userCollections)
                {
                    var list = item;
                    this.UserCollections.Add(list);
                }

                userCollectionsCursor = userCollections.NextCursor;
            }
            catch
            {
                if (!useCursor || string.IsNullOrWhiteSpace(userCollectionsCursor))
                    this.UserCollections.Clear();

                this.UpdatingUserCollections = false;
                return;
            }

            

            this.UpdatingUserCollections = false;
        }

        public async Task AddStatusToCollection(string collectionId, long statusId)
        {
            if (this._ScreenName == null || this.Tokens == null)
                return;

            try
            {
                await this.Tokens.Collections.EntriesAddAsync(id => collectionId, tweet_id => statusId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_NotImplementedException"), new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Notifications.Core.Instance.PopupToastNotification(Notifications.PopupNotificationType.System, new ResourceLoader().GetString("Notification_System_ErrorOccurred"), new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }
        }
    }
}
