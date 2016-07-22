using CoreTweet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Flantter.MilkyWay.Views.Contents.Authorize
{
    public class AccountInfo
    {
        public string ScreenName { get; set; }
        public long UserId { get; set; }
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }

    public enum AuthorizeStep
    {
        Config,
        Authorize,
        Exit,
    }

    public sealed partial class AuthorizePopup : UserControl
    {
        private const string ConsumerKey = "qSlXQa4PGPArQVjwajG0tg";
        private const string ConsumerSecret = "h3s83WfKRE2UzwFwFMDMxZN8r4pkDzgsDG7kKr0ZhgI";

        private Popup contentPopup;
        private ResourceLoader resourceLoader;

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
        }


        private void SizeChanced_Event(object sender, WindowSizeChangedEventArgs e)
        {
            this.Height = Window.Current.Bounds.Height;
            this.Width = Window.Current.Bounds.Width;
        }


        private OAuth.OAuthSession oAuthSettion;
        private AuthorizeStep authorizeStep = AuthorizeStep.Exit;

        private void AuthorizePopup_HeaderBackButton_Click(object sender, RoutedEventArgs e)
        {
            switch (authorizeStep)
            {
                case AuthorizeStep.Authorize:
                    VisualStateManager.GoToState(this, "Config", true);
                    authorizeStep = AuthorizeStep.Config;
                    break;
                case AuthorizeStep.Config:
                    authorizeStep = AuthorizeStep.Exit;
                    break;
            }
        }

        private async void AuthorizePopup_CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://twitter.com/signup"));
        }

        private bool urlCallbackAuthorization = false;
        private async void AuthorizePopup_ConfigNextButton_Click(object sender, RoutedEventArgs e)
        {
            var cKey = string.Empty;
            var cSecret = string.Empty;
            var callbackUrl = string.Empty;

            // CKCSの設定を取得
            if (AuthorizePopup_CustomCKCSCheckBox.IsChecked == true)
            {
                cKey = AuthorizePopup_CustomCKTextBox.Text.Trim();
                cSecret = AuthorizePopup_CustomCSTextBox.Text.Trim();
                callbackUrl = "oob";
                urlCallbackAuthorization = false;
            }
            else
            {
                cKey = ConsumerKey;
                cSecret = ConsumerSecret;
                callbackUrl = "http://cucmber.net/";
                urlCallbackAuthorization = true;
            }

            if (string.IsNullOrWhiteSpace(cKey) || string.IsNullOrWhiteSpace(cSecret))
            {
                await new MessageDialog(resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                return;
            }

            // 認証開始
            AuthorizePopup_ConfigNextButton.IsEnabled = false;

            try
            {
                oAuthSettion = await OAuth.AuthorizeAsync(cKey, cSecret, callbackUrl);
            }
            catch (Exception ex)
            {
                await new MessageDialog(resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                return;
            }
            finally
            {
                AuthorizePopup_ConfigNextButton.IsEnabled = true;
            }

            VisualStateManager.GoToState(this, "Authorize", true);
            VisualStateManager.GoToState(this, "AuthorizeNormal", true);

            AuthorizePopup_AuthorizeWebView.Navigate(oAuthSettion.AuthorizeUri);

            authorizeStep = AuthorizeStep.Authorize;
        }

        private async void AuthorizePopup_AuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
            var pin = AuthorizePopup_AuthorizePinTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(pin))
            {
                await new MessageDialog(resourceLoader.GetString("AuthorizePopup_PincodeIsEmpty")).ShowAsync();
                return;
            }

            AuthorizePopup_AuthorizeButton.IsEnabled = false;

            Tokens tokens = null;
            try
            {
                tokens = await oAuthSettion.GetTokensAsync(pin);
            }
            catch (Exception ex)
            {
                await new MessageDialog(resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                return;
            }
            finally
            {
                AuthorizePopup_AuthorizeButton.IsEnabled = true;
            }
            
            accountInfo = new AccountInfo() { ConsumerKey = tokens.ConsumerKey, ConsumerSecret = tokens.ConsumerSecret, AccessToken = tokens.AccessToken, AccessTokenSecret = tokens.AccessTokenSecret, ScreenName = tokens.ScreenName, UserId = tokens.UserId };

            authorizeStep = AuthorizeStep.Exit;
        }

        private void AuthorizePopup_AuthorizeWebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (authorizeStep == AuthorizeStep.Authorize)
            {
                if (!urlCallbackAuthorization)
                {
                    VisualStateManager.GoToState(this, "AuthorizeWithPin", true);
                    return;
                }

                var url = args.Uri.AbsoluteUri;
                if (url.StartsWith("http://cucmber.net/?oauth_token="))
                {
                    args.Cancel = true;
                    var match = Regex.Match(url, @"^http://cucmber.net/\?oauth_token=(?<OauthToken>.*)&oauth_verifier=(?<OauthVerifier>.*)$");
                    this.AuthorizeWithCallback(match.Groups["OauthToken"].Value, match.Groups["OauthVerifier"].Value);
                }
            }
        }

        private async void AuthorizeWithCallback(string oauthToken, string oauthVerifier)
        {
            Tokens tokens = null;
            try
            {
                tokens = await oAuthSettion.GetTokensAsync(oauthVerifier);
            }
            catch (Exception ex)
            {
                await new MessageDialog(resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                return;
            }
            finally
            {
                AuthorizePopup_AuthorizeButton.IsEnabled = true;
            }

            accountInfo = new AccountInfo() { ConsumerKey = tokens.ConsumerKey, ConsumerSecret = tokens.ConsumerSecret, AccessToken = tokens.AccessToken, AccessTokenSecret = tokens.AccessTokenSecret, ScreenName = tokens.ScreenName, UserId = tokens.UserId };

            authorizeStep = AuthorizeStep.Exit;
        }

        private AccountInfo accountInfo = null;
        public async Task<AccountInfo> ShowAsync()
        {
            this.Height = Window.Current.Bounds.Height;
            this.Width = Window.Current.Bounds.Width;
            Window.Current.SizeChanged += SizeChanced_Event;

            accountInfo = null;
            contentPopup.IsOpen = true;
            authorizeStep = AuthorizeStep.Config;
            await Task.Run(() =>
            {
                while (authorizeStep != AuthorizeStep.Exit)
                    Task.Delay(200).Wait();
            });
            contentPopup.IsOpen = false;

            Window.Current.SizeChanged -= SizeChanced_Event;

            if (accountInfo != null)
                await new MessageDialog(resourceLoader.GetString("AuthorizePopup_AuthorizeCompleted")).ShowAsync();

            return accountInfo;
        }

    }
}
