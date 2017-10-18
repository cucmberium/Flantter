using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;
using Flantter.MilkyWay.Models.Notifications;
using Flantter.MilkyWay.Models.Apis.Objects;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class ImagePreviewPopup : UserControl, IContentPopup
    {
        private readonly ResourceLoader _resourceLoader;

        private bool _imageOpened;
        private readonly Popup _imagePreview;

        public ImagePreviewPopup()
        {
            InitializeComponent();

            _resourceLoader = new ResourceLoader();

            KeyDown += ImagePreviewPopup_KeyDown;
            Window.Current.SizeChanged += (s, e) => ImagePreviewPopup_LayoutRefresh();
            DisplayInformation.GetForCurrentView().OrientationChanged += (s, e) => ImagePreviewPopup_LayoutRefresh();

            _imagePreview = new Popup
            {
                Child = this,
                IsLightDismissEnabled = false,
                Opacity = 1
            };

            Width = WindowSizeHelper.Instance.ClientWidth;
            Height = WindowSizeHelper.Instance.ClientHeight;

            Canvas.SetTop(_imagePreview, WindowSizeHelper.Instance.StatusBarHeight);
            Canvas.SetLeft(_imagePreview, WindowSizeHelper.Instance.StatusBarWidth);
        }

        public List<MediaEntity> Images { get; set; }
        public int ImageIndex { get; set; }

        public bool IsOpen => _imagePreview.IsOpen;

        public void Show()
        {
            Canvas.SetTop(_imagePreview, WindowSizeHelper.Instance.StatusBarHeight);
            Canvas.SetLeft(_imagePreview, WindowSizeHelper.Instance.StatusBarWidth);

            ImagePreviewCanvas.Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, WindowSizeHelper.Instance.ClientWidth, WindowSizeHelper.Instance.ClientHeight)
            };

            if (_imageOpened)
                ImageInitialize();

            _imagePreview.IsOpen = true;
            Focus(FocusState.Programmatic);
        }

        public void Hide()
        {
            _imagePreview.IsOpen = false;
        }

        private void ImagePreviewPopup_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Left)
            {
                e.Handled = true;
                if (ImageIndex <= 0 || Images.Count <= 1)
                    return;

                ImageIndex -= 1;
                ImageRefresh();

                e.Handled = true;
            }
            else if (e.Key == VirtualKey.Right)
            {
                e.Handled = true;
                if (ImageIndex >= Images.Count - 1 || Images.Count <= 1)
                    return;

                ImageIndex += 1;
                ImageRefresh();
            }
            else if (e.Key == VirtualKey.Escape)
            {
                if (!IsOpen)
                    return;

                Hide();
                e.Handled = true;
            }
        }

        private void ImagePreviewPopup_LayoutRefresh()
        {
            Canvas.SetTop(_imagePreview, WindowSizeHelper.Instance.StatusBarHeight);
            Canvas.SetLeft(_imagePreview, WindowSizeHelper.Instance.StatusBarWidth);

            Width = WindowSizeHelper.Instance.ClientWidth;
            Height = WindowSizeHelper.Instance.ClientHeight;

            ImagePreviewCanvas.Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, WindowSizeHelper.Instance.ClientWidth, WindowSizeHelper.Instance.ClientHeight)
            };

            var imageWidth = ((BitmapImage) ImagePreviewImage.Source).PixelWidth;
            var imageHeight = ((BitmapImage) ImagePreviewImage.Source).PixelHeight;
            var windowWidth = WindowSizeHelper.Instance.ClientWidth;
            var windowHeight = WindowSizeHelper.Instance.ClientHeight;

            var raito = 1.0;

            if (imageWidth > windowWidth * 0.95 && imageHeight > windowHeight * 0.95)
            {
                var imageWindowWidthRaito = windowWidth / imageWidth;
                var imageWindowHeightRaito = windowHeight / imageHeight;
                raito =
                    (imageWindowHeightRaito < imageWindowWidthRaito ? imageWindowHeightRaito : imageWindowWidthRaito) *
                    0.95;
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

            ImagePreviewImage.Width = imageWidth * raito;
            ImagePreviewImage.Height = imageHeight * raito;

            var canvasTop = (WindowSizeHelper.Instance.ClientHeight - ImagePreviewImage.Height) / 2;
            var canvasLeft = (WindowSizeHelper.Instance.ClientWidth - ImagePreviewImage.Width) / 2;

            Canvas.SetTop(ImagePreviewImage, canvasTop);
            Canvas.SetLeft(ImagePreviewImage, canvasLeft);
        }

        public void ImageRefresh()
        {
            ImagePreviewProgressRing.Visibility = Visibility.Visible;
            ImagePreviewProgressRing.IsActive = true;
            ImagePreviewSymbolIcon.Visibility = Visibility.Collapsed;
            ImagePreviewImage.Visibility = Visibility.Collapsed;
            // こっちのほうが画像がキャッシュされるような気がする(気のせい)
            ImagePreviewImage.Source =
                (ImageSource) XamlBindingHelper.ConvertValue(typeof(ImageSource), Images[ImageIndex].MediaUrl);
            _imageOpened = false;

            if (ImageIndex <= 0 || Images.Count <= 1)
                ImagePreviewPreviousButton.Visibility = Visibility.Collapsed;
            else
                ImagePreviewPreviousButton.Visibility = Visibility.Visible;

            if (ImageIndex >= Images.Count - 1 || Images.Count <= 1)
                ImagePreviewNextButton.Visibility = Visibility.Collapsed;
            else
                ImagePreviewNextButton.Visibility = Visibility.Visible;
        }

        public void ImageInitialize()
        {
            var element = ImagePreviewImage as UIElement;
            var transform = element.RenderTransform as CompositeTransform ?? new CompositeTransform();

            transform.TranslateX = 0.0;
            transform.TranslateY = 0.0;

            transform.ScaleX = 1.0;
            transform.ScaleY = 1.0;

            element.RenderTransform = transform;

            var imageWidth = ((BitmapImage) ImagePreviewImage.Source).PixelWidth;
            var imageHeight = ((BitmapImage) ImagePreviewImage.Source).PixelHeight;
            var windowWidth = WindowSizeHelper.Instance.ClientWidth;
            var windowHeight = WindowSizeHelper.Instance.ClientHeight;

            var raito = 1.0;

            if (imageWidth > windowWidth * 0.95 && imageHeight > windowHeight * 0.95)
            {
                var imageWindowWidthRaito = windowWidth / imageWidth;
                var imageWindowHeightRaito = windowHeight / imageHeight;
                raito =
                    (imageWindowHeightRaito < imageWindowWidthRaito ? imageWindowHeightRaito : imageWindowWidthRaito) *
                    0.95;
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

            ImagePreviewImage.Width = imageWidth * raito;
            ImagePreviewImage.Height = imageHeight * raito;

            var canvasTop = (windowHeight - ImagePreviewImage.Height) / 2;
            var canvasLeft = (windowWidth - ImagePreviewImage.Width) / 2;

            Canvas.SetTop(ImagePreviewImage, canvasTop);
            Canvas.SetLeft(ImagePreviewImage, canvasLeft);
        }

        private void ImagePreviewImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            _imageOpened = true;

            ImagePreviewProgressRing.Visibility = Visibility.Collapsed;
            ImagePreviewProgressRing.IsActive = false;
            ImagePreviewSymbolIcon.Visibility = Visibility.Collapsed;
            ImagePreviewImage.Visibility = Visibility.Collapsed;

            ImageInitialize();

            ImagePreviewImage.Visibility = Visibility.Visible;
        }

        private void ImagePreviewImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ImagePreviewProgressRing.Visibility = Visibility.Collapsed;
            ImagePreviewProgressRing.IsActive = false;
            ImagePreviewSymbolIcon.Visibility = Visibility.Visible;
            ImagePreviewImage.Visibility = Visibility.Collapsed;
        }

        private void ImagePreviewGrid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var element = ImagePreviewImage as UIElement;
            var transform = element.RenderTransform as CompositeTransform ?? new CompositeTransform();

            if (e.GetCurrentPoint(element).Properties.MouseWheelDelta > 0)
            {
                if (transform.ScaleX >= 5 || transform.ScaleY >= 5)
                    return;

                transform.ScaleX *= 1.07;
                transform.ScaleY *= 1.07;

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

                transform.ScaleX *= 1.0 / 1.07;
                transform.ScaleY *= 1.0 / 1.07;

                if (transform.ScaleX <= 0.30 || transform.ScaleY <= 0.30)
                {
                    transform.ScaleX = 0.30;
                    transform.ScaleY = 0.30;
                }
            }

            if (transform.TranslateX + ImagePreviewImage.ActualWidth * transform.ScaleX / 2 <
                -WindowSizeHelper.Instance.ClientWidth / 2)
                transform.TranslateX = -WindowSizeHelper.Instance.ClientWidth / 2 -
                                       ImagePreviewImage.ActualWidth * transform.ScaleX / 2;
            else if (transform.TranslateX - ImagePreviewImage.ActualWidth * transform.ScaleX / 2 >
                     WindowSizeHelper.Instance.ClientWidth / 2)
                transform.TranslateX = WindowSizeHelper.Instance.ClientWidth / 2 +
                                       ImagePreviewImage.ActualWidth * transform.ScaleX / 2;

            if (transform.TranslateY + ImagePreviewImage.ActualHeight * transform.ScaleY / 2 <
                -WindowSizeHelper.Instance.ClientHeight / 2)
                transform.TranslateY = -WindowSizeHelper.Instance.ClientHeight / 2 -
                                       ImagePreviewImage.ActualHeight * transform.ScaleY / 2;
            else if (transform.TranslateY - ImagePreviewImage.ActualHeight * transform.ScaleY / 2 >
                     WindowSizeHelper.Instance.ClientHeight / 2)
                transform.TranslateY = WindowSizeHelper.Instance.ClientHeight / 2 +
                                       ImagePreviewImage.ActualHeight * transform.ScaleY / 2;

            element.RenderTransform = transform;

            e.Handled = true;
        }

        private void ImagePreviewGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Hide();
            e.Handled = true;
        }

        private void ImagePreviewCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var element = ImagePreviewImage as UIElement;
            var transform = element.RenderTransform as CompositeTransform ?? new CompositeTransform();

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

            transform.TranslateX = transform.TranslateX + (e.Position.X -
                                                           (Canvas.GetLeft(element) +
                                                            ImagePreviewImage.ActualWidth / 2 + transform.TranslateX)) *
                                   (1 - e.Delta.Scale);
            transform.TranslateY = transform.TranslateY + (e.Position.Y -
                                                           (Canvas.GetTop(element) +
                                                            ImagePreviewImage.ActualHeight / 2 +
                                                            transform.TranslateY)) * (1 - e.Delta.Scale);

            element.RenderTransform = transform;

            e.Handled = true;
        }

        private void ImagePreviewCanvas_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var element = ImagePreviewImage as UIElement;
            var transform = element.RenderTransform as CompositeTransform ?? new CompositeTransform();

            if (transform.TranslateX + ImagePreviewImage.ActualWidth * transform.ScaleX / 2 <
                -WindowSizeHelper.Instance.ClientWidth / 2)
                transform.TranslateX = -WindowSizeHelper.Instance.ClientWidth / 2 -
                                       ImagePreviewImage.ActualWidth * transform.ScaleX / 2;
            else if (transform.TranslateX - ImagePreviewImage.ActualWidth * transform.ScaleX / 2 >
                     WindowSizeHelper.Instance.ClientWidth / 2)
                transform.TranslateX = WindowSizeHelper.Instance.ClientWidth / 2 +
                                       ImagePreviewImage.ActualWidth * transform.ScaleX / 2;

            if (transform.TranslateY + ImagePreviewImage.ActualHeight * transform.ScaleY / 2 <
                -WindowSizeHelper.Instance.ClientHeight / 2)
                transform.TranslateY = -WindowSizeHelper.Instance.ClientHeight / 2 -
                                       ImagePreviewImage.ActualHeight * transform.ScaleY / 2;
            else if (transform.TranslateY - ImagePreviewImage.ActualHeight * transform.ScaleY / 2 >
                     WindowSizeHelper.Instance.ClientHeight / 2)
                transform.TranslateY = WindowSizeHelper.Instance.ClientHeight / 2 +
                                       ImagePreviewImage.ActualHeight * transform.ScaleY / 2;

            element.RenderTransform = transform;

            e.Handled = true;
        }

        private void ImagePreviewImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void ImagePreviewImage_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var element = ImagePreviewImage as UIElement;
            var transform = element.RenderTransform as CompositeTransform ?? new CompositeTransform();

            double scale;
            if (transform.ScaleX >= 4 || transform.ScaleY >= 4)
                scale = 1.0;
            else if (transform.ScaleX >= 2 || transform.ScaleY >= 2)
                scale = 4.0;
            else if (transform.ScaleX >= 1 || transform.ScaleY >= 1)
                scale = 2.0;
            else
                scale = 1.0;

            var storyboard = new Storyboard();
            var scaleAnimX = new DoubleAnimation
            {
                To = scale,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                EasingFunction = new PowerEase {EasingMode = EasingMode.EaseInOut}
            };
            storyboard.Children.Add(scaleAnimX);
            Storyboard.SetTarget(scaleAnimX, element);
            Storyboard.SetTargetProperty(scaleAnimX, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
            var scaleAnimY = new DoubleAnimation
            {
                To = scale,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                EasingFunction = new PowerEase {EasingMode = EasingMode.EaseInOut}
            };
            storyboard.Children.Add(scaleAnimY);
            Storyboard.SetTarget(scaleAnimY, element);
            Storyboard.SetTargetProperty(scaleAnimY, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");

            var translateAnimXVal = transform.TranslateX +
                                    (e.GetPosition(Window.Current.Content).X -
                                     (WindowSizeHelper.Instance.StatusBarWidth + Canvas.GetLeft(element) +
                                      ImagePreviewImage.ActualWidth / 2 + transform.TranslateX)) *
                                    (1 - scale / transform.ScaleX);
            var translateAnimYVal = transform.TranslateY +
                                    (e.GetPosition(Window.Current.Content).Y -
                                     (WindowSizeHelper.Instance.StatusBarHeight + Canvas.GetTop(element) +
                                      ImagePreviewImage.ActualHeight / 2 + transform.TranslateY)) *
                                    (1 - scale / transform.ScaleY);
            if (scale <= 1.0)
            {
                translateAnimYVal = 0.0;
                translateAnimXVal = 0.0;
            }

            var translateAnimX = new DoubleAnimation
            {
                To = translateAnimXVal,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                EasingFunction = new PowerEase {EasingMode = EasingMode.EaseInOut}
            };
            storyboard.Children.Add(translateAnimX);
            Storyboard.SetTarget(translateAnimX, element);
            Storyboard.SetTargetProperty(translateAnimX, "(UIElement.RenderTransform).(CompositeTransform.TranslateX)");
            var translateAnimY = new DoubleAnimation
            {
                To = translateAnimYVal,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                EasingFunction = new PowerEase {EasingMode = EasingMode.EaseInOut}
            };
            storyboard.Children.Add(translateAnimY);
            Storyboard.SetTarget(translateAnimY, element);
            Storyboard.SetTargetProperty(translateAnimY, "(UIElement.RenderTransform).(CompositeTransform.TranslateY)");

            storyboard.Begin();
        }

        private void ImagePreviewPreviousButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageIndex -= 1;
            ImageRefresh();

            e.Handled = true;
        }

        private void ImagePreviewNextButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ImageIndex += 1;
            ImageRefresh();

            e.Handled = true;
        }

        private async void ImagePreviewMenu_SaveImage(object sender, RoutedEventArgs e)
        {
            try
            {
                var extension = "";
                var imageFileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                var client = new HttpClient();
                var response = await client.GetAsync(new Uri(Images[ImageIndex].MediaUrl));
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
                        imageFile = await KnownFolders.PicturesLibrary.CreateFileAsync(imageFileName,
                            CreationCollisionOption.GenerateUniqueName);
                        break;
                    case 1:
                        var folders = await KnownFolders.PicturesLibrary.GetFoldersAsync();
                        var storage = folders.Any(x => x.Name == "Flantter")
                            ? folders.First(x => x.Name == "Flantter")
                            : await KnownFolders.PicturesLibrary.CreateFolderAsync("Flantter");
                        imageFile =
                            await storage.CreateFileAsync(imageFileName, CreationCollisionOption.GenerateUniqueName);
                        break;
                    case 2:
                        var filePicker = new FileSavePicker();
                        filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                        filePicker.FileTypeChoices.Add("ImageFile", new List<string> {extension});
                        filePicker.SuggestedFileName = imageFileName;
                        imageFile = await filePicker.PickSaveFileAsync();
                        break;
                }

                await FileIO.WriteBytesAsync(imageFile, (await response.Content.ReadAsBufferAsync()).ToArray());
            }
            catch
            {
                Core.Instance.PopupToastNotification(PopupNotificationType.System, _resourceLoader.GetString("ImagePreviewPopup_FailedtoImageSave"));
                return;
            }

            Core.Instance.PopupToastNotification(PopupNotificationType.System, _resourceLoader.GetString("ImagePreviewPopup_ImageSavedSuccessfuly"));
        }

        private async void ImagePreviewMenu_ShowinBrowser(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(Images[ImageIndex].ExpandedUrl));
        }

        private async void ImagePreviewMenu_SearchSimilarImage(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.google.co.jp/searchbyimage?image_url=" +
                                                  Images[ImageIndex].MediaUrl));
        }

        private void ImagePreviewMenu_Close(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void ImagePreviewTriangleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(ImagePreviewTriangleButton);

            e.Handled = true;
        }
    }
}