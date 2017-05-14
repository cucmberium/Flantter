using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Notifications;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Models.Twitter.Wrapper;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class UserCollectionsSettingsFlyoutModel : BindableBase
    {
        private ResourceLoader _resourceLoader;
        private string _userCollectionsCursor = "";

        public UserCollectionsSettingsFlyoutModel()
        {
            _resourceLoader = new ResourceLoader();

            UserCollections = new ObservableCollection<Collection>();
        }

        public ObservableCollection<Collection> UserCollections { get; set; }

        public async Task UpdateUserCollections(bool useCursor = false)
        {
            if (UpdatingUserCollections)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            if (useCursor && string.IsNullOrWhiteSpace(_userCollectionsCursor))
                return;

            UpdatingUserCollections = true;

            if (!useCursor || string.IsNullOrWhiteSpace(_userCollectionsCursor))
                UserCollections.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"user_id", _userId},
                    {"count", 20}
                };
                if (useCursor && !string.IsNullOrWhiteSpace(_userCollectionsCursor))
                    param.Add("cursor", _userCollectionsCursor);
                var userCollections = await Tokens.Collections.ListAsync(param);

                if (!useCursor || string.IsNullOrWhiteSpace(_userCollectionsCursor))
                    UserCollections.Clear();

                foreach (var list in userCollections)
                    UserCollections.Add(list);

                _userCollectionsCursor = userCollections.NextCursor;
            }
            catch
            {
                if (!useCursor || string.IsNullOrWhiteSpace(_userCollectionsCursor))
                    UserCollections.Clear();

                UpdatingUserCollections = false;
                return;
            }

            UpdatingUserCollections = false;
        }

        public async Task<bool> CreateCollection(string cname, string cdescription, string curl)
        {
            if (CreatingCollection)
                return false;

            if (_userId == 0 || Tokens == null)
                return false;

            if (_userId != Tokens.UserId)
                return false;

            CreatingCollection = true;

            try
            {
                await Tokens.Collections.CreateAsync(name => cname, description => cdescription, url => curl);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                CreatingCollection = false;
                return false;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
                CreatingCollection = false;
                return false;
            }

            CreatingCollection = false;
            return true;
        }

        public async Task<bool> UpdateCollection(string cid, string cname, string cdescription, string curl)
        {
            if (CreatingCollection)
                return false;

            if (_userId == 0 || Tokens == null)
                return false;

            if (_userId != Tokens.UserId)
                return false;

            CreatingCollection = true;

            try
            {
                await Tokens.Collections.UpdateAsync(id => cid, name => cname, description => cdescription,
                    url => curl);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                CreatingCollection = false;
                return false;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
                CreatingCollection = false;
                return false;
            }

            CreatingCollection = false;
            return true;
        }

        public async Task<bool> DeleteCollection(string cid)
        {
            if (CreatingCollection)
                return false;

            if (_userId == 0 || Tokens == null)
                return false;

            if (_userId != Tokens.UserId)
                return false;

            CreatingCollection = true;

            try
            {
                await Tokens.Collections.DestroyAsync(id => cid);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                CreatingCollection = false;
                return false;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    _resourceLoader.GetString("Notification_System_CheckNetwork"));
                CreatingCollection = false;
                return false;
            }

            CreatingCollection = false;
            return true;
        }

        #region Tokens変更通知プロパティ

        private Tokens _tokens;

        public Tokens Tokens
        {
            get => _tokens;
            set => SetProperty(ref _tokens, value);
        }

        #endregion

        #region UserId変更通知プロパティ

        private long _userId;

        public long UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value);
        }

        #endregion

        #region UpdatingUserCollections変更通知プロパティ

        private bool _updatingUserCollections;

        public bool UpdatingUserCollections
        {
            get => _updatingUserCollections;
            set => SetProperty(ref _updatingUserCollections, value);
        }

        #endregion

        #region CreatingCollection変更通知プロパティ

        private bool _creatingCollection;

        public bool CreatingCollection
        {
            get => _creatingCollection;
            set => SetProperty(ref _creatingCollection, value);
        }

        #endregion
    }
}