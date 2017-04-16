using Flantter.MilkyWay.Models.Twitter.Objects;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class TrendViewModel
    {
        public TrendViewModel(Trend trend)
        {
            Model = trend;
            Name = trend.Name;
        }

        public string Name { get; set; }

        public Trend Model { get; set; }
    }
}