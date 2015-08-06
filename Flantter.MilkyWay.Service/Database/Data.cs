using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Service.Database
{
    public sealed class TweetData
    {
        public string Type { get; set; }

        public long Id { get; set; }

        public string Text { get; set; }

        public DateTimeOffset CreateAt { get; set; }

        public string Filter { get; set; }

        public string Json { get; set; }
    }
}
