using Flantter.MilkyWay.Models.Twitter.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.Views.Controls;
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
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class ImagePreviewPopup : UserControl, ContentPopup
    {
        private Popup ImagePreview;

        private bool imageOpened = false; 
        public List<MediaEntity> Images { get; set; }
        public int ImageIndex { get; set; }

        ResourceLoader _ResourceLoader;

        public bool IsOpen { get { return this.ImagePreview.IsOpen; } }

        public ImagePreviewPopup()
        {
            this.InitializeComponent();

            _ResourceLoader = new ResourceLoader();

            Window.Current.SizeChanged += ImagePreviewPopup_SizeChanged;

            this.ImagePreview = new Popup
            {
                Child = this,
                IsLightDismissEnabled = false,
                Opacity = 1
            };
            
            this.Width = WindowSizeHelper.Instance.ClientWidth;
            this.Height = WindowSizeHelper.Instance.ClientHeight - LayoutHelper.Instance.TitleBarHeight.Value;

            Canvas.SetTop(this.ImagePreview, LayoutHelper.Instance.TitleBarHeight.Value);
            Canvas.SetLeft(this.ImagePreview, 0);

            //this.ImagePreviewImage.Source = new BitmapImage(new Uri("http://localhost"));
        }
        
        ~ImagePreviewPopup()
        {
            Window.Current.SizeChanged -= ImagePreviewPopup_SizeChanged;
        }

        private void ImagePreviewPopup_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            Canvas.SetTop(this.ImagePreview, LayoutHelper.Instance.TitleBarHeight.Value);
            Canvas.SetLeft(this.ImagePreview, 0);

            this.Width = WindowSizeHelper.Instance.ClientWidth;
            this.Height = WindowSizeHelper.Instance.ClientHeight - LayoutHelper.Instance.TitleBarHeight.Value;

            this.ImagePreviewCanvas.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, WindowSizeHelper.Instance.ClientWidth, WindowSizeHelper.Instance.ClientHeight - LayoutHelper.Instance.TitleBarHeight.Value)
            };

            var imageWidth = ((BitmapImage)this.ImagePreviewImage.Source).PixelWidth;
            var imageHeight = ((BitmapImage)this.ImagePreviewImage.Source).PixelHeight;
            var windowWidth = WindowSizeHelper.Instance.ClientWidth;
            var windowHeight = WindowSizeHelper.Instance.ClientHeight - LayoutHelper.Instance.TitleBarHeight.Value;

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

            var canvasTop = (WindowSizeHelper.Instance.ClientHeight - LayoutHelper.Instance.TitleBarHeight.Value - this.ImagePreviewImage.Height) / 2;
            var canvasLeft = (WindowSizeHelper.Instance.ClientWidth - this.ImagePreviewImage.Width) / 2;

            Canvas.SetTop(this.ImagePreviewImage, canvasTop);
            Canvas.SetLeft(this.ImagePreviewImage, canvasLeft);
        }

        public void ImageRefresh()
        {
            this.ImagePreviewProgressRing.Visibility = Visibility.Visible;
            this.ImagePreviewProgressRing.IsActive = true;
            this.ImagePreviewSymbolIcon.Visibility = Visibility.Collapsed;
            this.ImagePreviewImage.Visibility = Visibility.Collapsed;
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
            var windowHeight = WindowSizeHelper.Instance.ClientHeight - LayoutHelper.Instance.TitleBarHeight.Value;

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
            Canvas.SetTop(this.ImagePreview, LayoutHelper.Instance.TitleBarHeight.Value);
            Canvas.SetLeft(this.ImagePreview, 0);

            this.ImagePreviewCanvas.Clip = new RectangleGeometry()
            {
                Rect = new Rect(0, 0, WindowSizeHelper.Instance.ClientWidth, WindowSizeHelper.Instance.ClientHeight - LayoutHelper.Instance.TitleBarHeight.Value)
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
            this.ImagePreviewImage.Visibility = Visibility.Collapsed;

            this.ImageInitialize();

            this.ImagePreviewImage.Visibility = Visibility.Visible;
        }

        private void ImagePreviewImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            this.ImagePreviewProgressRing.Visibility = Visibility.Collapsed;
            this.ImagePreviewProgressRing.IsActive = false;
            this.ImagePreviewSymbolIcon.Visibility = Visibility.Visible;
            this.ImagePreviewImage.Visibility = Visibility.Collapsed;
        }

        private void ImagePreviewGrid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var element = this.ImagePreviewImage as UIElement;
            var transform = element.RenderTransform as CompositeTransform;

            if (transform == null)
                transform = new CompositeTransform();

            if (e.GetCurrentPoint(element).Properties.MouseWheelDelta > 0)
            {
                if (transform.ScaleX >= 5 || transform.ScaleY >= 5)
                    return;

                transform.ScaleX *= 1.1;
                transform.ScaleY *= 1.1;

                if (transform.ScaleX >= 5 || transform.ScaleY >= 5)
                {
                    transform.ScaleX = 5;
                    transform.ScaleY = 5;
                }
            }
            else
            {
                if (transform.ScaleX <= 0.40 || transform.ScaleY <= 0.40)
                    return;

                transform.ScaleX *= 0.90;
                transform.ScaleY *= 0.90;

                if (transform.ScaleX <= 0.40 || transform.ScaleY <= 0.40)
                {
                    transform.ScaleX = 0.40;
                    transform.ScaleY = 0.40;
                }
            }
            
            if (transform.TranslateX + (this.ImagePreviewImage.ActualWidth * transform.ScaleX) / 2 < -WindowSizeHelper.Instance.ClientWidth / 2)
                transform.TranslateX = -WindowSizeHelper.Instance.ClientWidth / 2 - (this.ImagePreviewImage.ActualWidth * transform.ScaleX) / 2;
            else if (transform.TranslateX - (this.ImagePreviewImage.ActualWidth * transform.ScaleX) / 2 > WindowSizeHelper.Instance.ClientWidth / 2)
                transform.TranslateX = WindowSizeHelper.Instance.ClientWidth / 2 + (this.ImagePreviewImage.ActualWidth * transform.ScaleX) / 2;

            if (transform.TranslateY + (this.ImagePreviewImage.ActualHeight * transform.ScaleY) / 2 < -WindowSizeHelper.Instance.ClientHeight / 2)
                transform.TranslateY = -(WindowSizeHelper.Instance.ClientHeight - LayoutHelper.Instance.TitleBarHeight.Value) / 2 - (this.ImagePreviewImage.ActualHeight * transform.ScaleY) / 2;
            else if (transform.TranslateY - (this.ImagePreviewImage.ActualHeight * transform.ScaleY) / 2 > WindowSizeHelper.Instance.ClientHeight / 2)
                transform.TranslateY = (WindowSizeHelper.Instance.ClientHeight - LayoutHelper.Instance.TitleBarHeight.Value) / 2 + (this.ImagePreviewImage.ActualHeight * transform.ScaleY) / 2;

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

            if (transform.ScaleX >= 5 || transform.ScaleY >= 5)
            {
                transform.ScaleX = 5;
                transform.ScaleY = 5;
            }
            else if (transform.ScaleX <= 0.40 || transform.ScaleY <= 0.40)
            {
                transform.ScaleX = 0.40;
                transform.ScaleY = 0.40;
            }

            transform.TranslateX += e.Delta.Translation.X;
            transform.TranslateY += e.Delta.Translation.Y;

            transform.TranslateX = transform.TranslateX + (e.Position.X - (Canvas.GetLeft(element) + this.ImagePreviewImage.ActualWidth / 2 + transform.TranslateX)) * (1 - e.Delta.Scale);
            transform.TranslateY = transform.TranslateY + (e.Position.Y - (Canvas.GetTop(element) + LayoutHelper.Instance.TitleBarHeight.Value + this.ImagePreviewImage.ActualHeight / 2 + transform.TranslateY)) * (1 - e.Delta.Scale);
            
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
                transform.TranslateY = -(WindowSizeHelper.Instance.ClientHeight - LayoutHelper.Instance.TitleBarHeight.Value) / 2 - (this.ImagePreviewImage.ActualHeight * transform.ScaleY) / 2;
            else if (transform.TranslateY - (this.ImagePreviewImage.ActualHeight * transform.ScaleY) / 2 > WindowSizeHelper.Instance.ClientHeight / 2)
                transform.TranslateY = (WindowSizeHelper.Instance.ClientHeight - LayoutHelper.Instance.TitleBarHeight.Value) / 2 + (this.ImagePreviewImage.ActualHeight * transform.ScaleY) / 2;

            element.RenderTransform = transform;

            e.Handled = true;
        }

        private void ImagePreviewImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void ImagePreviewImage_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var element = this.ImagePreviewImage as UIElement;
            var transform = element.RenderTransform as CompositeTransform;

            if (transform == null)
                transform = new CompositeTransform();

            var scale = 1.0;
            if (transform.ScaleX >= 4 || transform.ScaleY >= 4)
                scale = 1.0;
            else if (transform.ScaleX >= 2 || transform.ScaleY >= 2)
                scale = 4.0;
            else if (transform.ScaleX >= 1 || transform.ScaleY >= 1)
                scale = 2.0;
            else
                scale = 1.0;

            Storyboard storyboard = new Storyboard();
            DoubleAnimation scaleAnimX = new DoubleAnimation() { To = scale, Duration = new Duration(TimeSpan.FromMilliseconds(200)), EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut } };
            storyboard.Children.Add(scaleAnimX);
            Storyboard.SetTarget(scaleAnimX, element);
            Storyboard.SetTargetProperty(scaleAnimX, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
            DoubleAnimation scaleAnimY = new DoubleAnimation() { To = scale, Duration = new Duration(TimeSpan.FromMilliseconds(200)), EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut } };
            storyboard.Children.Add(scaleAnimY);
            Storyboard.SetTarget(scaleAnimY, element);
            Storyboard.SetTargetProperty(scaleAnimY, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");

            var translateAnimXVal = transform.TranslateX + (e.GetPosition(Window.Current.Content).X - (Canvas.GetLeft(element) + this.ImagePreviewImage.ActualWidth / 2 + transform.TranslateX)) * (1 - scale / transform.ScaleX);
            var translateAnimYVal = transform.TranslateY + (e.GetPosition(Window.Current.Content).Y - (Canvas.GetTop(element) + LayoutHelper.Instance.TitleBarHeight.Value + this.ImagePreviewImage.ActualHeight / 2 + transform.TranslateY)) * (1 - scale / transform.ScaleY);
            if (scale <= 1.0)
            {
                translateAnimYVal = 0.0;
                translateAnimXVal = 0.0;
            }

            DoubleAnimation translateAnimX = new DoubleAnimation() { To = translateAnimXVal, Duration = new Duration(TimeSpan.FromMilliseconds(200)), EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut } };
            storyboard.Children.Add(translateAnimX);
            Storyboard.SetTarget(translateAnimX, element);
            Storyboard.SetTargetProperty(translateAnimX, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
            DoubleAnimation translateAnimY = new DoubleAnimation() { To = translateAnimYVal, Duration = new Duration(TimeSpan.FromMilliseconds(200)), EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut } };
            storyboard.Children.Add(translateAnimY);
            Storyboard.SetTarget(translateAnimY, element);
            Storyboard.SetTargetProperty(translateAnimY, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");
            
            storyboard.Begin();
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
                var extension = "";
                var imageFileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                var client = new HttpClient();
                var response = await client.GetAsync(new Uri(this.Images[this.ImageIndex].MediaUrl));
                switch (response.Content.Headers.ContentType.MediaType)
                {
                    case "image/jpeg":
                        imageFileName += ".jpg";
                        extension = ".jpg";
                        break;
                    case "image/gif":
                        imageFileName += ".gif";
                        extension = ".gif";
                        break;
                    case "image/png":
                        imageFileName += ".png";
                        extension = ".png";
                        break;
                    case "image/tiff":
                        imageFileName += ".tiff";
                        extension = ".tiff";
                        break;
                    case "image/x-bmp":
                        imageFileName += ".bmp";
                        extension = ".bmp";
                        break;
                }


                StorageFile imageFile = null;
                switch (SettingService.Setting.PictureSavePath)
                {
                    case 0:
                        imageFile = await KnownFolders.PicturesLibrary.CreateFileAsync(imageFileName, CreationCollisionOption.GenerateUniqueName);
                        break;
                    case 1:
                        var folders = await KnownFolders.PicturesLibrary.GetFoldersAsync();
                        var storage = folders.Any(x => x.Name == "Flantter") ? folders.First(x => x.Name == "Flantter") : await KnownFolders.PicturesLibrary.CreateFolderAsync("Flantter");
                        imageFile = await storage.CreateFileAsync(imageFileName, CreationCollisionOption.GenerateUniqueName);
                        break;
                    case 2:
                        var filePicker = new FileSavePicker();
                        filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                        filePicker.FileTypeChoices.Add("ImageFile", new List<string>() { extension });
                        filePicker.SuggestedFileName = imageFileName;
                        imageFile = await filePicker.PickSaveFileAsync();
                        break;
                }
                
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
