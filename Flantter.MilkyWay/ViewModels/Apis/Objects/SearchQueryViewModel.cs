using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.ViewModels.Services;

namespace Flantter.MilkyWay.ViewModels.Apis.Objects
{
    public class SearchQueryViewModel
    {
        public SearchQueryViewModel(SearchQuery searchQuery)
        {
            Model = searchQuery;
            Name = searchQuery.Name;

            Notice = Notice.Instance;
        }

        public string Name { get; set; }

        public SearchQuery Model { get; set; }

        public Notice Notice { get; set; }
    }
}