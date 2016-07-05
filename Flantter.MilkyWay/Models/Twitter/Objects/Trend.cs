using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class Trend
    {
        public Trend(CoreTweet.Trend cTrend)
        {
            this.Name = cTrend.Name;
            this.Query = cTrend.Query;
        }

        public Trend()
        {
        }

        public string Name { get; set; }

        public string Query { get; set; }
    }
}
