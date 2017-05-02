using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Windows.Storage;
using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Newtonsoft.Json;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using SQLite.Net.Platform.WinRT;

namespace Flantter.MilkyWay.Models.Services.Database
{
    public class Database
    {
        private readonly object _lock = new object();
        private readonly List<TweetData> _tweetDataQueue = new List<TweetData>();
        private readonly List<TweetInfo> _tweetInfoQueue = new List<TweetInfo>();

        private bool _initialized;
        private IDisposable _timer;

        private Database()
        {
        }

        public static Database Instance { get; } = new Database();

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            var storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            _timer = Observable.Timer(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5))
                .SubscribeOn(ThreadPoolScheduler.Default)
                .Subscribe(_ =>
                {
                    using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
                    {
                        db.BeginTransaction();

                        db.CreateTable<TweetInfo>();
                        db.CreateTable<TweetData>(CreateFlags.AllImplicit);

                        lock (_lock)
                        {
                            foreach (var tweetInfo in _tweetInfoQueue)
                            {
                                var count = db.Table<TweetInfo>()
                                    .Count(x => x.Id == tweetInfo.Id && x.Parameter == tweetInfo.Parameter &&
                                                x.UserId == tweetInfo.UserId);
                                if (count != 0)
                                    continue;

                                db.Insert(tweetInfo);
                            }

                            foreach (var tweetData in _tweetDataQueue)
                                db.InsertOrReplace(tweetData);

                            _tweetInfoQueue.Clear();
                            _tweetDataQueue.Clear();
                        }

                        db.Execute(
                            "delete from TweetData where Id in (select Id from TweetData order by Id desc limit -1 offset ?);",
                            SettingService.Setting.MaximumHoldingNumberOfTweet);
                        db.Execute("delete from TweetInfo where Id not in (select Id from TweetData);");

                        db.Commit();
                    }
                });
        }

        public void Free()
        {
            _timer?.Dispose();
            _initialized = false;
        }

        public void InsertTweet(Status status, IEnumerable<string> param, long userid)
        {
            lock (_lock)
            {
                var id = status.HasRetweetInformation ? status.RetweetInformation.Id : status.Id;
                foreach (var p in param)
                {
                    var tweetInfo = new TweetInfo {Id = id, Parameter = p, UserId = userid};
                    _tweetInfoQueue.Add(tweetInfo);
                }

                var tweetData = new TweetData
                {
                    Id = id,
                    Json = JsonConvert.SerializeObject(status),
                    InReplyToStatusId = status.InReplyToStatusId != 0 ? status.InReplyToStatusId : (long?) null
                };
                _tweetDataQueue.Add(tweetData);
            }
        }

        public void InsertTweet(DirectMessage directMessage, IEnumerable<string> param, long userid)
        {
            lock (_lock)
            {
                var id = directMessage.Id;
                foreach (var p in param)
                {
                    var tweetInfo = new TweetInfo {Id = id, Parameter = p, UserId = userid};
                    _tweetInfoQueue.Add(tweetInfo);
                }

                var tweetData = new TweetData {Id = id, Json = JsonConvert.SerializeObject(directMessage)};
                _tweetDataQueue.Add(tweetData);
            }
        }

        public void InsertTweet(EventMessage eventMessage, IEnumerable<string> param, long userid)
        {
            lock (_lock)
            {
                var id = eventMessage.Id;
                foreach (var p in param)
                {
                    var tweetInfo = new TweetInfo {Id = id, Parameter = p, UserId = userid};
                    _tweetInfoQueue.Add(tweetInfo);
                }

                var tweetData = new TweetData {Id = id, Json = JsonConvert.SerializeObject(eventMessage)};
                _tweetDataQueue.Add(tweetData);
            }
        }

        public void InsertTweet(CollectionEntry collection, IEnumerable<string> param, long userid)
        {
            lock (_lock)
            {
                var id = collection.Id;
                foreach (var p in param)
                {
                    var tweetInfo = new TweetInfo {Id = id, Parameter = p, UserId = userid};
                    _tweetInfoQueue.Add(tweetInfo);
                }

                var tweetData = new TweetData {Id = id, Json = JsonConvert.SerializeObject(collection)};
                _tweetDataQueue.Add(tweetData);
            }
        }

        public Status GetStatusFromId(long id)
        {
            string json;
            var storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(CreateFlags.AllImplicit);

                var tweets = db.Table<TweetData>().Where(x => x.Id == id).ToList();
                db.Commit();

                if (tweets.Count == 0)
                    return null;

                json = tweets.First().Json;
            }

            var status = JsonConvert.DeserializeObject<Status>(json);
            status.Entities.Media.ForEach(x => x.ParentEntities = status.Entities);

            return status;
        }

        public Status GetReplyStatusFromId(long id)
        {
            string json;
            var storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(CreateFlags.AllImplicit);

                var tweets = db.Table<TweetData>().Where(x => x.InReplyToStatusId == id).ToList();
                db.Commit();

                if (tweets.Count == 0)
                    return null;

                json = tweets.First().Json;
            }

            var status = JsonConvert.DeserializeObject<Status>(json);
            status.Entities.Media.ForEach(x => x.ParentEntities = status.Entities);

            return status;
        }

        public IEnumerable<Status> GetStatusesFromParam(string param, long userId, int count = 200)
        {
            IEnumerable<string> jsons;
            var storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(CreateFlags.AllImplicit);

                //var tweets = db.Table<TweetInfo>().Join(db.Table<TweetData>(), x => x.Id, x => x.Id, (TweetInfo, TweetData) => new { TweetInfo, TweetData } )
                //                                  .Where(x => x.TweetInfo.Parameter == param)
                //                                  .OrderByDescending(x => x.TweetInfo.Id)
                //                                  .Take(count).ToList();
                var tweets = db.Query<TweetData>(
                    $"select * from TweetData where TweetData.Id in (select TweetInfo.Id from TweetInfo where TweetInfo.Parameter = \"{param}\" and TweetInfo.UserId = {userId}) order by TweetData.Id desc limit {count}");
                db.Commit();

                jsons = tweets.Select(x => x.Json);
            }

            foreach (var json in jsons)
            {
                var status = JsonConvert.DeserializeObject<Status>(json);
                status.Entities.Media.ForEach(x => x.ParentEntities = status.Entities);

                yield return status;
            }
        }

        public IEnumerable<DirectMessage> GetDirectMessagesFromParam(long userId, int count = 200)
        {
            IEnumerable<string> jsons;
            var storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(CreateFlags.AllImplicit);

                //var tweets = db.Table<TweetInfo>().Join(db.Table<TweetData>(), x => x.Id, x => x.Id, (TweetInfo, TweetData) => new { TweetInfo, TweetData }).Where(x => x.TweetInfo.Parameter == "directmessages://").OrderByDescending(x => x.TweetInfo.Id).Take(count).ToList();
                var tweets = db.Query<TweetData>(
                    $"select * from TweetData where TweetData.Id in (select TweetInfo.Id from TweetInfo where TweetInfo.Parameter = \"directmessages://\" and TweetInfo.UserId = {userId}) order by TweetData.Id desc limit {count}");
                db.Commit();

                jsons = tweets.Select(x => x.Json);
            }

            foreach (var json in jsons)
            {
                var dm = JsonConvert.DeserializeObject<DirectMessage>(json);
                dm.Entities.Media.ForEach(x => x.ParentEntities = dm.Entities);

                yield return dm;
            }
        }

        public IEnumerable<EventMessage> GetEventMessagesFromParam(long userId, int count = 200)
        {
            IEnumerable<string> jsons;
            var storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(CreateFlags.AllImplicit);

                //var tweets = db.Table<TweetInfo>().Join(db.Table<TweetData>(), x => x.Id, x => x.Id, (TweetInfo, TweetData) => new { TweetInfo, TweetData }).Where(x => x.TweetInfo.Parameter == "events://").OrderByDescending(x => x.TweetInfo.Id).Take(count).ToList();
                var tweets = db.Query<TweetData>(
                    $"select * from TweetData where TweetData.Id in (select TweetInfo.Id from TweetInfo where TweetInfo.Parameter = \"events://\" and TweetInfo.UserId = {userId}) order by TweetData.Id desc limit {count}");
                db.Commit();

                jsons = tweets.Select(x => x.Json);
            }

            foreach (var json in jsons)
            {
                var ev = JsonConvert.DeserializeObject<EventMessage>(json);
                yield return ev;
            }
        }

        public IEnumerable<CollectionEntry> GetCollectionEntryFromParam(long userId, int count = 200)
        {
            IEnumerable<string> jsons;
            var storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(CreateFlags.AllImplicit);

                //var tweets = db.Table<TweetInfo>().Join(db.Table<TweetData>(), x => x.Id, x => x.Id, (TweetInfo, TweetData) => new { TweetInfo, TweetData }).Where(x => x.TweetInfo.Parameter == "events://").OrderByDescending(x => x.TweetInfo.Id).Take(count).ToList();
                var tweets = db.Query<TweetData>(
                    $"select * from TweetData where TweetData.Id in (select TweetInfo.Id from TweetInfo where TweetInfo.Parameter = \"collection://\" and TweetInfo.UserId = {userId}) order by TweetData.Id desc limit {count}");
                db.Commit();

                jsons = tweets.Select(x => x.Json);
            }

            foreach (var json in jsons)
            {
                var ev = JsonConvert.DeserializeObject<CollectionEntry>(json);
                yield return ev;
            }
        }

        public void ClearTweet(long userId, string parameter)
        {
            var storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(CreateFlags.AllImplicit);

                db.Query<TweetData>(
                    $"delete from TweetInfo where TweetInfo.Parameter = \"{parameter}\" and TweetInfo.UserId = {userId}");
                db.Query<TweetInfo>(
                    "delete from TweetData where TweetData.Id not in (select TweetInfo.Id from TweetInfo)");
            }
        }
    }

    public class TweetInfo
    {
        [Indexed]
        public long Id { get; set; }

        [Indexed]
        public long UserId { get; set; }

        [Indexed]
        public string Parameter { get; set; }
    }

    public class TweetData
    {
        [PrimaryKey]
        [Indexed]
        public long Id { get; set; }

        [Indexed]
        public long? InReplyToStatusId { get; set; }

        public string Json { get; set; }
    }
}