using Flantter.MilkyWay.Models.Twitter.Objects;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels.Twitter.Objects
{
    public class MediaEntityViewModel
    {
        public MediaEntityViewModel(MediaEntity mediaEntity)
        {
            this.Model = mediaEntity;
            this.MediaThumbnailUrl = mediaEntity.ObserveProperty(x => x.MediaThumbnailUrl).ToReactiveProperty();

            this.Notice = new ReactiveProperty<Service.Notice>(Service.Notice.Instance);
        }

        public MediaEntity Model { get; private set; }

        public ReactiveProperty<string> MediaThumbnailUrl { get; set; }

        public ReactiveProperty<Service.Notice> Notice { get; set; }
    }
}
