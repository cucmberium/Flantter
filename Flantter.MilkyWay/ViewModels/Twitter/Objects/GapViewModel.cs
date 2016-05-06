using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class GapViewModel : ITweetViewModel
    {
        public GapViewModel(Gap gap)
        {
            this.Model = gap;
            this.Id = gap.Id;

            this.Notice = Services.Notice.Instance;
            this.Setting = SettingService.Setting;
        }

        public Gap Model { get; set; }

        public long Id { get; set; }

        public Services.Notice Notice { get; set; }

        public Setting.SettingService Setting { get; set; }
    }

}
