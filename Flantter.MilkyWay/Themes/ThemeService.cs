using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Flantter.MilkyWay.Setting;

namespace Flantter.MilkyWay.Themes
{
    public class ThemeService
    {
        private static ThemeService _instance;

        private string _themeString;

        private ThemeService()
        {
            ThemeString = SettingService.Setting.Theme.ToString();
        }

        public string ThemeString
        {
            get { return _themeString; }
            set
            {
                if (_themeString != value)
                {
                    _themeString = value;
                    OnPropertyChanged();
                }
            }
        }

        public static ThemeService Theme => _instance ?? (_instance = new ThemeService());

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public async void ChangeTheme()
        {
            ResourceDictionary customThemeResourceDictionary = null;
            if (SettingService.Setting.UseCustomTheme &&
                !string.IsNullOrWhiteSpace(SettingService.Setting.CustomThemePath))
            {
                try
                {
                    var theme = await ApplicationData.Current.LocalFolder.GetFileAsync("Theme.xaml");
                    var read = await FileIO.ReadTextAsync(theme);
                    var obj = XamlReader.Load(read);
                    customThemeResourceDictionary = obj as ResourceDictionary;
                }
                catch
                {
                }
            }
            
            var targetResourceDictionary = Application.Current.Resources;
            if (targetResourceDictionary == null)
                return;

            if (customThemeResourceDictionary != null)
                foreach (var pair in customThemeResourceDictionary)
                {
                    var key = pair.Key as string;
                    var brush = pair.Value as SolidColorBrush;
                    if (string.IsNullOrWhiteSpace(key) || brush == null)
                        continue;

                    try
                    {
                        ((SolidColorBrush) targetResourceDictionary[key]).Color = brush.Color;
                    }
                    catch
                    {
                        Debug.WriteLine(key);
                    }
                }
            ChangeBackgroundAlpha();

            ThemeString = customThemeResourceDictionary == null ? SettingService.Setting.Theme.ToString() : "Custom";
        }

        public void ChangeBackgroundAlpha()
        {
            var targetResourceDictionary = Application.Current.Resources;
            if (targetResourceDictionary == null)
                return;

            ((SolidColorBrush) targetResourceDictionary["ColumnViewBackgroundBrush"]).Color = Color.FromArgb(
                Convert.ToByte(SettingService.Setting.ColumnBackgroundBrushAlpha),
                ((SolidColorBrush) targetResourceDictionary["ColumnViewBackgroundBrush"]).Color.R,
                ((SolidColorBrush) targetResourceDictionary["ColumnViewBackgroundBrush"]).Color.G,
                ((SolidColorBrush) targetResourceDictionary["ColumnViewBackgroundBrush"]).Color.B);

            ((SolidColorBrush) targetResourceDictionary["TweetFavoriteBackgroundBrush"]).Color = Color.FromArgb(
                Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha),
                ((SolidColorBrush) targetResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.R,
                ((SolidColorBrush) targetResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.G,
                ((SolidColorBrush) targetResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.B);
            ((SolidColorBrush) targetResourceDictionary["TweetRetweetBackgroundBrush"]).Color = Color.FromArgb(
                Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha),
                ((SolidColorBrush) targetResourceDictionary["TweetRetweetBackgroundBrush"]).Color.R,
                ((SolidColorBrush) targetResourceDictionary["TweetRetweetBackgroundBrush"]).Color.G,
                ((SolidColorBrush) targetResourceDictionary["TweetRetweetBackgroundBrush"]).Color.B);
            ((SolidColorBrush) targetResourceDictionary["TweetMentionBackgroundBrush"]).Color = Color.FromArgb(
                Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha),
                ((SolidColorBrush) targetResourceDictionary["TweetMentionBackgroundBrush"]).Color.R,
                ((SolidColorBrush) targetResourceDictionary["TweetMentionBackgroundBrush"]).Color.G,
                ((SolidColorBrush) targetResourceDictionary["TweetMentionBackgroundBrush"]).Color.B);
            ((SolidColorBrush) targetResourceDictionary["TweetMyTweetBackgroundBrush"]).Color = Color.FromArgb(
                Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha),
                ((SolidColorBrush) targetResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.R,
                ((SolidColorBrush) targetResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.G,
                ((SolidColorBrush) targetResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.B);
        }
    }
}