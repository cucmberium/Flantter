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
    public class ListMembersSettingsFlyoutModel : BindableBase
    {
        private readonly ResourceLoader _resourceLoader;

        private long _listMembersCursor;

        public ListMembersSettingsFlyoutModel()
        {
            _resourceLoader = new ResourceLoader();

            ListMembers = new ObservableCollection<User>();
        }

        public ObservableCollection<User> ListMembers { get; set; }

        public async Task UpdateListMembers(bool useCursor = false)
        {
            if (Updating)
                return;

            if (_id == 0 || Tokens == null)
                return;

            if (useCursor && _listMembersCursor == 0)
                return;

            Updating = true;

            if (!useCursor || _listMembersCursor == 0)
                ListMembers.Clear();

            try
            {
                var param = new Dictionary<string, object>
                {
                    {"list_id", _id},
                    {"count", 20}
                };
                if (useCursor && _listMembersCursor != 0)
                    param.Add("cursor", _listMembersCursor);

                var listMembers = await Tokens.Lists.Members.ListAsync(param);
                if (!useCursor || _listMembersCursor == 0)
                    ListMembers.Clear();

                foreach (var user in listMembers)
                    ListMembers.Add(user);

                _listMembersCursor = listMembers.NextCursor;
            }
            catch
            {
                if (!useCursor || _listMembersCursor == 0)
                    ListMembers.Clear();
            }

            Updating = false;
        }

        public async Task<bool> DeleteUser(long cid)
        {
            if (_id == 0 || Tokens == null)
                return false;
            
            try
            {
                await Tokens.Lists.Members.DestroyAsync(list_id => _id, user_id => cid);
            }
            catch (CoreTweet.TwitterException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Errors.First().Message);
                return false;
            }
            catch (TootNet.Exception.MastodonException ex)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"), ex.Message);
                return false;
            }
            catch (NotImplementedException e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_NotImplementedException"),
                    _resourceLoader.GetString("Notification_System_NotImplementedException"));
                return false;
            }
            catch (Exception e)
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System,
                    _resourceLoader.GetString("Notification_System_ErrorOccurred"),
                    e.ToString());
                return false;
            }
            
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

        #region Id変更通知プロパティ

        private long _id;

        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
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