using Flantter.MilkyWay.Setting;
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

namespace Flantter.MilkyWay.Themes
{
    public class ThemeService
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

        private static ThemeService _instance;
        public static ThemeService Theme { get { return _instance ?? (_instance = new ThemeService()); } }
        private ThemeService()
        {
            this.ThemeString = SettingService.Setting.Theme;
            ResourceDictionary.Source = new Uri("ms-appx:///Themes/Skins/" + this.ThemeString + ".xaml", UriKind.Absolute);
            DefaultResourceDictionary.Source = new Uri("ms-appx:///Themes/Skins/" + this.ThemeString + ".xaml", UriKind.Absolute);
        }

        protected T GetValue<T>([CallerMemberName] string name = null)
        {
            object value;
            bool result;

            result = ResourceDictionary.TryGetValue(name, out value);
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
                    this.ThemeString = supportedThemeNames.Contains(name) ? name : "Dark";
                    ResourceDictionary.Source = new Uri("ms-appx:///Themes/Skins/" + this.ThemeString + ".xaml", UriKind.Absolute);
                    DefaultResourceDictionary.Source = new Uri("ms-appx:///Themes/Skins/" + this.ThemeString + ".xaml", UriKind.Absolute);
                }
            }
            else
            {
                this.ThemeString = supportedThemeNames.Contains(name) ? name : "Dark";
                ResourceDictionary.Source = new Uri("ms-appx:///Themes/Skins/" + this.ThemeString + ".xaml", UriKind.Absolute);
                DefaultResourceDictionary.Source = new Uri("ms-appx:///Themes/Skins/" + this.ThemeString + ".xaml", UriKind.Absolute);
            }

            try
            {
				((SolidColorBrush)Application.Current.Resources["FlyoutBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["FlyoutBackgroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["FlyoutBorderThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["FlyoutBorderThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["MenuFlyoutSeparatorBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["MenuFlyoutSeparatorBackgroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["MenuFlyoutItemPointerOverBackgroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemDisabledBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["MenuFlyoutItemDisabledBackgroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemPressedBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["MenuFlyoutItemPressedBackgroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemFocusVisualWhiteStrokeThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["MenuFlyoutItemFocusVisualWhiteStrokeThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemFocusVisualBlackStrokeThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["MenuFlyoutItemFocusVisualBlackStrokeThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemTextblockForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["MenuFlyoutItemTextblockForegroundThemeBrush"]).Color;

				((SolidColorBrush)Application.Current.Resources["PageBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["PageBackgroundBrush"]).Color;

				((SolidColorBrush)Application.Current.Resources["BottomAppBarBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomAppBarBackgroundBrush"]).Color;

				((SolidColorBrush)Application.Current.Resources["BottomBarBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarBackgroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonSelectedBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarTextblockButtonSelectedBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonUnselectedBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarTextblockButtonUnselectedBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarButtonSelectedBackgroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarButtonSelectedForegroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarButtonUnselectedBackgroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarButtonUnselectedForegroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarAppBarButtonItemForegroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarAppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color;

				((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetMultipulActionAppBarButtonItemBackgroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetMultipulActionAppBarButtonItemDisabledForegroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetMultipulActionAppBarButtonItemForegroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetMultipulActionAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetMultipulActionAppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetMultipulActionAppBarButtonItemPressedForegroundThemeBrush"]).Color;

				((SolidColorBrush)Application.Current.Resources["PullToRefreshCharacterBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["PullToRefreshCharacterBrush"]).Color;

				((SolidColorBrush)Application.Current.Resources["ColumnViewBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewBackgroundBrush"]).Color;

				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarSelectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarSelectedForegroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnselectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarUnselectedForegroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarSymbolIconForegroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarDisabledSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarDisabledSymbolIconForegroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarTextblockForegroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnreadCountGridBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarUnreadCountGridBackgroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnreadCountTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarUnreadCountTextblockForegroundBrush"]).Color;
			}
            catch
            {
            }

            this.OnPropertyChanged(string.Empty);
        }

        private readonly List<string> supportedThemeNames = new List<string>()
		{
			"Dark",
			"Light",
            "Custom"
		};

        //public Thickness TweetFieldTweetListListViewItemListViewItemSelectedBorderThickness { get { return GetValue<Thickness>(); } }
        //public Double TweetFieldTweetListListViewItemListViewItemSelectedBorderThicknessDouble { get { return GetValue<Thickness>("TweetFieldTweetListListViewItemListViewItemSelectedBorderThickness").Bottom; } }
    }
}
