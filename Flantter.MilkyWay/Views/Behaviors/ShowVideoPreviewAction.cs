using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Views.Contents;
using Flantter.MilkyWay.Views.Util;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Flantter.MilkyWay.Views.Behaviors
{
    public class ShowVideoPreviewAction : DependencyObject, IAction
    {
        VideoPreviewPopup _VideoPreviewPopup = null;

        public ShowVideoPreviewAction()
        {
        }

        public object Execute(object sender, object parameter)
        {
            var notification = parameter as Notification;
            var mediaEntity = notification.Content as MediaEntity;

            if (_VideoPreviewPopup == null)
                _VideoPreviewPopup = new VideoPreviewPopup();
            
            this._VideoPreviewPopup.Id = mediaEntity.VideoInfo.VideoId;
            this._VideoPreviewPopup.VideoWebUrl = mediaEntity.ExpandedUrl;
            this._VideoPreviewPopup.VideoThumbnailUrl = mediaEntity.MediaThumbnailUrl;
            this._VideoPreviewPopup.VideoType = mediaEntity.VideoInfo.VideoType;
            this._VideoPreviewPopup.VideoContentType = mediaEntity.VideoInfo.VideoContentType;
            this._VideoPreviewPopup.VideoChanged();

            this._VideoPreviewPopup.Show();

            return null;
        }
    }
}
