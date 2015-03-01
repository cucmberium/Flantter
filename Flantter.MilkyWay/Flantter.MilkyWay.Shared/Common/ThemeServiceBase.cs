using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Flantter.MilkyWay.Setting;

namespace Flantter.MilkyWay.Common
{
    public class ThemeServiceBase<Impl> : INotifyPropertyChanged
        where Impl : class, new()
    {
        private ResourceDictionary _ResourceDictionary = new ResourceDictionary();
        public ResourceDictionary ResourceDictionary 
        {
            get { return this._ResourceDictionary; } 
        }

        private ResourceDictionary _DefaultResourceDictionary = new ResourceDictionary();
        public ResourceDictionary DefaultResourceDictionary
        {
            get { return this._DefaultResourceDictionary; }
        }

        public string ThemeString { get; set; }

        protected T GetValue<T>([CallerMemberName] string name = null)
        {
            object value;
            bool result;

            result = ResourceDictionary.TryGetValue(name,out value);
            if (result)
                return (T)value;

            result = DefaultResourceDictionary.TryGetValue(name, out value);
            if (result)
                return (T)value;
                
            return value == null ? default(T) : (T)value;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var h = PropertyChanged;
            if (h != null) h(this, new PropertyChangedEventArgs(name));
        }

        private static Impl _instance;
        public static Impl Theme { get { return _instance ?? (_instance = new Impl()); } }
        protected ThemeServiceBase()
        {
            this.ThemeString = "Light";
            ResourceDictionary.Source = new Uri("ms-appx:///Themes/Colors/Light.xaml", UriKind.Absolute);
            DefaultResourceDictionary.Source = new Uri("ms-appx:///Themes/Colors/Light.xaml", UriKind.Absolute);
        }

        public void ChangeTheme(string name)
        {
            this.ThemeString = supportedThemeNames.Contains(name) ? name : "Light";
            ResourceDictionary.Source = new Uri("ms-appx:///Themes/Colors/" + this.ThemeString + ".xaml", UriKind.Absolute);

            this.OnPropertyChanged(string.Empty);
        }

        public async void ChangeTheme()
        {
            var name = SettingService.Setting.Theme;
            if (SettingService.Setting.UseCustomTheme && !string.IsNullOrWhiteSpace(SettingService.Setting.CustomThemePath))
            {
                this.ThemeString = "Custom";

                try
                {
                    var theme = await ApplicationData.Current.LocalFolder.GetFileAsync("Theme.xaml");
                    using (var s = await theme.OpenStreamForReadAsync())
                    {
                        var read = await FileIO.ReadTextAsync(theme);
                        var obj = XamlReader.Load(read);
                        _ResourceDictionary = obj as ResourceDictionary;
                    }
                }
                catch (Exception ex)
                {
                    this.ThemeString = supportedThemeNames.Contains(name.ToString()) ? name.ToString() : "Light";
                    ResourceDictionary.Source = new Uri("ms-appx:///Themes/Colors/" + this.ThemeString + ".xaml", UriKind.Absolute);
                }
            }
            else
            {
                this.ThemeString = supportedThemeNames.Contains(name.ToString()) ? name.ToString() : "Light";
                ResourceDictionary.Source = new Uri("ms-appx:///Themes/Colors/" + this.ThemeString + ".xaml", UriKind.Absolute);
            }

            try
            {
                // Default Resource
                ((SolidColorBrush)Application.Current.Resources["CustomAppBarItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomAppBarItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomAppBarItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomAppBarItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomAppBarItemForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomAppBarItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomAppBarItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomAppBarItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomAppBarItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomAppBarItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomAppBarItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomAppBarItemPressedForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["CustomButtonBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomButtonBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomButtonBorderThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomButtonBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomButtonDisabledBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomButtonDisabledBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomButtonDisabledBorderThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomButtonDisabledBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomButtonDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomButtonDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomButtonForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomButtonForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomButtonPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomButtonPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomButtonPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomButtonPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomButtonPressedBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomButtonPressedBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["CustomButtonPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["CustomButtonPressedForegroundThemeBrush"]).Color;

                // Custom Resource
                ((SolidColorBrush)Application.Current.Resources["GridBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["GridBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["BottomAppBarBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomAppBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomAppBarCharactorCountTextBoxForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomAppBarCharactorCountTextBoxForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TopAppBarBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TopAppBarBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["LeftSideMenuBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["LeftSideMenuBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["LeftSideMenuTextBlockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["LeftSideMenuTextBlockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["LeftSideMenuSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["LeftSideMenuSymbolIconForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["BottomBarGridBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarGridBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarAppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarAppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarLargeSelectedBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarLargeSelectedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarLargeUnselectedBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarLargeUnselectedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarShortSelectedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarShortSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarShortSelectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarShortSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarShortUnselectedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarShortUnselectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarShortUnselectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarShortUnselectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarMiddleSelectedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarMiddleSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarMiddleSelectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarMiddleSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarMiddleUnselectedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarMiddleUnselectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarMiddleUnselectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarMiddleUnselectedForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListPullToRefreshCharacterBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListPullToRefreshCharacterBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemCheckHintBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemCheckHintBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemCheckSelectingBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemCheckSelectingBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemCheckBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemCheckBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemDragBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemDragBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemDragForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemDragForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemFocusBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemFocusBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemPlaceholderBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemPlaceholderBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemSelectedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemSelectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemSelectedPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemSelectedPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListListViewItemSelectedPointerOverBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListListViewItemSelectedPointerOverBorderBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetFavoriteBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetFavoriteBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetRetweetBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetRetweetBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetMentionBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetMentionBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetMyStatusBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetMyStatusBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetUserNameTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetUserNameTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetUserScreenNameNameTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetUserScreenNameNameTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetTextTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetTextTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetHyperlinkTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetHyperlinkTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetDateTimeTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetDateTimeTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetSourceTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetSourceTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetOtherTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetOtherTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetRetweetTextTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetRetweetTextTextblockForegroundBrush"]).Color;
        
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetCommandReplySymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetCommandReplySymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetCommandRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetCommandRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetCommandDeleteRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetCommandDeleteRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetCommandFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetCommandFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetCommandDeleteFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetCommandDeleteFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetCommandUrlSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetCommandUrlSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListTweetCommandMenuSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListTweetCommandMenuSymbolIconForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListBarSelectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListBarSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListBarUnselectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListBarUnselectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListBarSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListBarSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListBarDisabledSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListBarDisabledSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListBarTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListBarTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListBarUnreadCountGridBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListBarUnreadCountGridBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTweetListBarUnreadCountTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTweetListBarUnreadCountTextblockForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetFieldTriangleTweetButtonForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTriangleTweetButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTriangleTweetButtonPointerOverBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTriangleTweetButtonPointerOverBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTriangleTweetButtonPointerPressedBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTriangleTweetButtonPointerPressedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTriangleTweetButtonSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTriangleTweetButtonSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTriangleTweetButtonSymbolIconPointerOverBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTriangleTweetButtonSymbolIconPointerOverBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFieldTriangleTweetButtonSymbolIconPointerPressedBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFieldTriangleTweetButtonSymbolIconPointerPressedBrush"]).Color;
            }
            catch
            { }
            

            this.OnPropertyChanged(string.Empty);
        }

        private readonly List<string> supportedThemeNames = new List<string>()
		{
			"Dark",
			"Light",
            "Custom"
		};

    }
}
