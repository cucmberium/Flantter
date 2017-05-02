namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class SearchQuery
    {
        public SearchQuery(CoreTweet.SearchQuery cSearchQuery)
        {
            Id = cSearchQuery.Id ?? 0;
            Name = cSearchQuery.Name;
            Query = cSearchQuery.Query;
        }

        public SearchQuery()
        {
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Query { get; set; }
    }
}