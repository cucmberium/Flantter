using CoreTweet;
using CoreTweet.Streaming;
using CoreTweet.Streaming.Reactive;
using Flantter.MilkyWay.Service.Database;
using Flantter.MilkyWay.Service.Twitter;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;

namespace Flantter.MilkyWay.Service
{
    public sealed class AppServiceTask : IBackgroundTask
    {
        private static BackgroundTaskDeferral _serviceDeferral;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += TaskInstance_Canceled;
            _serviceDeferral = taskInstance.GetDeferral();
            var appService = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (appService.Name == "Flantter.MilkyWay.Service")
            {
                appService.AppServiceConnection.RequestReceived += AppServiceConnection_RequestReceived;
            }
        }

        private async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var messageDeferral = args.GetDeferral();

            var message = args.Request.Message;
            var command = message["Command"] as string;
            switch (command)
            {
                case "Initialize":
                    this.Initialize();
                    break;
                case "Finalize":
                    this.Uninitialize();
                    break;
                case "AddAccountInfo":
                    this.AddAccountInfo(new Account()
                    {
                        Name = message["Name"] as string,
                        ScreenName = message["ScreenName"] as string,
                        UserId = (long)message["UserId"],
                        ConsumerKey = message["ConsumerKey"] as string,
                        ConsumerSecret = message["ConsumerSecret"] as string,
                        AccessToken = message["AccessToken"] as string,
                        AccessTokenSecret = message["AccessTokenSecret"] as string,
                        IncludeFollowingsActivity = (bool)message["IncludeFollowingsActivity"],
                        PossiblySensitive = (bool)message["PossiblySensitive"],
                    });
                    break;
                case "StartUserstream":
                    this.StartUserstream((long)message["UserId"]);
                    break;
                default:
                    break;
            }

            messageDeferral.Complete();
            return;
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _serviceDeferral.Complete();
        }

        private void Initialize()
        {
            accountList = new List<Account>();
            userstreamDict = new Dictionary<long, IDisposable>();

            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "fldb.sqlite");
            sqliteConnection = new SQLiteConnection(new SQLitePlatformWinRT(), path);
        }
        
        private void Uninitialize() // ホントはFinalizeが良かった
        {
            if (sqliteConnection != null)
                sqliteConnection.Dispose();
        }

        private void AddAccountInfo(Account account)
        {
            if (!sqliteConnection.TableMappings.Any(x => x.TableName == account.UserId.ToString()))
            {
                string sql = "create table if not exists \"Data" + account.UserId.ToString() + "\"(Type varchar, Id integer, Text varchar, CreateAt bigint, Filter varchar, Json varchar);";
                SQLiteCommand cmd = sqliteConnection.CreateCommand(sql);
                cmd.ExecuteNonQuery();
            }
            if (!accountList.Any(x => x.UserId == account.UserId))
            {
                accountList.Add(account);
            }
        }

        private void StartUserstream(long userId)
        {
            var account = accountList.FirstOrDefault(x => x.UserId == userId);
            if (account == null)
                return;

            var token = Tokens.Create(account.ConsumerKey, account.ConsumerSecret, account.AccessToken, account.AccessTokenSecret);

            var param = new Dictionary<string, object>() { { "include_followings_activity", account.IncludeFollowingsActivity } };
            var observable = token.Streaming.UserAsObservable(param);

            observable
                .Catch((Exception ex) =>
                {
                    return observable.DelaySubscription(TimeSpan.FromSeconds(10)).Retry();
                })
                .Subscribe(
                    async (StreamingMessage m) =>
                    {
                        
                        switch (m.Type)
                        {
                            case MessageType.Create:
                                var status = m as StatusMessage;
                                var sql = string.Format("insert into Data" + userId.ToString() + " values({0}, {1}, {2}, {3}, {4}, {5});", "Status", status.Status.Id, status.Status.Text, status.Status.CreatedAt.DateTime.ToBinary(), "home://", status.Json);
                                SQLiteCommand cmd = sqliteConnection.CreateCommand(sql);
                                cmd.ExecuteNonQuery();
                                break;
                        }
                    },
                    (Exception ex) => { userstreamDict.Remove(userId); },
                    () => { userstreamDict.Remove(userId); }
                );
        }

        private void StopUserstream(long userId)
        {

        }

        private SQLiteConnection sqliteConnection;

        private List<Account> accountList;
        private Dictionary<long, IDisposable> userstreamDict; 
    }
}
