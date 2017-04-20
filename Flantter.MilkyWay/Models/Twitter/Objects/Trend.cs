namespace Flantter.MilkyWay.Models.Twitter.Objects
{
    public class Trend
    {
        public Trend(CoreTweet.Trend cTrend)
        {
            Name = cTrend.Name;
            Query = cTrend.Query;
        }

        public Trend()
        {
        }

        public string Name { get; set; }

        public string Query { get; set; }
    }
}