using Flantter.MilkyWay.Models.Twitter.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class TrendViewModel
    {
        public TrendViewModel(Trend trend)
        {
            this.Model = trend;
            this.Name = trend.Name;
        }

        public string Name { get; set; }

        public Trend Model { get; set; }
    }
}
