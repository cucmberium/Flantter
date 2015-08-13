using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Setting;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Flantter.MilkyWay.ViewModels
{
    public class MainPageViewModel : ViewModel
    {
        private MainPageModel _MainPageModel { get; set; }

        public ReadOnlyReactiveCollection<AccountViewModel> Accounts { get; private set; }
        public ReactiveProperty<bool> TitleBarVisivility { get; private set; }

        #region Constructor
        public MainPageViewModel()
        {
            this._MainPageModel = MainPageModel.Instance;
            this.Accounts = this._MainPageModel.ReadOnlyAccounts.ToReadOnlyReactiveCollection(x => new AccountViewModel(x));

            // 設定によってTitlebarの表示を変える
            this.TitleBarVisivility = SettingService.Setting.ObserveProperty(x => x.TitleBarVisibility).ToReactiveProperty();

            SettingService.Setting.ObserveProperty(x => x.TweetBackgroundBrushAlpha).Subscribe(x =>
            {
                ((SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color.R, ((SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color.G, ((SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color.R, ((SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color.G, ((SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"]).Color.R, ((SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"]).Color.G, ((SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetMyStatusBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)Application.Current.Resources["TweetMyStatusBackgroundBrush"]).Color.R, ((SolidColorBrush)Application.Current.Resources["TweetMyStatusBackgroundBrush"]).Color.G, ((SolidColorBrush)Application.Current.Resources["TweetMyStatusBackgroundBrush"]).Color.B);
            });
        }
        #endregion

        #region Destructor
        ~MainPageViewModel()
        {
        }
        #endregion

        #region Others
        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

            this._MainPageModel.Initialize();
        }
        #endregion
    }
}
