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
    public class MastodonUserListsSettingsFlyoutModel : BindableBase
    {
        private readonly ResourceLoader _resourceLoader;
        private long _userListsCursor = 0;

        public MastodonUserListsSettingsFlyoutModel()
        {
            _resourceLoader = new ResourceLoader();

            UserLists = new ObservableCollection<List>();
        }

        public ObservableCollection<List> UserLists { get; set; }

        public async Task UpdateUserLists(bool useCursor = false)
        {
            if (UpdatingUserLists)
                return;

            if (_userId == 0 || Tokens == null)
                return;

            if (useCursor && _userListsCursor == 0)
                return;

            UpdatingUserLists = true;

            if (!useCursor || _userListsCursor == 0)
                UserLists.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"user_id", _userId},
                    {"count", 20}
                };
                if (useCursor && _userListsCursor != 0)
                    param.Add("cursor", _userListsCursor);
                var userLists = await Tokens.Lists.OwnershipsAsync(param);

                if (!useCursor || _userListsCursor == 0)
                    UserLists.Clear();

                foreach (var list in userLists)
                    UserLists.Add(list);

                _userListsCursor = userLists.NextCursor;
            }
            catch
            {
                if (!useCursor || _userListsCursor == 0)
                    UserLists.Clear();

                UpdatingUserLists = false;
                return;
            }

            UpdatingUserLists = false;
        }

        public async Task<bool> CreateList(string cname)
        {
            if (CreatingOrUpdatingList)
                return false;

            if (_userId == 0 || Tokens == null)
                return false;

            if (_userId != Tokens.UserId)
                return false;

            CreatingOrUpdatingList = true;

            try
            {
                await Tokens.Lists.CreateAsync(name => cname);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                CreatingOrUpdatingList = false;
                return false;
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
                CreatingOrUpdatingList = false;
                return false;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
                CreatingOrUpdatingList = false;
                return false;
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    e.ToString());
                CreatingOrUpdatingList = false;
                return false;
            }

            CreatingOrUpdatingList = false;
            return true;
        }

        public async Task<bool> UpdateList(long cid, string cname)
        {
            if (CreatingOrUpdatingList)
                return false;

            if (_userId == 0 || Tokens == null)
                return false;

            if (_userId != Tokens.UserId)
                return false;

            CreatingOrUpdatingList = true;

            try
            {
                await Tokens.Lists.UpdateAsync(id => cid, name => cname);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                CreatingOrUpdatingList = false;
                return false;
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
                CreatingOrUpdatingList = false;
                return false;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
                CreatingOrUpdatingList = false;
                return false;
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    e.ToString());
                CreatingOrUpdatingList = false;
                return false;
            }

            CreatingOrUpdatingList = false;
            return true;
        }

        public async Task<bool> DeleteList(long cid)
        {
            if (CreatingOrUpdatingList)
                return false;

            if (_userId == 0 || Tokens == null)
                return false;

            if (_userId != Tokens.UserId)
                return false;

            CreatingOrUpdatingList = true;

            try
            {
                await Tokens.Lists.DestroyAsync(id => cid);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                CreatingOrUpdatingList = false;
                return false;
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
                CreatingOrUpdatingList = false;
                return false;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
                CreatingOrUpdatingList = false;
                return false;
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    e.ToString());
                CreatingOrUpdatingList = false;
                return false;
            }

            CreatingOrUpdatingList = false;
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

        #region UpdatingUserLists変更通知プロパティ

        private bool _updatingUserLists;

        public bool UpdatingUserLists
        {
            get => _updatingUserLists;
            set => SetProperty(ref _updatingUserLists, value);
        }

        #endregion

        #region CreatingOrUpdatingList変更通知プロパティ

        private bool _CreatingOrUpdatingList;

        public bool CreatingOrUpdatingList
        {
            get => _CreatingOrUpdatingList;
            set => SetProperty(ref _CreatingOrUpdatingList, value);
        }

        #endregion
    }
}