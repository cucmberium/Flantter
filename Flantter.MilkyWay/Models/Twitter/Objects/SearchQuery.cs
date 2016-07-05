using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class SearchQuery
    {
        public SearchQuery(CoreTweet.SearchQuery cSearchQuery)
        {
            this.Id = cSearchQuery.Id.HasValue ? cSearchQuery.Id.Value : 0;
            this.Name = cSearchQuery.Name;
            this.Query = cSearchQuery.Query;
        }

        public SearchQuery()
        {
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Query { get; set; }
    }
}
