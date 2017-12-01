using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;
using Flantter.MilkyWay.Setting;
using System.Text;
using Windows.Globalization;
using Newtonsoft.Json;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.Models.Notifications
{
    public class Push
    {
        public class Registration
        {
            public string UserUuid { get; set; }
            
            public string Url { get; set; }
        }

        public class User
        {
            public string UserUuid { get; set; }

            public string Content { get; set; }
        }

        public class AccountSetting
        {
            public string ScreenName { get; set; }

            public long UserId { get; set; }

            public string ConsumerKey { get; set; }

            public string ConsumerSecret { get; set; }

            public string AccessToken { get; set; }

            public string AccessTokenSecret { get; set; }

            public string Platform { get; set; }

            public string Instance { get; set; }

            public bool ReplyNotification { get; set; }

            public bool DirectMessageNotification { get; set; }

            public bool RetweetNotification { get; set; }

            public bool FavoriteNotification { get; set; }

            public bool FollowNotification { get; set; }

            public string Language { get; set; }
        }

        private Push()
        {
        }

        public void Initialize()
        {
            SettingService.Setting.ObserveProperty(x => x.PushNotification).Take(1).Subscribe(async pushNotification =>
            {
                if (pushNotification)
                    await Register();
            });

            SettingService.Setting.ObserveProperty(x => x.PushNotification).Skip(1).Subscribe(async pushNotification =>
            {
                if (pushNotification)
                {
                    await Register();
                    await Update();
                }
                else
                {
                    await Unregister();
                }
            });

            SettingService.Setting.ObserveProperty(x => x.FavoriteNotification).Skip(1).Subscribe(async x =>
            {
                if (SettingService.Setting.PushNotification)
                    await Update();
            });

            SettingService.Setting.ObserveProperty(x => x.RetweetNotification).Skip(1).Subscribe(async x =>
            {
                if (SettingService.Setting.PushNotification)
                    await Update();
            });

            SettingService.Setting.ObserveProperty(x => x.MentionNotification).Skip(1).Subscribe(async x =>
            {
                if (SettingService.Setting.PushNotification)
                    await Update();
            });

            SettingService.Setting.ObserveProperty(x => x.DirectMessageNotification).Skip(1).Subscribe(async x =>
            {
                if (SettingService.Setting.PushNotification)
                    await Update();
            });

            SettingService.Setting.ObserveProperty(x => x.FollowNotification).Skip(1).Subscribe(async x =>
            {
                if (SettingService.Setting.PushNotification)
                    await Update();
            });
        }

        public static Push Instance { get; } = new Push();

        public async Task Register()
        {
            if (string.IsNullOrWhiteSpace(SettingService.Setting.UserUuid))
                SettingService.Setting.UserUuid = Guid.NewGuid().ToString();

            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();

            var registration = new Registration
            {
                UserUuid = SettingService.Setting.UserUuid,
                Url = channel.Uri
            };

            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(registration), Encoding.UTF8, "application/json");
                await client.PostAsync("https://nocona.cucmber.net/flantter/register", content);
            }
        }

        public async Task Unregister()
        {
            var registration = new Registration
            {
                UserUuid = SettingService.Setting.UserUuid,
            };

            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(registration), Encoding.UTF8, "application/json");
                await client.PostAsync("https://nocona.cucmber.net/flantter/unregister", content);
            }
        }

        public async Task Update()
        {
            if (string.IsNullOrWhiteSpace(SettingService.Setting.UserUuid))
                return;

            string userLanguage;
            switch (ApplicationLanguages.Languages.First())
            {
                case "ja":
                case "ja-JP":
                    userLanguage = "ja";
                    break;
                default:
                    userLanguage = "en";
                    break;
            }

            var pushAccountSettings = new List<AccountSetting>();
            foreach (var accountSetting in AdvancedSettingService.AdvancedSetting.Accounts)
            {
                var pushAccountSetting = new AccountSetting
                {
                    ScreenName = accountSetting.ScreenName,
                    UserId = accountSetting.UserId,
                    AccessToken = accountSetting.AccessToken,
                    AccessTokenSecret = accountSetting.AccessTokenSecret,
                    ConsumerKey = accountSetting.ConsumerKey,
                    ConsumerSecret = accountSetting.ConsumerSecret,
                    Platform = accountSetting.Platform.ToString(),
                    Instance = accountSetting.Instance,
                    DirectMessageNotification = SettingService.Setting.DirectMessageNotification,
                    FavoriteNotification = SettingService.Setting.FavoriteNotification,
                    FollowNotification = SettingService.Setting.FollowNotification,
                    ReplyNotification = SettingService.Setting.MentionNotification,
                    RetweetNotification = SettingService.Setting.RetweetNotification,
                    Language = userLanguage
                };
                pushAccountSettings.Add(pushAccountSetting);
            }
            var json = JsonConvert.SerializeObject(pushAccountSettings);

            var user = new User
            {
                UserUuid = SettingService.Setting.UserUuid,
                Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            };

            using (var client = new HttpClient())
            {
                var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                await client.PostAsync("https://nocona.cucmber.net/flantter/update", content);
            }
        }
    }
}
