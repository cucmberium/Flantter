using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class UserViewModel : ExtendedBindableBase
    {
        public UserViewModel(User user)
        {
            this.Model = user;

            this.BackgroundBrush = "Default";

            this.Description = user.Description;
            this.Entities = user.Entities;

            this.IsMuting = user.IsMuting;
            this.IsProtected = user.IsProtected;
            this.IsVerified = user.IsVerified;
            this.Name = user.Name;
            this.ProfileImageUrl = string.IsNullOrWhiteSpace(user.ProfileImageUrl) ? "http://localhost/" : user.ProfileImageUrl;
            this.ScreenName = user.ScreenName;
            this.Url = user.Url;

            this.Notice = Services.Notice.Instance;
            this.Setting = SettingService.Setting;
        }

        public User Model { get; private set; }

        public string BackgroundBrush { get; set; }

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

        #region IsVerified変更通知プロパティ
        public bool IsVerified { get; set; }
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

        public Services.Notice Notice { get; set; }

        public Setting.SettingService Setting { get; set; }
    }
}
