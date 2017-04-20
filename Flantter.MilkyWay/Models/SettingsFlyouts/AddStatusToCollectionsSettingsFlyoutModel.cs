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
    public class AddStatusToCollectionSettingsFlyoutModel : BindableBase
    {
        private string _userCollectionsCursor = "";

        public AddStatusToCollectionSettingsFlyoutModel()
        {
            UserCollections = new ObservableCollection<Collection>();
        }

        public ObservableCollection<Collection> UserCollections { get; set; }

        public async Task UpdateUserCollections(bool useCursor = false)
        {
            if (UpdatingUserCollections)
                return;

            if (_screenName == null || Tokens == null)
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
                    {"screen_name", _screenName},
                    {"count", 20}
                };
                if (useCursor && !string.IsNullOrWhiteSpace(_userCollectionsCursor))
                    param.Add("cursor", _userCollectionsCursor);

                var userCollections = await Tokens.Collections.ListAsync(param);
                if (!useCursor || string.IsNullOrWhiteSpace(_userCollectionsCursor))
                    UserCollections.Clear();

                foreach (var item in userCollections)
                {
                    var list = item;
                    UserCollections.Add(list);
                }

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

        public async Task AddStatusToCollection(string collectionId, long statusId)
        {
            if (_screenName == null || Tokens == null)
                return;

            try
            {
                await Tokens.Collections.EntriesAddAsync(id => collectionId, tweet_id => statusId);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    new ResourceLoader().GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    new ResourceLoader().GetString("Notification_System_NotImplementedException"),
                    new ResourceLoader().GetString("Notification_System_NotImplementedException"));
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    new ResourceLoader().GetString("Notification_System_ErrorOccurred"),
                    new ResourceLoader().GetString("Notification_System_CheckNetwork"));
            }
        }

        #region Tokens変更通知プロパティ

        private Tokens _tokens;

        public Tokens Tokens
        {
            get => _tokens;
            set => SetProperty(ref _tokens, value);
        }

        #endregion

        #region ScreenName変更通知プロパティ

        private string _screenName;

        public string ScreenName
        {
            get => _screenName;
            set => SetProperty(ref _screenName, value);
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
    }
}