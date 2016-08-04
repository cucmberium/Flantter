using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using SQLite.Net.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System.Reactive.Concurrency;

namespace Flantter.MilkyWay.Models.Services.Database
{
    public class Database
    {
        private static Database _Instance = new Database();
        
        private Database()
        {
        }

        public static Database Instance
        {
            get { return _Instance; }
        }

        private object _lock = new object();
        private List<TweetInfo> _tweetInfoQueue = new List<TweetInfo>();
        private List<TweetData> _tweetDataQueue = new List<TweetData>();
        private IDisposable _timer = null;

        private bool _initialized = false;
        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            string storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            _timer = Observable.Timer(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5)).SubscribeOn(ThreadPoolScheduler.Default).Subscribe(_ => 
            {
                //var stopWatch = System.Diagnostics.Stopwatch.StartNew();

                using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
                {
                    db.BeginTransaction();

                    db.CreateTable<TweetInfo>();
                    db.CreateTable<TweetData>(SQLite.Net.Interop.CreateFlags.AllImplicit);

                    lock (_lock)
                    {
                        foreach (var tweetInfo in _tweetInfoQueue)
                        {
                            var count = db.Table<TweetInfo>().Where(x => x.Id == tweetInfo.Id && x.Parameter == tweetInfo.Parameter && x.UserId == tweetInfo.UserId).Count();
                            if (count != 0)
                                continue;

                            db.Insert(tweetInfo);
                        }

                        foreach (var tweetData in _tweetDataQueue)
                        {
                            db.InsertOrReplace(tweetData);
                        }

                        _tweetInfoQueue.Clear();
                        _tweetDataQueue.Clear();
                    }

                    db.Execute("delete from TweetData where Id in (select Id from TweetData order by Id desc limit -1 offset ?);", SettingService.Setting.MaximumHoldingNumberOfTweet);
                    db.Execute("delete from TweetInfo where Id not in (select Id from TweetData);");

                    db.Commit();
                }

                //stopWatch.Stop();
                //System.Diagnostics.Debug.WriteLine(stopWatch.ElapsedMilliseconds);
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
                    var tweetInfo = new TweetInfo() { Id = id, Parameter = p, UserId = userid };
                    _tweetInfoQueue.Add(tweetInfo);
                }

                var tweetData = new TweetData() { Id = id, Json = JsonConvert.SerializeObject(status), InReplyToStatusId = status.InReplyToStatusId != 0 ? status.InReplyToStatusId : (long?)null };
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
                    var tweetInfo = new TweetInfo() { Id = id, Parameter = p, UserId = userid };
                    _tweetInfoQueue.Add(tweetInfo);
                }
                
                var tweetData = new TweetData() { Id = id, Json = JsonConvert.SerializeObject(directMessage) };
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
                    var tweetInfo = new TweetInfo() { Id = id, Parameter = p, UserId = userid };
                    _tweetInfoQueue.Add(tweetInfo);
                }
                
                var tweetData = new TweetData() { Id = id, Json = JsonConvert.SerializeObject(eventMessage) };
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
                    var tweetInfo = new TweetInfo() { Id = id, Parameter = p, UserId = userid };
                    _tweetInfoQueue.Add(tweetInfo);
                }

                var tweetData = new TweetData() { Id = id, Json = JsonConvert.SerializeObject(collection) };
                _tweetDataQueue.Add(tweetData);
            }
        }

        public Status GetStatusFromId(long id)
        {
            string json = null;
            string storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(SQLite.Net.Interop.CreateFlags.AllImplicit);

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
            string json = null;
            string storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(SQLite.Net.Interop.CreateFlags.AllImplicit);

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
            IEnumerable<string> jsons = null;
            string storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(SQLite.Net.Interop.CreateFlags.AllImplicit);

                //var tweets = db.Table<TweetInfo>().Join(db.Table<TweetData>(), x => x.Id, x => x.Id, (TweetInfo, TweetData) => new { TweetInfo, TweetData } )
                //                                  .Where(x => x.TweetInfo.Parameter == param)
                //                                  .OrderByDescending(x => x.TweetInfo.Id)
                //                                  .Take(count).ToList();
                var tweets = db.Query<TweetData>($"select * from TweetData where TweetData.Id in (select TweetInfo.Id from TweetInfo where TweetInfo.Parameter = \"{param}\" and TweetInfo.UserId = {userId.ToString()}) order by TweetData.Id desc limit {count.ToString()}");
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
            IEnumerable<string> jsons = null;
            string storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(SQLite.Net.Interop.CreateFlags.AllImplicit);

                //var tweets = db.Table<TweetInfo>().Join(db.Table<TweetData>(), x => x.Id, x => x.Id, (TweetInfo, TweetData) => new { TweetInfo, TweetData }).Where(x => x.TweetInfo.Parameter == "directmessages://").OrderByDescending(x => x.TweetInfo.Id).Take(count).ToList();
                var tweets = db.Query<TweetData>($"select * from TweetData where TweetData.Id in (select TweetInfo.Id from TweetInfo where TweetInfo.Parameter = \"directmessages://\" and TweetInfo.UserId = {userId.ToString()}) order by TweetData.Id desc limit {count.ToString()}");
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
            IEnumerable<string> jsons = null;
            string storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(SQLite.Net.Interop.CreateFlags.AllImplicit);

                //var tweets = db.Table<TweetInfo>().Join(db.Table<TweetData>(), x => x.Id, x => x.Id, (TweetInfo, TweetData) => new { TweetInfo, TweetData }).Where(x => x.TweetInfo.Parameter == "events://").OrderByDescending(x => x.TweetInfo.Id).Take(count).ToList();
                var tweets = db.Query<TweetData>($"select * from TweetData where TweetData.Id in (select TweetInfo.Id from TweetInfo where TweetInfo.Parameter = \"events://\" and TweetInfo.UserId = {userId.ToString()}) order by TweetData.Id desc limit {count.ToString()}");
                db.Commit();

                jsons = tweets.Select(x => x.Json);
            }

            foreach (var json in jsons)
            {
                var ev = JsonConvert.DeserializeObject<EventMessage>(json);
                yield return ev;
            }
        }

        public IEnumerable<EventMessage> GetCollectionEntryFromParam(long userId, int count = 200)
        {
            IEnumerable<string> jsons = null;
            string storagePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "tweet.db");
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), storagePath))
            {
                db.BeginTransaction();

                db.CreateTable<TweetInfo>();
                db.CreateTable<TweetData>(SQLite.Net.Interop.CreateFlags.AllImplicit);

                //var tweets = db.Table<TweetInfo>().Join(db.Table<TweetData>(), x => x.Id, x => x.Id, (TweetInfo, TweetData) => new { TweetInfo, TweetData }).Where(x => x.TweetInfo.Parameter == "events://").OrderByDescending(x => x.TweetInfo.Id).Take(count).ToList();
                var tweets = db.Query<TweetData>($"select * from TweetData where TweetData.Id in (select TweetInfo.Id from TweetInfo where TweetInfo.Parameter = \"collection://\" and TweetInfo.UserId = {userId.ToString()}) order by TweetData.Id desc limit {count.ToString()}");
                db.Commit();

                jsons = tweets.Select(x => x.Json);
            }

            foreach (var json in jsons)
            {
                var ev = JsonConvert.DeserializeObject<EventMessage>(json);
                yield return ev;
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
        [PrimaryKey, Indexed]
        public long Id { get; set; }
        [Indexed]
        public long? InReplyToStatusId { get; set; }
        public string Json { get; set; }
    }
}
