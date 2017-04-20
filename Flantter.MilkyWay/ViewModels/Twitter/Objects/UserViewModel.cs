using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class UserViewModel : ExtendedBindableBase
    {
        public UserViewModel(User user)
        {
            Model = user;

            Description = user.Description;
            Entities = user.Entities;

            IsMuting = user.IsMuting;
            IsProtected = user.IsProtected;
            Name = user.Name;
            ProfileImageUrl = string.IsNullOrWhiteSpace(user.ProfileImageUrl)
                ? "http://localhost/"
                : user.ProfileImageUrl;
            ScreenName = user.ScreenName;
            Url = user.Url;

            Notice = Notice.Instance;
            Setting = SettingService.Setting;
        }

        public User Model { get; }

        #region Description変更通知プロパティ

        public string Description { get; set; }

        #endregion

        #region Entities変更通知プロパティ

        public UserEntities Entities { get; set; }

        #endregion

        #region IsMuting変更通知プロパティ

        public bool IsMuting { get; set; }

        #endregion

        #region IsProtected変更通知プロパティ

        public bool IsProtected { get; set; }

        #endregion

        #region Name変更通知プロパティ

        public string Name { get; set; }

        #endregion

        #region ProfileImageUrl変更通知プロパティ

        public string ProfileImageUrl { get; set; }

        #endregion

        #region ScreenName変更通知プロパティ

        public string ScreenName { get; set; }

        #endregion

        #region Url変更通知プロパティ

        public string Url { get; set; }

        #endregion

        public Notice Notice { get; set; }

        public SettingService Setting { get; set; }
    }
}