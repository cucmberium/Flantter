using System;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class VideoPreviewPopup : UserControl, IContentPopup
    {
        private AppBar _bottomAppBar;
        private readonly Popup _videoPreview;

        public VideoPreviewPopup()
        {
            InitializeComponent();

            _videoPreview = new Popup
            {
                Child = this,
                IsLightDismissEnabled = false,
                Opacity = 1
            };

            _videoPreview.Loaded += (_, __) =>
            {
                var page = (Window.Current.Content as Frame).Content as Page;
                _bottomAppBar = page.Tag as AppBar;

                if (_bottomAppBar == null)
                    return;

                _isBottomBarOpen = _bottomAppBar.IsOpen;
                _bottomAppBar.Opening += (s, e) =>
                {
                    _isBottomBarOpen = true;
                    VideoPreviewPopup_LayoutRefresh();
                };
                _bottomAppBar.Closed += (s, e) =>
                {
                    _isBottomBarOpen = false;
                    VideoPreviewPopup_LayoutRefresh();
                };
            };

            Window.Current.SizeChanged += (s, e) => VideoPreviewPopup_LayoutRefresh();
            DisplayInformation.GetForCurrentView().OrientationChanged += (s, e) => VideoPreviewPopup_LayoutRefresh();

            VideoPreviewPopup_LayoutRefresh();
        }

        private bool _isSmallView;
        private bool _isBottomBarOpen;

        public string VideoWebUrl { get; set; }
        public string VideoThumbnailUrl { get; set; }
        public string VideoType { get; set; }
        public string VideoContentType { get; set; }
        public string Id { get; set; }

        public bool IsOpen => _videoPreview.IsOpen;

        public void Show()
        {
            VideoPreviewWebView.Visibility = Visibility.Visible;
            VideoPreviewSymbolIcon.Visibility = Visibility.Collapsed;
            _videoPreview.IsOpen = true;
            Focus(FocusState.Programmatic);
        }

        public void Hide()
        {
            _videoPreview.IsOpen = false;
            VideoPreviewWebView.NavigateToString("<html></html>");

            _isSmallView = false;

            VideoPreviewSmallViewTriangleButton.Visibility = Visibility.Visible;
            VideoPreviewLargeViewTriangleButton.Visibility = Visibility.Collapsed;

            VideoPreviewPopup_LayoutRefresh();
        }

        private void VideoPreviewPopup_LayoutRefresh()
        {
            if (_isSmallView)
            {
                double videoWidth;
                double bottomMargin;
                double rightMargin;
                if (WindowSizeHelper.Instance.WindowHeight < 500.0)
                {
                    bottomMargin = 64.0 + 30.0;
                    rightMargin = 0.0;

                    videoWidth = WindowSizeHelper.Instance.WindowHeight * 16 / 9 * 0.6;
                }
                else if (WindowSizeHelper.Instance.ClientWidth < 384.0)
                {
                    bottomMargin = 64.0 + 30.0;
                    rightMargin = 0.0;

                    videoWidth = WindowSizeHelper.Instance.ClientWidth;
                }
                else if (WindowSizeHelper.Instance.ClientWidth < 500.0)
                {
                    bottomMargin = 64.0 + 10.0 + 30.0;
                    rightMargin = 10.0;

                    videoWidth = WindowSizeHelper.Instance.ClientWidth - 5.0 * 2 - 10.0;
                }
                else
                {
                    bottomMargin = 75.0 + 10.0 + 30.0;
                    rightMargin = 10.0;

                    videoWidth = Math.Max((WindowSizeHelper.Instance.ClientWidth - 5.0 * 2) / 2.0 - 10.0, 480.0);
                    if (videoWidth > 640)
                        videoWidth = 640;
                }
                var videoHeight = videoWidth * 9 / 16;

                if (_isBottomBarOpen)
                    bottomMargin = (_bottomAppBar.Content as FrameworkElement).ActualHeight;

                Canvas.SetTop(_videoPreview,
                    WindowSizeHelper.Instance.ClientHeight - videoHeight - bottomMargin +
                    WindowSizeHelper.Instance.VisibleBounds.Top);
                Canvas.SetLeft(_videoPreview,
                    WindowSizeHelper.Instance.ClientWidth - videoWidth - rightMargin +
                    WindowSizeHelper.Instance.VisibleBounds.Left);

                Width = videoWidth;
                Height = videoHeight;

                VideoPreviewWebView.Width = videoWidth;
                VideoPreviewWebView.Height = videoHeight;
            }
            else
            {
                Canvas.SetTop(_videoPreview, WindowSizeHelper.Instance.VisibleBounds.Top);
                Canvas.SetLeft(_videoPreview, WindowSizeHelper.Instance.VisibleBounds.Left);

                Width = WindowSizeHelper.Instance.ClientWidth;
                Height = WindowSizeHelper.Instance.ClientHeight;

                var videoWidth = WindowSizeHelper.Instance.ClientWidth;
                if (WindowSizeHelper.Instance.ClientWidth - 20 > 960)
                    videoWidth = 960;
                else if (WindowSizeHelper.Instance.ClientWidth - 20 > 400)
                    videoWidth = WindowSizeHelper.Instance.ClientWidth - 20;

                var videoHeight = videoWidth * 9 / 16;
                if (videoHeight > WindowSizeHelper.Instance.ClientHeight)
                {
                    videoHeight = WindowSizeHelper.Instance.ClientHeight;
                    videoWidth = videoHeight * 16 / 9;
                }

                if (VideoType == "Vine")
                {
                    var size = videoWidth;
                    if (size > 600)
                        size = 600;
                    if (size > WindowSizeHelper.Instance.ClientHeight)
                        size = WindowSizeHelper.Instance.ClientHeight;

                    videoHeight = size;
                    videoWidth = size;
                }
                else if (VideoType == "Twitter")
                {
                    if (videoWidth > 720)
                        videoWidth = 720;

                    videoHeight = videoWidth * 9 / 16;
                }

                VideoPreviewWebView.Width = videoWidth;
                VideoPreviewWebView.Height = videoHeight;
            }
        }

        public void VideoChanged()
        {
            VideoPreviewWebView.NavigateToString("<html></html>");

            _isSmallView = false;

            VideoPreviewPopup_LayoutRefresh();

            if (VideoType == "Vine")
            {
                VideoPreviewSmallViewTriangleButton.Visibility = Visibility.Collapsed;

                VideoPreviewWebView.Navigate(new Uri("https://vine.co/v/" + Id + "/embed/simple?audio=1"));
            }
            else if (VideoType == "Twitter")
            {
                var html =
                    "<html><head><link href=\"http://vjs.zencdn.net/5.8.8/video-js.css\" rel=\"stylesheet\"><style type=\"text/css\"> \n body {{ margin: 0; }} \n #twitter {{ position: absolute; top: 0; left: 0; width:100%; height:100%; overflow: hidden; }} \n</style></head>";
                html += "<body><script src=\"http://vjs.zencdn.net/5.8.8/video.js\"></script>";
                html +=
                    "<video id=\"twitter\" class=\"video-js vjs-default-skin vjs-big-play-centered\" controls autoplay loop preload=\"auto\" width=\"auto\" height=\"auto\" poster=\"{0}\" data-setup=\"{{}}\"><source src=\"{1}\" type=\"{2}\"></video>";
                html += "</body></html>";
                VideoPreviewWebView.NavigateToString(string.Format(html, VideoThumbnailUrl, Id, VideoContentType));
            }
            else if (VideoType == "Youtube")
            {
                var html =
                    "<html><head><style type=\"text/css\"> \n body {{ margin: 0; }} .video-container {{ position: relative; padding-bottom: 56.25%; padding-top: 0; height: 0; overflow: hidden; }} .video-container iframe {{ position: absolute; top: 0; left: 0; width: 100%; height: 100%; }} \n </style></head>";
                html +=
                    "<body><div class=\"video-container\"><iframe width=\"960\" height=\"540\" src=\"https://www.youtube.com/embed/{0}?html5=1\" frameborder=\"0\"></iframe></div></body></html>";
                VideoPreviewWebView.NavigateToString(string.Format(html, Id));
            }
            else
            {
                VideoPreviewWebView.Navigate(new Uri("http://embed.nicovideo.jp/watch/" + Id));
            }
        }

        private async void VideoPreviewMenu_ShowinBrowser(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(VideoWebUrl));
        }

        private void VideoPreviewTriangleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(VideoPreviewTriangleButton);

            e.Handled = true;
        }

        private void VideoPreviewGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Hide();
            e.Handled = true;
        }

        private void VideoPreviewSmallViewTriangleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _isSmallView = true;

            VideoPreviewSmallViewTriangleButton.Visibility = Visibility.Collapsed;
            VideoPreviewLargeViewTriangleButton.Visibility = Visibility.Visible;

            VideoPreviewPopup_LayoutRefresh();

            e.Handled = true;
        }

        private void VideoPreviewLargeViewTriangleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _isSmallView = false;

            VideoPreviewSmallViewTriangleButton.Visibility = Visibility.Visible;
            VideoPreviewLargeViewTriangleButton.Visibility = Visibility.Collapsed;

            VideoPreviewPopup_LayoutRefresh();

            e.Handled = true;
        }

        private void VideoPreviewMenu_Close(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}