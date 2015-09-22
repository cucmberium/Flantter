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
    public class ShowImagePreviewAction : DependencyObject, IAction
    {
        private ImagePreviewPopup _ImagePreviewPopup = null;

        public ShowImagePreviewAction()
        {
        }

        public object Execute(object sender, object parameter)
        {
            var notification = parameter as Notification;
            var mediaEntity = notification.Content as MediaEntity;

            if (_ImagePreviewPopup == null)
                _ImagePreviewPopup = new ImagePreviewPopup();

            this._ImagePreviewPopup.ImageUrl = mediaEntity.MediaUrl;
            this._ImagePreviewPopup.ImageWebUrl = mediaEntity.ExpandedUrl;
            this._ImagePreviewPopup.ImageChanged();

            this._ImagePreviewPopup.Show();

            return null;
        }
    }
}
