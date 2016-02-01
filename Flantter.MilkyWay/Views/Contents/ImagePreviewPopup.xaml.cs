using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Util;
using NotificationsExtensions.Toasts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class ImagePreviewPopup : UserControl
    {
        private Popup ImagePreview;

        private bool imageOpened = false; 
        public List<MediaEntity> Images { get; set; }
        public int ImageIndex { get; set; }

        ResourceLoader _ResourceLoader;

        public ImagePreviewPopup()
        {
            this.InitializeComponent();

            _ResourceLoader = new ResourceLoader();

            Window.Current.SizeChanged += ImagePreviewPopup_SizeChanged;

            this.Width = WindowSizeHelper.Instance.ClientWidth;
            this.Height = WindowSizeHelper.Instance.ClientHeight;

            this.ImagePreview = new Popup
            {
                Child = this,
                IsLightDismissEnabled = false,
                Opacity = 1
            };

            Canvas.SetTop(this.ImagePreview, WindowSizeHelper.Instance.TitleBarHeight);
            Canvas.SetLeft(this.ImagePreview, 0);

            //this.ImagePreviewImage.Source = new BitmapImage(new Uri("http://localhost"));
        }
        
        ~ImagePreviewPopup()
        {
            Window.Current.SizeChanged -= ImagePreviewPopup_SizeChanged;
        }

        private void ImagePreviewPopup_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            Canvas.SetTop(this.ImagePreview, WindowSizeHelper.Instance.TitleBarHeight);
            Canvas.SetLeft(this.ImagePreview, 0);

            this.Width = WindowSizeHelper.Instance.ClientWidth;
            this.Height = WindowSizeHelper.Instance.ClientHeight;

            this.ImagePreviewCanvas.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, WindowSizeHelper.Instance.ClientWidth, WindowSizeHelper.Instance.ClientHeight)
            };

            var canvasTop = (WindowSizeHelper.Instance.ClientHeight - this.ImagePreviewImage.Height) / 2;
            var canvasLeft = (WindowSizeHelper.Instance.ClientWidth - this.ImagePreviewImage.Width) / 2;

            Canvas.SetTop(this.ImagePreviewImage, canvasTop);
            Canvas.SetLeft(this.ImagePreviewImage, canvasLeft);
        }

        public void ImageRefresh()
        {
            this.ImagePreviewProgressRing.Visibility = Visibility.Visible;
            this.ImagePreviewProgressRing.IsActive = true;
            this.ImagePreviewSymbolIcon.Visibility = Visibility.Collapsed;
            this.ImagePreviewImage.Opacity = 0;
            // こっちのほうが画像がキャッシュされるような気がする(気のせい)
            this.ImagePreviewImage.Source = (ImageSource)Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(ImageSource), this.Images[this.ImageIndex].MediaUrl);
            imageOpened = false;

            if (this.ImageIndex <= 0 || this.Images.Count <= 1)
                this.ImagePreviewPreviousButton.Visibility = Visibility.Collapsed;
            else
                this.ImagePreviewPreviousButton.Visibility = Visibility.Visible;

            if (this.ImageIndex >= this.Images.Count - 1 || this.Images.Count <= 1)
                this.ImagePreviewNextButton.Visibility = Visibility.Collapsed;
            else
                this.ImagePreviewNextButton.Visibility = Visibility.Visible;
        }

        public void ImageInitialize()
        {
            var element = this.ImagePreviewImage as UIElement;
            var transform = element.RenderTransform as CompositeTransform;

            if (transform == null)
                transform = new CompositeTransform();

            transform.TranslateX = 0.0;
            transform.TranslateY = 0.0;

            transform.ScaleX = 1.0;
            transform.ScaleY = 1.0;

            element.RenderTransform = transform;


            var imageWidth = ((BitmapImage)this.ImagePreviewImage.Source).PixelWidth;
            var imageHeight = ((BitmapImage)this.ImagePreviewImage.Source).PixelHeight;
            var windowWidth = WindowSizeHelper.Instance.ClientWidth;
            var windowHeight = WindowSizeHelper.Instance.ClientHeight;

            double raito = 1.0;

            if (imageWidth > windowWidth * 0.95 && imageHeight > windowHeight * 0.95)
            {
                var imageWindowWidthRaito = windowWidth / imageWidth;
                var imageWindowHeightRaito = windowHeight / imageHeight;
                raito = (imageWindowHeightRaito < imageWindowWidthRaito ? imageWindowHeightRaito : imageWindowWidthRaito) * 0.95;
            }
            else if (imageWidth <= windowWidth * 0.95 && imageHeight <= windowHeight * 0.95)
            {
                raito = 1.0;
            }
            else if (imageWidth > windowWidth * 0.95 && imageHeight <= windowHeight * 0.95)
            {
                raito = windowWidth / imageWidth * 0.95;
            }
            else if (imageWidth <= windowWidth * 0.95 && imageHeight > windowHeight * 0.95)
            {
                raito = windowHeight / imageHeight * 0.95;
            }

            this.ImagePreviewImage.Width = imageWidth * raito;
            this.ImagePreviewImage.Height = imageHeight * raito;

            var canvasTop = (windowHeight - this.ImagePreviewImage.Height) / 2;
            var canvasLeft = (windowWidth - this.ImagePreviewImage.Width) / 2;

            Canvas.SetTop(this.ImagePreviewImage, canvasTop);
            Canvas.SetLeft(this.ImagePreviewImage, canvasLeft);
        }

        public void Show()
        {
            Canvas.SetTop(this.ImagePreview, WindowSizeHelper.Instance.TitleBarHeight);
            Canvas.SetLeft(this.ImagePreview, 0);

            this.ImagePreviewCanvas.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, WindowSizeHelper.Instance.ClientWidth, WindowSizeHelper.Instance.ClientHeight)
            };
            
            if (imageOpened)
                this.ImageInitialize();

            this.ImagePreview.IsOpen = true;
        }

        public void Hide()
        {
            this.ImagePreview.IsOpen = false;
        }

        private void ImagePreviewImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            imageOpened = true;

            this.ImagePreviewProgressRing.Visibility = Visibility.Collapsed;
            this.ImagePreviewProgressRing.IsActive = false;
            this.ImagePreviewSymbolIcon.Visibility = Visibility.Collapsed;
            this.ImagePreviewImage.Opacity = 0;

            this.ImageInitialize();

            this.ImagePreviewImage.Opacity = 1;
        }

        private void ImagePreviewImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            this.ImagePreviewProgressRing.Visibility = Visibility.Collapsed;
            this.ImagePreviewProgressRing.IsActive = false;
            this.ImagePreviewSymbolIcon.Visibility = Visibility.Visible;
            this.ImagePreviewImage.Opacity = 0;
        }

        private void ImagePreviewGrid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var element = this.ImagePreviewImage as UIElement;
            var transform = element.RenderTransform as CompositeTransform;

            if (transform == null)
                transform = new CompositeTransform();

            if (e.GetCurrentPoint(element).Properties.MouseWheelDelta > 0)
            {
                if (transform.ScaleX > 5 || transform.ScaleY > 5)
                    return;

                transform.ScaleX += 0.05 * transform.ScaleX;
                transform.ScaleY += 0.05 * transform.ScaleY;
            }
            else
            {
                if (transform.ScaleX < 0.20 || transform.ScaleY < 0.20)
                    return;

                transform.ScaleX -= 0.05 * transform.ScaleX;
                transform.ScaleY -= 0.05 * transform.ScaleY;
            }

            element.RenderTransform = transform;

            e.Handled = true;
        }

        private void ImagePreviewGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();
            e.Handled = true;
        }

        private void ImagePreviewCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var element = this.ImagePreviewImage as UIElement;
            var transform = element.RenderTransform as CompositeTransform;
            
            if (transform == null)
                transform = new CompositeTransform();

            transform.ScaleX *= e.Delta.Scale;
            transform.ScaleY *= e.Delta.Scale;

            if (transform.ScaleX > 5 || transform.ScaleY > 5)
            {
                transform.ScaleX = 5;
                transform.ScaleY = 5;
            }
            else if (transform.ScaleX < 0.20 || transform.ScaleY < 0.20)
            {
                transform.ScaleX = 0.20;
                transform.ScaleY = 0.20;
            }

            transform.TranslateX += e.Delta.Translation.X;
            transform.TranslateY += e.Delta.Translation.Y;

            transform.TranslateX -= (e.Position.X - (Canvas.GetLeft(element) + this.ImagePreviewImage.ActualWidth / 2)) * (1 - 1 / e.Delta.Scale) * transform.ScaleX;
            transform.TranslateY -= (e.Position.Y - (Canvas.GetTop(element) + this.ImagePreviewImage.ActualHeight / 2)) * (1 - 1 / e.Delta.Scale) * transform.ScaleY;

            element.RenderTransform = transform;

            e.Handled = true;
        }

        private void ImagePreviewCanvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var element = this.ImagePreviewImage as UIElement;
            var transform = element.RenderTransform as CompositeTransform;

            if (transform == null)
                transform = new CompositeTransform();
            

            if (transform.TranslateX + (this.ImagePreviewImage.ActualWidth * transform.ScaleX) / 2 < -WindowSizeHelper.Instance.ClientWidth / 2)
                transform.TranslateX = -WindowSizeHelper.Instance.ClientWidth / 2 - (this.ImagePreviewImage.ActualWidth * transform.ScaleX) / 2;
            else if (transform.TranslateX - (this.ImagePreviewImage.ActualWidth * transform.ScaleX) / 2 > WindowSizeHelper.Instance.ClientWidth / 2)
                transform.TranslateX = WindowSizeHelper.Instance.ClientWidth / 2 + (this.ImagePreviewImage.ActualWidth * transform.ScaleX) / 2;

            if (transform.TranslateY + (this.ImagePreviewImage.ActualHeight * transform.ScaleY) / 2 < -WindowSizeHelper.Instance.ClientHeight / 2)
                transform.TranslateY = -WindowSizeHelper.Instance.ClientHeight / 2 - (this.ImagePreviewImage.ActualHeight * transform.ScaleY) / 2;
            else if (transform.TranslateY - (this.ImagePreviewImage.ActualHeight * transform.ScaleY) / 2 > WindowSizeHelper.Instance.ClientHeight / 2)
                transform.TranslateY = WindowSizeHelper.Instance.ClientHeight / 2 + (this.ImagePreviewImage.ActualHeight * transform.ScaleY) / 2;

            element.RenderTransform = transform;

            e.Handled = true;
        }

        private void ImagePreviewImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void ImagePreviewImage_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
        }

        private void ImagePreviewPreviousButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.ImageIndex -= 1;
            this.ImageRefresh();

            e.Handled = true;
        }

        private void ImagePreviewNextButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.ImageIndex += 1;
            this.ImageRefresh();

            e.Handled = true;
        }

        private async void ImagePreviewMenu_SaveImage(object sender, RoutedEventArgs e)
        {
            var toastContent = new ToastContent();
            toastContent.Visual = new ToastVisual();
            toastContent.Visual.TitleText = new ToastText() { Text = "Flantter" };
            toastContent.Visual.BodyTextLine1 = new ToastText() { Text = _ResourceLoader.GetString("ImagePreviewPopup_ImageSavedSuccessfuly") };

            if (!SettingService.Setting.NotificationSound)
                toastContent.Audio = new ToastAudio() { Silent = true };

            try
            {
                var imageFileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                var client = new HttpClient();
                var response = await client.GetAsync(new Uri(this.Images[this.ImageIndex].MediaUrl));
                switch (response.Content.Headers.ContentType.MediaType)
                {
                    case "image/jpeg":
                        imageFileName += ".jpg";
                        break;
                    case "image/gif":
                        imageFileName += ".gif";
                        break;
                    case "image/png":
                        imageFileName += ".png";
                        break;
                    case "image/tiff":
                        imageFileName += ".tiff";
                        break;
                    case "image/x-bmp":
                        imageFileName += ".bmp";
                        break;
                }
                var imageFile = await KnownFolders.PicturesLibrary.CreateFileAsync(imageFileName, CreationCollisionOption.GenerateUniqueName);
                await Windows.Storage.FileIO.WriteBytesAsync(imageFile, (await response.Content.ReadAsBufferAsync()).ToArray());
            }
            catch
            {
                toastContent.Visual.BodyTextLine1.Text = new ResourceLoader().GetString("ImagePreviewPopup_FailedtoImageSave");
            }

            var toast = new ToastNotification(toastContent.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        private async void ImagePreviewMenu_ShowinBrowser(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(this.Images[this.ImageIndex].ExpandedUrl));
        }

        private async void ImagePreviewMenu_SearchSimilarImage(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.google.co.jp/searchbyimage?image_url=" + this.Images[this.ImageIndex].MediaUrl));
        }

        private void ImagePreviewMenu_Close(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void ImagePreviewTriangleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(this.ImagePreviewTriangleButton);

            e.Handled = true;
        }
    }
}
