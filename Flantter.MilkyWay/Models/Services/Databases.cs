using Flantter.MilkyWay.Setting;
using Newtonsoft.Json;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Services
{
    public class Databases
    {
        private static Databases _Instance = new Databases();
        private Databases() { }

        public static Databases Instance
        {
            get { return _Instance; }
        }

        private SQLiteConnection sqliteConnection;

        IDisposable sqliteDisposableTimer = null;
        public void Initialize()
        {
            var path = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "fldb.sqlite");
            sqliteConnection = new SQLiteConnection(new SQLitePlatformWinRT(), path);
            SQLiteCommand cmd = sqliteConnection.CreateCommand("pragma encoding=utf8;");
            cmd.ExecuteNonQuery();
        }

        public void Uninitialize()
        {
            sqliteDisposableTimer.Dispose();
            if (sqliteConnection.IsInTransaction)
                sqliteConnection.Commit();

            sqliteConnection.Close();
        }

        public void CreateUserTable(AccountSetting account)
        {
            string sql = "create table if not exists \"Data" + account.UserId.ToString() + "\"(Type varchar, Id integer, CreateAt bigint, Filter varchar, Json varchar);";
            SQLiteCommand cmd = sqliteConnection.CreateCommand(sql);
            cmd.ExecuteNonQuery();

            sql = "create index if not exists \"Index" + account.UserId.ToString() + "\" on \"Data" + account.UserId.ToString() + "\"(Id, CreateAt, Filter);";
            cmd = sqliteConnection.CreateCommand(sql);
            cmd.ExecuteNonQuery();
        }

        public void RemoveUserTable(AccountSetting account)
        {
            string sql = "drop table if exists \"Data" + account.UserId.ToString() + "\"";
            SQLiteCommand cmd = sqliteConnection.CreateCommand(sql);
            cmd.ExecuteNonQuery();

            sql = "drop index if not exists \"Data" + account.UserId.ToString() + "\"";
            cmd = sqliteConnection.CreateCommand(sql);
            cmd.ExecuteNonQuery();
        }

        public void StoreTweet(TweetEventArgs tweet)
        {
            // Todo : Transactionを使った軽量化

            return;

            if (tweet.Type == TweetEventArgs.TypeEnum.Status)
            {
                var sql = "insert into Data" + tweet.UserId.ToString() + " values(?,?,?,?,?);"; // string.Format(, "Status", tweet.Status.Id, tweet.Status.Text, tweet.Status.CreatedAt.DateTime.ToBinary(), string.Join(",", tweet.Parameter), JsonConvert.SerializeObject(tweet.Status));
                SQLiteCommand cmd = sqliteConnection.CreateCommand(sql);
                cmd.Bind("Status");
                cmd.Bind(tweet.Status.Id);
                cmd.Bind(tweet.Status.CreatedAt.ToBinary());
                cmd.Bind(string.Join(",", tweet.Parameter));
                cmd.Bind(JsonConvert.SerializeObject(tweet.Status));
                cmd.ExecuteNonQuery();
                
            }
            else if (tweet.Type == TweetEventArgs.TypeEnum.DirectMessage)
            {
                var sql = "insert into Data" + tweet.UserId.ToString() + " values(?,?,?,?,?);"; // string.Format(, "DirectMessage", tweet.DirectMessage.Id, tweet.DirectMessage.Text, tweet.DirectMessage.CreatedAt.DateTime.ToBinary(), string.Join(",", tweet.Parameter), JsonConvert.SerializeObject(tweet.DirectMessage));
                SQLiteCommand cmd = sqliteConnection.CreateCommand(sql);
                cmd.Bind("DirectMessage");
                cmd.Bind(tweet.DirectMessage.Id);
                cmd.Bind(tweet.DirectMessage.CreatedAt.ToBinary());
                cmd.Bind(string.Join(",", tweet.Parameter));
                cmd.Bind(JsonConvert.SerializeObject(tweet.Status));
                cmd.ExecuteNonQuery();
            }
            else if (tweet.Type == TweetEventArgs.TypeEnum.EventMessage)
            {
                var sql = "insert into Data" + tweet.UserId.ToString() + " values(?,?,?,?,?);"; //var sql = string.Format("insert into Data" + tweet.UserId.ToString() + " values('{0}', {1}, '{2}', {3}, '{4}', '{5}');", "EventMessage", 0, "event", tweet.EventMessage.CreatedAt.DateTime.ToBinary(), string.Join(",", tweet.Parameter), JsonConvert.SerializeObject(tweet.EventMessage));
                SQLiteCommand cmd = sqliteConnection.CreateCommand(sql);
                cmd.Bind("EventMessage");
                cmd.Bind(0);
                cmd.Bind(tweet.EventMessage.CreatedAt.ToBinary());
                cmd.Bind(string.Join(",", tweet.Parameter));
                cmd.Bind(JsonConvert.SerializeObject(tweet.Status));
                cmd.ExecuteNonQuery();
            }
        }

        public void RemoveTweet(TweetDeleteEventArgs tweet)
        {
            var sql = "delete from Data" + tweet.UserId.ToString() + " where Id = ?"; //string.Format(, tweet.Id);
            SQLiteCommand cmd = sqliteConnection.CreateCommand(sql);
            cmd.Bind(tweet.Id);
            cmd.ExecuteNonQuery();
        }
    }
}
