using Flantter.MilkyWay.Models.Apis.Objects;

namespace Flantter.MilkyWay.ViewModels.Apis.Objects
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