using Flantter.MilkyWay.Common;
using Flantter.MilkyWay.Models.Twitter.Video;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.Views.Controls;
using Flantter.MilkyWay.Views.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Flantter.MilkyWay.Views.Contents
{
    public sealed partial class VideoPreviewPopup : UserControl, ContentPopup
    {
        private Popup VideoPreview;
        private AppBar _BottomAppBar;
        
        private bool _IsSmallView { get; set; }
        private bool _IsBottomBarOpen { get; set; }

        public string VideoWebUrl { get; set; }
        public string VideoThumbnailUrl { get; set; }
        public string VideoType { get; set; }
        public string VideoContentType { get; set; }
        public string Id { get; set; }

        public bool IsOpen { get { return this.VideoPreview.IsOpen; } }

        public VideoPreviewPopup()
        {
            this.InitializeComponent();

            this.VideoPreview = new Popup
            {
                Child = this,
                IsLightDismissEnabled = false,
                Opacity = 1,
            };

            this.VideoPreview.Loaded += (_, __) =>
            {
                var page = (Window.Current.Content as Frame).Content as Page;
                _BottomAppBar = page.Tag as AppBar;

                if (_BottomAppBar == null)
                    return;

                this._IsBottomBarOpen = _BottomAppBar.IsOpen;
                _BottomAppBar.Opening += (s, e) => { this._IsBottomBarOpen = true; this.VideoPreviewPopup_LayoutRefresh(); };
                _BottomAppBar.Closed += (s, e) => { this._IsBottomBarOpen = false; this.VideoPreviewPopup_LayoutRefresh(); };
            };
            
            Window.Current.SizeChanged += (s, e) => this.VideoPreviewPopup_LayoutRefresh();
            Windows.Graphics.Display.DisplayInformation.GetForCurrentView().OrientationChanged += (s, e) => this.VideoPreviewPopup_LayoutRefresh();

            this.VideoPreviewPopup_LayoutRefresh();
        }

        private void VideoPreviewPopup_LayoutRefresh()
        {
            if (_IsSmallView)
            {
                var videoWidth = WindowSizeHelper.Instance.ClientWidth;
                
                var bottomMargin = 0.0;
                var rightMargin = 0.0;
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

                    videoWidth = (WindowSizeHelper.Instance.ClientWidth - 5.0 * 2) - 10.0;
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

                if (_IsBottomBarOpen)
                    bottomMargin = (_BottomAppBar.Content as FrameworkElement).ActualHeight;

                Canvas.SetTop(this.VideoPreview, WindowSizeHelper.Instance.ClientHeight - videoHeight - bottomMargin + WindowSizeHelper.Instance.StatusBarHeight);
                Canvas.SetLeft(this.VideoPreview, WindowSizeHelper.Instance.ClientWidth - videoWidth - rightMargin + WindowSizeHelper.Instance.StatusBarWidth);

                this.Width = videoWidth;
                this.Height = videoHeight;

                this.VideoPreviewWebView.Width = videoWidth;
                this.VideoPreviewWebView.Height = videoHeight;
            }
            else
            {
                Canvas.SetTop(this.VideoPreview, WindowSizeHelper.Instance.StatusBarHeight);
                Canvas.SetLeft(this.VideoPreview, WindowSizeHelper.Instance.StatusBarWidth);

                this.Width = WindowSizeHelper.Instance.ClientWidth;
                this.Height = WindowSizeHelper.Instance.ClientHeight;

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

                if (this.VideoType == "Vine")
                {
                    var size = videoWidth;
                    if (size > 600)
                        size = 600;
                    if (size > WindowSizeHelper.Instance.ClientHeight)
                        size = WindowSizeHelper.Instance.ClientHeight;
                    
                    videoHeight = size;
                    videoWidth = size;
                }
                else if (this.VideoType == "Twitter")
                {
                    if (videoWidth > 720)
                        videoWidth = 720;

                    videoHeight = videoWidth * 9 / 16;
                }

                this.VideoPreviewWebView.Width = videoWidth;
                this.VideoPreviewWebView.Height = videoHeight;
            }
        }

        public async void VideoChanged()
        {
            this.VideoPreviewWebView.NavigateToString("<html></html>");
            
            _IsSmallView = false;

            this.VideoPreviewPopup_LayoutRefresh();

            if (this.VideoType == "Vine")
            {
                this.VideoPreviewSmallViewTriangleButton.Visibility = Visibility.Collapsed;

                this.VideoPreviewWebView.Navigate(new Uri("https://vine.co/v/" + this.Id + "/embed/simple?audio=1"));
                //var html = "<html><head><style type=\"text/css\"> \n body {{ margin: 0; }} \n</style></head>";
                //html += "<body><iframe class=\"vine-embed\" src=\"https://vine.co/v/{0}/embed/simple?audio=1\" width=\"{1}\" height=\"{2}\" frameborder=\"0\"></iframe><script async src=\"https://platform.vine.co/static/scripts/embed.js\" charset=\"utf-8\"></script></body></html>";
                //this.VideoPreviewWebView.NavigateToString(string.Format(html, this.Id, videoWidth, videoHeight));
            }
            else if (this.VideoType == "Twitter")
            {
                var html = "<html><head><link href=\"http://vjs.zencdn.net/5.8.8/video-js.css\" rel=\"stylesheet\"><style type=\"text/css\"> \n body {{ margin: 0; }} \n #twitter {{ position: absolute; top: 0; left: 0; width:100%; height:100%; overflow: hidden; }} \n</style></head>";
                html += "<body><script src=\"http://vjs.zencdn.net/5.8.8/video.js\"></script>";
                html += "<video id=\"twitter\" class=\"video-js vjs-default-skin vjs-big-play-centered\" controls autoplay loop preload=\"auto\" width=\"auto\" height=\"auto\" poster=\"{0}\" data-setup=\"{{}}\"><source src=\"{1}\" type=\"{2}\"></video>";
                html += "</body></html>";
                this.VideoPreviewWebView.NavigateToString(string.Format(html, this.VideoThumbnailUrl, this.Id, this.VideoContentType));
            }
            else if (this.VideoType == "Youtube")
            {
                var html = "<html><head><style type=\"text/css\"> \n body {{ margin: 0; }} .video-container {{ position: relative; padding-bottom: 56.25%; padding-top: 0; height: 0; overflow: hidden; }} .video-container iframe {{ position: absolute; top: 0; left: 0; width: 100%; height: 100%; }} \n </style></head>";
                html += "<body><div class=\"video-container\"><iframe width=\"960\" height=\"540\" src=\"https://www.youtube.com/embed/{0}?html5=1\" frameborder=\"0\"></iframe></div></body></html>";
                this.VideoPreviewWebView.NavigateToString(string.Format(html, this.Id));
            }
            else
            {
                /*var nicoVideo = new NicoVideo();
                await nicoVideo.GetNicoVideoInfo(this.Id);
                if (string.IsNullOrWhiteSpace(nicoVideo.VideoUrl) || string.IsNullOrWhiteSpace(nicoVideo.VideoCookieUrl))
                {
                    this.VideoPreviewWebView.Visibility = Visibility.Collapsed;
                    this.VideoPreviewSymbolIcon.Visibility = Visibility.Visible;
                    return;
                }
                var html = "<html><head><link href=\"http://vjs.zencdn.net/5.8.8/video-js.css\" rel=\"stylesheet\"><style type=\"text/css\"> \n body {{ margin: 0; }} \n .video-js {{ padding-top: 56.25%; }} \n </style></head>";
                html += "<body><script src=\"http://vjs.zencdn.net/5.8.8/video.js\"></script><div id=\"video\"><video id=\"nicovideo\" class=\"video-js vjs-default-skin vjs-big-play-centered\" controls preload=\"auto\" width=\"auto\" height=\"auto\" poster=\"{0}\" data-setup=\"{{}}\"><source src=\"{1}\" type=\"{2}\"></video></div></body></html>";
                this.VideoPreviewWebView.NavigateToString(string.Format(html, this.VideoThumbnailUrl, nicoVideo.VideoUrl, nicoVideo.VideoContentType));*/

                this.VideoPreviewWebView.Navigate(new Uri("http://embed.nicovideo.jp/watch/" + this.Id));
            }
        }

        public void Show()
        {
            this.VideoPreviewWebView.Visibility = Visibility.Visible;
            this.VideoPreviewSymbolIcon.Visibility = Visibility.Collapsed;
            this.VideoPreview.IsOpen = true;
            this.Focus(FocusState.Programmatic);
        }

        public void Hide()
        {
            this.VideoPreview.IsOpen = false;
            this.VideoPreviewWebView.NavigateToString("<html></html>");

            _IsSmallView = false;

            this.VideoPreviewSmallViewTriangleButton.Visibility = Visibility.Visible;
            this.VideoPreviewLargeViewTriangleButton.Visibility = Visibility.Collapsed;

            this.VideoPreviewPopup_LayoutRefresh();
        }

        private async void VideoPreviewMenu_ShowinBrowser(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(this.VideoWebUrl));
        }

        private void VideoPreviewTriangleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(this.VideoPreviewTriangleButton);

            e.Handled = true;
        }

        private void VideoPreviewGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();
            e.Handled = true;
        }

        private void VideoPreviewSmallViewTriangleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _IsSmallView = true;

            this.VideoPreviewSmallViewTriangleButton.Visibility = Visibility.Collapsed;
            this.VideoPreviewLargeViewTriangleButton.Visibility = Visibility.Visible;

            this.VideoPreviewPopup_LayoutRefresh();

            e.Handled = true;
        }

        private void VideoPreviewLargeViewTriangleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _IsSmallView = false;

            this.VideoPreviewSmallViewTriangleButton.Visibility = Visibility.Visible;
            this.VideoPreviewLargeViewTriangleButton.Visibility = Visibility.Collapsed;

            this.VideoPreviewPopup_LayoutRefresh();

            e.Handled = true;
        }

        private void VideoPreviewMenu_Close(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
