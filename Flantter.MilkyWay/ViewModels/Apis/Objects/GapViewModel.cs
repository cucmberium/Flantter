using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;

namespace Flantter.MilkyWay.ViewModels.Apis.Objects
{
    public class GapViewModel : ITweetViewModel
    {
        public GapViewModel(Gap gap)
        {
            Model = gap;
            Id = gap.Id;

            Notice = Notice.Instance;
            Setting = SettingService.Setting;
        }

        public Gap Model { get; set; }

        public Notice Notice { get; set; }

        public SettingService Setting { get; set; }

        public long Id { get; set; }
    }
}