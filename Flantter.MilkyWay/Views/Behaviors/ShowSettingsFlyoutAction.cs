using CoreTweet;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Views.Contents;
using Flantter.MilkyWay.Views.Contents.SettingsFlyout;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class ShowSettingsFlyoutAction : DependencyObject, IAction
    {
        private List<ExtendedSettingsFlyout> _SettingsFlyoutList = null;

        public ShowSettingsFlyoutAction()
        {
            this._SettingsFlyoutList = new List<ExtendedSettingsFlyout>();
        }

        public object Execute(object sender, object parameter)
        {
            var notification = parameter as ShowSettingsFlyoutNotification;
            if (notification == null)
                return null;

            ExtendedSettingsFlyout settingsFlyout = null;
            IEnumerable<ExtendedSettingsFlyout> settingsFlyoutList = null;

            switch (notification.SettingsFlyoutType)
            {
                case "UserProfile":
                    settingsFlyoutList = _SettingsFlyoutList.Where(x => x is UserProfileSettingsFlyout && !x.IsOpen);
                    if (settingsFlyoutList.Count() > 0)
                    {
                        settingsFlyout = settingsFlyoutList.First();
                    }
                    else
                    {
                        settingsFlyout = new UserProfileSettingsFlyout();
                        this._SettingsFlyoutList.Add(settingsFlyout);
                    }

                    settingsFlyout.Show();
                    break;
            }

            return null;
        }
    }
    public class ShowSettingsFlyoutNotification : Notification
    {
        /// <summary>
        /// SettingsFlyoutの種類
        /// </summary>
        public string SettingsFlyoutType { get; set; }

        /// <summary>
        /// CoreTweetのTokens
        /// </summary>
        public Tokens Tokens { get; set; }
    }

}
