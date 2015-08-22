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

            this.MediaThumbnailUrl = mediaEntity.MediaThumbnailUrl;

            this.Notice = Services.Notice.Instance;
        }

        public MediaEntity Model { get; private set; }

        public string MediaThumbnailUrl { get; set; }

        public Services.Notice Notice { get; set; }
    }

    public class UrlEntityViewModel
    {
        public UrlEntityViewModel(UrlEntity urlEntity)
        {
            this.Model = urlEntity;

            this.ExpandedUrl = urlEntity.ExpandedUrl;
            this.DisplayUrl = urlEntity.DisplayUrl;

            this.Notice = Services.Notice.Instance;
        }

        public UrlEntity Model { get; private set; }

        public string ExpandedUrl { get; set; }

        public string DisplayUrl { get; set; }

        public Services.Notice Notice { get; set; }
    }
}
