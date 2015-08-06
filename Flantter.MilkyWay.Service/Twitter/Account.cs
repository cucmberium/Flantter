using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Service.Twitter
{
    public sealed class Account
    {
        public string Name { get; set; }
        public string ScreenName { get; set; }
        public long UserId { get; set; }

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }

        public bool IncludeFollowingsActivity { get; set; }
        public bool PossiblySensitive { get; set; }
    }
}
