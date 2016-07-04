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
    public class Databases
    {
        private static Databases _Instance = new Databases();
        
        private Databases()
        {
        }

        public static Databases Instance
        {
            get { return _Instance; }
        }

        private object _lock = new object();
        private List<TweetInfo> _tweetInfoQueue = new List<TweetInfo>();
        private List<TweetData> _tweetDataQueue = new List<TweetData>();
        private IDisposable _timer = null;

        public void Initialize()
        {
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

                    db.Execute("delete from TweetData where Id in (select Id from TweetData order by Id desc limit -1 offset 10000);");
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

                var tweetData = new TweetData() { Id = id, Json = JsonConvert.SerializeObject(status) };
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
        public string Json { get; set; }
    }
}
