using Flantter.MilkyWay.Models.Twitter.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class SearchQueryViewModel
    {
        public SearchQueryViewModel(SearchQuery searchQuery)
        {
            this.Model = searchQuery;
            this.Name = searchQuery.Name;

            this.Notice = Services.Notice.Instance;
        }

        public string Name { get; set; }

        public SearchQuery Model { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
