using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Models.Apis.Wrapper;
using Flantter.MilkyWay.Models.Notifications;
using Prism.Mvvm;

namespace Flantter.MilkyWay.Models.SettingsFlyouts
{
    public class UserCollectionsSettingsFlyoutModel : BindableBase
    {
        private readonly ResourceLoader _resourceLoader;
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
            if (CreatingOrUpdatingCollection)
                return false;

            if (_userId == 0 || Tokens == null)
                return false;

            if (_userId != Tokens.UserId)
                return false;

            CreatingOrUpdatingCollection = true;

            try
            {
                await Tokens.Collections.CreateAsync(name => cname, description => cdescription, url => curl);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                CreatingOrUpdatingCollection = false;
                return false;
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
                CreatingOrUpdatingCollection = false;
                return false;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
                CreatingOrUpdatingCollection = false;
                return false;
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    e.ToString());
                CreatingOrUpdatingCollection = false;
                return false;
            }

            CreatingOrUpdatingCollection = false;
            return true;
        }

        public async Task<bool> UpdateCollection(string cid, string cname, string cdescription, string curl)
        {
            if (CreatingOrUpdatingCollection)
                return false;

            if (_userId == 0 || Tokens == null)
                return false;

            if (_userId != Tokens.UserId)
                return false;

            CreatingOrUpdatingCollection = true;

            try
            {
                await Tokens.Collections.UpdateAsync(id => cid, name => cname, description => cdescription,
                    url => curl);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                CreatingOrUpdatingCollection = false;
                return false;
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
                CreatingOrUpdatingCollection = false;
                return false;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
                CreatingOrUpdatingCollection = false;
                return false;
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    e.ToString());
                CreatingOrUpdatingCollection = false;
                return false;
            }

            CreatingOrUpdatingCollection = false;
            return true;
        }

        public async Task<bool> DeleteCollection(string cid)
        {
            if (CreatingOrUpdatingCollection)
                return false;

            if (_userId == 0 || Tokens == null)
                return false;

            if (_userId != Tokens.UserId)
                return false;

            CreatingOrUpdatingCollection = true;

            try
            {
                await Tokens.Collections.DestroyAsync(id => cid);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                CreatingOrUpdatingCollection = false;
                return false;
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
                CreatingOrUpdatingCollection = false;
                return false;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
                CreatingOrUpdatingCollection = false;
                return false;
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    e.ToString());
                CreatingOrUpdatingCollection = false;
                return false;
            }

            CreatingOrUpdatingCollection = false;
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

        #region CreatingOrUpdatingCollection変更通知プロパティ

        private bool _CreatingOrUpdatingCollection;

        public bool CreatingOrUpdatingCollection
        {
            get => _CreatingOrUpdatingCollection;
            set => SetProperty(ref _CreatingOrUpdatingCollection, value);
        }

        #endregion
    }
}