using CoreTweet;
using Mastonet;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Mastonet.Entities;

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
        public string Service { get; set; }
        public string Instance { get; set; }
    }

    public enum AuthorizeStep
    {
        Choice,
        TwitterConfig,
        MastodonConfig,
        TwitterAuthorize,
        MastodonAuthorize,
        Exit,
    }

    public class MastodonOAuth
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Instance { get; set; }

    }

    public sealed partial class AuthorizePopup : UserControl
    {
        private const string TwitterConsumerKey = "qSlXQa4PGPArQVjwajG0tg";
        private const string TwitterConsumerSecret = "h3s83WfKRE2UzwFwFMDMxZN8r4pkDzgsDG7kKr0ZhgI";

        private readonly Popup _contentPopup;
        private readonly ResourceLoader _resourceLoader;

        public AuthorizePopup()
        {
            this.InitializeComponent();

            _resourceLoader = new ResourceLoader();
            _contentPopup = new Popup()
            {
                Child = this,
                IsLightDismissEnabled = false,
                HorizontalOffset = 0.0,
                VerticalOffset = 0.0
            };
        }

        private void SizeChanced_Event(object sender, WindowSizeChangedEventArgs e)
        {
            this.Height = Window.Current.Bounds.Height;
            this.Width = Window.Current.Bounds.Width;
        }
        
        private AuthorizeStep _authorizeStep = AuthorizeStep.Exit;
        private void AuthorizePopupHeaderBackButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_authorizeStep)
            {
                case AuthorizeStep.TwitterAuthorize:
                    VisualStateManager.GoToState(this, "TwitterConfig", true);
                    _authorizeStep = AuthorizeStep.TwitterConfig;
                    break;
                case AuthorizeStep.MastodonAuthorize:
                    VisualStateManager.GoToState(this, "MastodonConfig", true);
                    _authorizeStep = AuthorizeStep.MastodonConfig;
                    break;
                case AuthorizeStep.TwitterConfig:
                case AuthorizeStep.MastodonConfig:
                    VisualStateManager.GoToState(this, "Choice", true);
                    _authorizeStep = AuthorizeStep.Choice;
                    break;
                case AuthorizeStep.Choice:
                    _authorizeStep = AuthorizeStep.Exit;
                    break;
            }
        }

        private async void AuthorizePopupCreateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (_authorizeStep == AuthorizeStep.TwitterConfig)
                await Launcher.LaunchUriAsync(new Uri("https://twitter.com/signup"));
            else if (_authorizeStep == AuthorizeStep.MastodonConfig)
                await Launcher.LaunchUriAsync(new Uri("https://mstdn.jp"));
        }

        private void AuthorizePopupTwitterButton_Click(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "TwitterConfig", true);
            _authorizeStep = AuthorizeStep.TwitterConfig;
        }

        private void AuthorizePopupMastodonButton_Click(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "MastodonConfig", true);
            _authorizeStep = AuthorizeStep.MastodonConfig;
        }

        private MastodonOAuth _mastodonOauthSettion = null;
        private OAuth.OAuthSession _twitterOAuthSettion = null;
        private bool _urlCallbackAuthorization = false;
        private async void AuthorizePopupConfigNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_authorizeStep == AuthorizeStep.TwitterConfig)
            {
                var editConsumerKey = AuthorizePopupTwitterEditConsumerKeyCheckBox.IsChecked;

                var consumerKey = string.Empty;
                var consumerSecret = string.Empty;
                var callbackUrl = string.Empty;
                if (editConsumerKey == true)
                {
                    consumerKey = AuthorizePopupTwitterConsumerKeyTextBox.Text.Trim();
                    consumerSecret = AuthorizePopupTwitterConsumerSecretTextBox.Text.Trim();
                    callbackUrl = "oob";
                    _urlCallbackAuthorization = false;
                }
                else
                {
                    consumerKey = TwitterConsumerKey;
                    consumerSecret = TwitterConsumerSecret;
                    callbackUrl = "http://cucmber.net/";
                    _urlCallbackAuthorization = true;
                }
                
                if (string.IsNullOrWhiteSpace(consumerKey) || string.IsNullOrWhiteSpace(consumerSecret))
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                    return;
                }

                AuthorizePopupConfigNextButton.IsEnabled = false;

                try
                {
                    _twitterOAuthSettion = await OAuth.AuthorizeAsync(consumerKey, consumerSecret, callbackUrl);
                }
                catch
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                    return;
                }
                finally
                {
                    AuthorizePopupConfigNextButton.IsEnabled = true;
                }
                AuthorizePopupAuthorizeWebView.Navigate(_twitterOAuthSettion.AuthorizeUri);
                VisualStateManager.GoToState(this, "Authorize", true);
                VisualStateManager.GoToState(this, "AuthorizeNormal", true);
                _authorizeStep = AuthorizeStep.TwitterAuthorize;
            }
            else if (_authorizeStep == AuthorizeStep.MastodonConfig)
            {
                var instance = AuthorizePopupMastodonInstanceTextBox.Text;
                var appName = AuthorizePopupMastodonAppNameTextBox.Text;
                var email = AuthorizePopupMastodonEmailTextBox.Text;
                var password = AuthorizePopupMastodonPasswordPasswordBox.Password;

                if (string.IsNullOrWhiteSpace(appName) || string.IsNullOrWhiteSpace(instance) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                    return;
                }

                AuthorizePopupConfigNextButton.IsEnabled = false;
                var url = string.Empty;
                try
                {
                    var appRegistration = await MastodonClient.CreateApp(instance, appName, Scope.Read | Scope.Write | Scope.Follow);
                    _mastodonOauthSettion = new MastodonOAuth { ClientId = appRegistration.ClientId, ClientSecret = appRegistration.ClientSecret, Instance = appRegistration.Instance };
                    var client = new MastodonClient(appRegistration);
                    await client.Connect(email, password);
                    client = new MastodonClient(appRegistration, client.AccessToken);
                    var user = await client.GetCurrentUser();
                    _accountInfo = new AccountInfo { ConsumerKey = _mastodonOauthSettion.ClientId, ConsumerSecret = _mastodonOauthSettion.ClientSecret, AccessToken = client.AccessToken, AccessTokenSecret = "", ScreenName = user.UserName, UserId = user.Id, Service = "Mastodon", Instance = _mastodonOauthSettion.Instance };
                    
                    _authorizeStep = AuthorizeStep.Exit;
                    return;
                }
                catch
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                    return;
                }
                finally
                {
                    AuthorizePopupConfigNextButton.IsEnabled = true;
                }
                AuthorizePopupAuthorizeWebView.Navigate(new Uri(url));
                VisualStateManager.GoToState(this, "Authorize", true);
                VisualStateManager.GoToState(this, "AuthorizeNormal", true);
                _authorizeStep = AuthorizeStep.MastodonAuthorize;
            }
        }

        private async void AuthorizePopupAuthorizeButton_Click(object sender, RoutedEventArgs e)
        {
            var pin = AuthorizePopupAuthorizePinTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(pin))
            {
                await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_PincodeIsEmpty")).ShowAsync();
                return;
            }

            AuthorizePopupAuthorizeButton.IsEnabled = false;

            if (_authorizeStep == AuthorizeStep.TwitterAuthorize)
            {
                Tokens tokens;
                try
                {
                    tokens = await _twitterOAuthSettion.GetTokensAsync(pin);
                    _accountInfo = new AccountInfo { ConsumerKey = tokens.ConsumerKey, ConsumerSecret = tokens.ConsumerSecret, AccessToken = tokens.AccessToken, AccessTokenSecret = tokens.AccessTokenSecret, ScreenName = tokens.ScreenName, UserId = tokens.UserId, Service = "Twitter", Instance = "" };
                }
                catch
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                    return;
                }
                finally
                {
                    AuthorizePopupAuthorizeButton.IsEnabled = true;
                }
            }
            else if (_authorizeStep == AuthorizeStep.MastodonAuthorize)
            {
                try
                {
                    var appRegistration = new AppRegistration() { ClientId = _mastodonOauthSettion.ClientId, ClientSecret = _mastodonOauthSettion.ClientSecret, Instance = _mastodonOauthSettion.Instance };
                    var client = new MastodonClient(appRegistration, pin);
                    var user = await client.GetCurrentUser();
                    _accountInfo = new AccountInfo { ConsumerKey = _mastodonOauthSettion.ClientId, ConsumerSecret = _mastodonOauthSettion.ClientSecret, AccessToken = pin, AccessTokenSecret = "", ScreenName = user.UserName, UserId = user.Id, Service = "Mastodon", Instance = _mastodonOauthSettion.Instance };
                }
                catch
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                    return;
                }
                finally
                {
                    AuthorizePopupAuthorizeButton.IsEnabled = true;
                }
            }

            _authorizeStep = AuthorizeStep.Exit;
        }

        private void AuthorizePopupAuthorizeWebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            var url = args.Uri.AbsoluteUri;
            if (_authorizeStep == AuthorizeStep.TwitterAuthorize)
            {
                if (!_urlCallbackAuthorization)
                {
                    VisualStateManager.GoToState(this, "AuthorizeWithPin", true);
                    return;
                }
                    

                if (url.StartsWith("http://cucmber.net/?oauth_token="))
                {
                    args.Cancel = true;
                    var match = Regex.Match(url, @"^http://cucmber.net/\?oauth_token=(?<OauthToken>.*)&oauth_verifier=(?<OauthVerifier>.*)$");
                    this.AuthorizeWithCallback(match.Groups["OauthVerifier"].Value);
                }
            }
            else if (_authorizeStep == AuthorizeStep.MastodonAuthorize)
            {
                if (url.StartsWith("https://" + _mastodonOauthSettion.Instance + "/oauth/authorize/"))
                {
                    args.Cancel = true;
                    var match = Regex.Match(url, @"^https://" + _mastodonOauthSettion.Instance + "/oauth/authorize/(?<OauthVerifier>.*)$");
                    this.AuthorizeWithCallback(match.Groups["OauthVerifier"].Value);
                }
            }

        }

        private async void AuthorizeWithCallback(string oauthVerifier)
        {
            if (_authorizeStep == AuthorizeStep.TwitterAuthorize)
            {
                Tokens tokens;
                try
                {
                    tokens = await _twitterOAuthSettion.GetTokensAsync(oauthVerifier);
                }
                catch
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                    return;
                }
                finally
                {
                    AuthorizePopupAuthorizeButton.IsEnabled = true;
                }

                _accountInfo = new AccountInfo { ConsumerKey = tokens.ConsumerKey, ConsumerSecret = tokens.ConsumerSecret, AccessToken = tokens.AccessToken, AccessTokenSecret = tokens.AccessTokenSecret, ScreenName = tokens.ScreenName, UserId = tokens.UserId, Service = "Twitter", Instance = "" };
            }
            else if (_authorizeStep == AuthorizeStep.MastodonAuthorize)
            {
                try
                {
                    var appRegistration = new AppRegistration() { ClientId = _mastodonOauthSettion.ClientId, ClientSecret = _mastodonOauthSettion.ClientSecret, Instance = _mastodonOauthSettion.Instance };
                    var client = new MastodonClient(appRegistration, oauthVerifier);
                    var user = await client.GetCurrentUser();
                    _accountInfo = new AccountInfo { ConsumerKey = _mastodonOauthSettion.ClientId, ConsumerSecret = _mastodonOauthSettion.ClientSecret, AccessToken = oauthVerifier, AccessTokenSecret = "", ScreenName = user.UserName, UserId = user.Id, Service = "Mastodon", Instance = _mastodonOauthSettion.Instance };
                }
                catch
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError")).ShowAsync();
                    return;
                }
                finally
                {
                    AuthorizePopupAuthorizeButton.IsEnabled = true;
                }
            }

            _authorizeStep = AuthorizeStep.Exit;
        }

        private AccountInfo _accountInfo;
        public async Task<AccountInfo> ShowAsync()
        {
            this.Height = Window.Current.Bounds.Height;
            this.Width = Window.Current.Bounds.Width;
            Window.Current.SizeChanged += SizeChanced_Event;

            _accountInfo = null;
            _contentPopup.IsOpen = true;
            _authorizeStep = AuthorizeStep.Choice;
            VisualStateManager.GoToState(this, "Choice", true);
            await Task.Run(() =>
            {
                while (_authorizeStep != AuthorizeStep.Exit)
                    Task.Delay(200).Wait();
            });
            _contentPopup.IsOpen = false;

            Window.Current.SizeChanged -= SizeChanced_Event;

            if (_accountInfo != null)
                await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_AuthorizeCompleted")).ShowAsync();

            return _accountInfo;
        }
    }
}
