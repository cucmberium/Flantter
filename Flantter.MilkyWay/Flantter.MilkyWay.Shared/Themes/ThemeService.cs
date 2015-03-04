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
#if WINDOWS_PHONE_APP
                // WindowsPhone Resource
                ((SolidColorBrush)Application.Current.Resources["WindowsPhone_PageBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["WindowsPhone_PageBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["WindowsPhone_BottomBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarTextblockButtonSelectedBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["WindowsPhone_BottomBarTextblockButtonSelectedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarTextblockButtonUnselectedBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["WindowsPhone_BottomBarTextblockButtonUnselectedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonSelectedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["WindowsPhone_BottomBarButtonSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonSelectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["WindowsPhone_BottomBarButtonSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonUnselectedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["WindowsPhone_BottomBarButtonUnselectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarButtonUnselectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["WindowsPhone_BottomBarButtonUnselectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["WindowsPhone_BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["WindowsPhone_BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarAppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["WindowsPhone_BottomBarAppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["WindowsPhone_BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["WindowsPhone_BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color;
#elif WINDOWS_APP
                // Windows Resource
                ((SolidColorBrush)Application.Current.Resources["Windows_PageBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_PageBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarTextblockButtonSelectedBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarTextblockButtonSelectedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarTextblockButtonUnselectedBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarTextblockButtonUnselectedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarButtonSelectedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarButtonSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarButtonSelectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarButtonSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarButtonUnselectedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarButtonUnselectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarButtonUnselectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarButtonUnselectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarAppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarAppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarAppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarAppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["Windows_BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["Windows_BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color;
#endif
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
