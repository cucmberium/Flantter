using CoreTweet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Flantter.MilkyWay.Views.Contents
{
    public class Account
    {
        public string ScreenName { get; set; }
        public long UserId { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }

    public sealed partial class AuthorizePopup : UserControl
    {


        private Popup contentPopup;
        private OAuth.OAuthSession authorize;
        private ResourceLoader resourceLoader;
        private const string ConsumerKey = "qSlXQa4PGPArQVjwajG0tg";
        private const string ConsumerSecret = "h3s83WfKRE2UzwFwFMDMxZN8r4pkDzgsDG7kKr0ZhgI";

        public AuthorizePopup()
        {
            this.InitializeComponent();
            resourceLoader = new ResourceLoader();

            contentPopup = new Popup()
            {
                Child = this,
                IsLightDismissEnabled = false,
                HorizontalOffset = 0.0,
                VerticalOffset = 0.0,
            };

            this.Height = Window.Current.Bounds.Height;
            this.Width = Window.Current.Bounds.Width;

            Window.Current.SizeChanged += SizeChanced_Event;
        }

        private void SizeChanced_Event(object sender, WindowSizeChangedEventArgs e)
        {
            this.Height = Window.Current.Bounds.Height;
            this.Width = Window.Current.Bounds.Width;
        }

        private void AuthorizePopup_HeaderBackButton_Click(object sender, RoutedEventArgs e)
        {
            if (AuthorizePopup_MainContentGrid.Visibility == Visibility.Collapsed && AuthorizePopup_AuthorizeGrid.Visibility == Visibility.Visible)
            {
                AuthorizePopup_MainContentGrid.Visibility = Visibility.Visible;
                AuthorizePopup_AuthorizeGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                authorizeCompleted = true;
            }
        }

        private void AuthorizePopup_CustomCKCSCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (AuthorizePopup_CustomCKCSCheckBox.IsChecked == true)
                AuthorizePopup_CustomCKCSStackPanel.Visibility = Visibility.Visible;
            else
                AuthorizePopup_CustomCKCSStackPanel.Visibility = Visibility.Collapsed;
        }

        private async void AuthorizePopup_MainContentNextButton_Click(object sender, RoutedEventArgs e)
        {
            var cKey = ConsumerKey;
            var cSecret = ConsumerSecret;

            if (AuthorizePopup_CustomCKCSCheckBox.IsChecked == true)
            {
                cKey = AuthorizePopup_CustomConsumerKeyTextBox.Text;
                cSecret = AuthorizePopup_CustomConsumerSecretTextBox.Text;
            }

            AuthorizePopup_MainContentNextButton.IsEnabled = false;

            try
            {
                authorize = await OAuth.AuthorizeAsync(ConsumerKey, ConsumerSecret);
            }
            catch (Exception ex)
            {
                new MessageDialog(resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                AuthorizePopup_MainContentNextButton.IsEnabled = true;
                return;
            }

            AuthorizePopup_MainContentNextButton.IsEnabled = true;
            AuthorizePopup_MainContentGrid.Visibility = Visibility.Collapsed;
            AuthorizePopup_AuthorizeGrid.Visibility = Visibility.Visible;

            AuthorizePopup_AuthorizeWebView.Navigate(authorize.AuthorizeUri);
        }

        private async void AuthorizePopup_AuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
            var pincode = AuthorizePopup_AuthorizePincodeTextBox.Text;
            if (string.IsNullOrWhiteSpace(pincode))
            {
                await new MessageDialog(resourceLoader.GetString("AuthorizePopup_PincodeIsEmpty")).ShowAsync();
                return;
            }

            AuthorizePopup_AuthorizeButton.IsEnabled = false;

            try
            {
                var tokens = await OAuth.GetTokensAsync(authorize, pincode);
                accountInfo = new Account() { ConsumerKey = tokens.ConsumerKey, ConsumerSecret = tokens.ConsumerSecret, AccessToken = tokens.AccessToken, AccessTokenSecret = tokens.AccessTokenSecret, ScreenName = tokens.ScreenName, UserId = tokens.UserId };
            }
            catch (Exception ex)
            {
                new MessageDialog(resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                AuthorizePopup_AuthorizeButton.IsEnabled = true;
                return;
            }

            AuthorizePopup_AuthorizeButton.IsEnabled = true;

            authorizeCompleted = true;
        }

        private Account accountInfo = null;
        private bool authorizeCompleted = false;
        public async Task<Account> ShowAsync()
        {
            accountInfo = null;
            authorizeCompleted = false;
            contentPopup.IsOpen = true;
            await Task.Run(() =>
            {
                while (authorizeCompleted == false)
                    new Task(() => { }).Wait(200);
            });
            contentPopup.IsOpen = false;

            if (accountInfo != null)
                await new MessageDialog(resourceLoader.GetString("AuthorizePopup_AuthorizeCompleted")).ShowAsync();

            return accountInfo;
        }
    }
}
