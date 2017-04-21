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
using CoreTweet;
using Mastonet;
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
        Exit
    }

    public sealed partial class AuthorizePopup : UserControl
    {
        private const string TwitterConsumerKey = "qSlXQa4PGPArQVjwajG0tg";
        private const string TwitterConsumerSecret = "h3s83WfKRE2UzwFwFMDMxZN8r4pkDzgsDG7kKr0ZhgI";

        private readonly Popup _contentPopup;
        private readonly ResourceLoader _resourceLoader;

        private AccountInfo _accountInfo;

        private AuthorizeStep _authorizeStep = AuthorizeStep.Exit;

        private AppRegistration _mastodonAppRegistration;
        private AuthenticationClient _mastodonOauthSettion;
        private OAuth.OAuthSession _twitterOAuthSettion;
        private bool _urlCallbackAuthorization;

        public AuthorizePopup()
        {
            InitializeComponent();

            _resourceLoader = new ResourceLoader();
            _contentPopup = new Popup
            {
                Child = this,
                IsLightDismissEnabled = false,
                HorizontalOffset = 0.0,
                VerticalOffset = 0.0
            };
        }

        private void SizeChanced_Event(object sender, WindowSizeChangedEventArgs e)
        {
            Height = Window.Current.Bounds.Height;
            Width = Window.Current.Bounds.Width;
        }

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

        private async void AuthorizePopupConfigNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (_authorizeStep == AuthorizeStep.TwitterConfig)
            {
                var editConsumerKey = AuthorizePopupTwitterEditConsumerKeyCheckBox.IsChecked;

                string consumerKey;
                string consumerSecret;
                string callbackUrl;
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
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError"))
                        .ShowAsync();
                    return;
                }

                AuthorizePopupConfigNextButton.IsEnabled = false;

                try
                {
                    _twitterOAuthSettion = await OAuth.AuthorizeAsync(consumerKey, consumerSecret, callbackUrl);
                }
                catch
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError"))
                        .ShowAsync();
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

                if (string.IsNullOrWhiteSpace(appName) || string.IsNullOrWhiteSpace(instance))
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError"))
                        .ShowAsync();
                    return;
                }

                var url = string.Empty;
                AuthorizePopupConfigNextButton.IsEnabled = false;
                try
                {
                    _mastodonOauthSettion = new AuthenticationClient(instance);
                    _mastodonAppRegistration =
                        await _mastodonOauthSettion.CreateApp(appName, Scope.Read | Scope.Write | Scope.Follow);
                    url = _mastodonOauthSettion.OAuthUrl();
                }
                catch
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError"))
                        .ShowAsync();
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
                    _accountInfo = new AccountInfo
                    {
                        ConsumerKey = tokens.ConsumerKey,
                        ConsumerSecret = tokens.ConsumerSecret,
                        AccessToken = tokens.AccessToken,
                        AccessTokenSecret = tokens.AccessTokenSecret,
                        ScreenName = tokens.ScreenName,
                        UserId = tokens.UserId,
                        Service = "Twitter",
                        Instance = ""
                    };
                }
                catch
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError"))
                        .ShowAsync();
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
                    var auth = await _mastodonOauthSettion.ConnectWithCode(pin);
                    var client = new MastodonClient(_mastodonAppRegistration, auth);
                    var user = await client.GetCurrentUser();
                    _accountInfo = new AccountInfo
                    {
                        ConsumerKey = _mastodonAppRegistration.ClientId,
                        ConsumerSecret = _mastodonAppRegistration.ClientSecret,
                        AccessToken = auth.AccessToken,
                        AccessTokenSecret = "",
                        ScreenName = user.UserName,
                        UserId = user.Id,
                        Service = "Mastodon",
                        Instance = _mastodonOauthSettion.Instance
                    };
                }
                catch
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError"))
                        .ShowAsync();
                    return;
                }
                finally
                {
                    AuthorizePopupAuthorizeButton.IsEnabled = true;
                }
            }

            _authorizeStep = AuthorizeStep.Exit;
        }

        private void AuthorizePopupAuthorizeWebView_NavigationStarting(WebView sender,
            WebViewNavigationStartingEventArgs args)
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
                    var match = Regex.Match(url,
                        @"^http://cucmber.net/\?oauth_token=(?<OauthToken>.*)&oauth_verifier=(?<OauthVerifier>.*)$");
                    AuthorizeWithCallback(match.Groups["OauthVerifier"].Value);
                }
            }
            else if (_authorizeStep == AuthorizeStep.MastodonAuthorize)
            {
                if (url.StartsWith("https://" + _mastodonOauthSettion.Instance + "/oauth/authorize/"))
                {
                    args.Cancel = true;
                    var match = Regex.Match(url,
                        @"^https://" + _mastodonOauthSettion.Instance + "/oauth/authorize/(?<OauthVerifier>.*)$");
                    AuthorizeWithCallback(match.Groups["OauthVerifier"].Value);
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
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError"))
                        .ShowAsync();
                    return;
                }
                finally
                {
                    AuthorizePopupAuthorizeButton.IsEnabled = true;
                }

                _accountInfo = new AccountInfo
                {
                    ConsumerKey = tokens.ConsumerKey,
                    ConsumerSecret = tokens.ConsumerSecret,
                    AccessToken = tokens.AccessToken,
                    AccessTokenSecret = tokens.AccessTokenSecret,
                    ScreenName = tokens.ScreenName,
                    UserId = tokens.UserId,
                    Service = "Twitter",
                    Instance = ""
                };
            }
            else if (_authorizeStep == AuthorizeStep.MastodonAuthorize)
            {
                try
                {
                    var auth = await _mastodonOauthSettion.ConnectWithCode(oauthVerifier);
                    var client = new MastodonClient(_mastodonAppRegistration, auth);
                    var user = await client.GetCurrentUser();
                    _accountInfo = new AccountInfo
                    {
                        ConsumerKey = _mastodonAppRegistration.ClientId,
                        ConsumerSecret = _mastodonAppRegistration.ClientSecret,
                        AccessToken = auth.AccessToken,
                        AccessTokenSecret = "",
                        ScreenName = user.UserName,
                        UserId = user.Id,
                        Service = "Mastodon",
                        Instance = _mastodonOauthSettion.Instance
                    };
                }
                catch
                {
                    await new MessageDialog(_resourceLoader.GetString("AuthorizePopup_OAuthAuthorizeError"))
                        .ShowAsync();
                    return;
                }
                finally
                {
                    AuthorizePopupAuthorizeButton.IsEnabled = true;
                }
            }

            _authorizeStep = AuthorizeStep.Exit;
        }

        public async Task<AccountInfo> ShowAsync()
        {
            Height = Window.Current.Bounds.Height;
            Width = Window.Current.Bounds.Width;
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