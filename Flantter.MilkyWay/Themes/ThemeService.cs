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
using Windows.UI;

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
            DefaultResourceDictionary.Source = new Uri("ms-appx:///Themes/Skins/" + "Default" + ".xaml", UriKind.Absolute);

            ChangeTheme();
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
				((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemDisabledTextblockForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["MenuFlyoutItemDisabledTextblockForegroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemPressedBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["MenuFlyoutItemPressedBackgroundThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemFocusVisualWhiteStrokeThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["MenuFlyoutItemFocusVisualWhiteStrokeThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemFocusVisualBlackStrokeThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["MenuFlyoutItemFocusVisualBlackStrokeThemeBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemTextblockForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["MenuFlyoutItemTextblockForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ScrollBarRepeatButtonPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarRepeatButtonPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowPointerOverForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarArrowPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarRepeatButtonPressedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarRepeatButtonPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowPressedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarArrowPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowDisabledForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarArrowDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarArrowForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarThumbFillBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarThumbFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarThumbPointerOverFillBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarThumbPointerOverFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarThumbPressedFillBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarThumbPressedFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarTrackRectDisabledStrokeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarTrackRectDisabledStrokeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarPanningThumbDisabledStrokeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarPanningThumbDisabledStrokeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarTrackRectFillBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarTrackRectFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarTrackRectStrokeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarTrackRectStrokeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarPanningThumbBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ScrollBarPanningThumbBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TextBoxForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxSelectionHighlightBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxSelectionHighlightBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxButtonBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxButtonBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxButtonBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxGlyphElementPointerOverForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxGlyphElementPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxButtonLayoutGridPressedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxButtonLayoutGridPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxGlyphElementPressedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxGlyphElementPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxGlyphElementForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxGlyphElementForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxHeaderContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxHeaderContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBackgroundElementDisabledBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxBackgroundElementDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementDisabledBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxBorderElementDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementDisabledBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxBorderElementDisabledBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxContentElementDisabledForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxContentElementDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxPlaceholderTextContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxPlaceholderTextContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementPointerOverBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxBorderElementPointerOverBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxPlaceholderTextContentPresenterFocusedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxPlaceholderTextContentPresenterFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBackgroundElementFocusedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxBackgroundElementFocusedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementFocusedBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxBorderElementFocusedBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxContentElementFocusedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxContentElementFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxHeaderContentPresenterForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TextBoxHeaderContentPresenterForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ButtonBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ButtonBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPointerOverBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ButtonContentPresenterPointerOverBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPointerOverForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ButtonContentPresenterPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonRootGridPressedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ButtonRootGridPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPressedBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ButtonContentPresenterPressedBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPressedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ButtonContentPresenterPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonRootGridDisabledBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ButtonRootGridDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ButtonContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterDisabledBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ButtonContentPresenterDisabledBorderBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ProgressBarForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ProgressBarForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ProgressRingForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ProgressRingForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarButtonItemPressedForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["AppBarBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarBorderBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["PageBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["PageBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["AppBarTweetButtonBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarTweetButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarTweetButtonForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarTweetButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarCharacterCountForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarCharacterCountForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarReplyOrQuotedStatusAreaBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaTextBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarReplyOrQuotedStatusAreaTextBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaNameBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarReplyOrQuotedStatusAreaNameBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaScreenNameBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarReplyOrQuotedStatusAreaScreenNameBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaQuotedRetweetSymbolBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarReplyOrQuotedStatusAreaQuotedRetweetSymbolBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaReplySymbolBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarReplyOrQuotedStatusAreaReplySymbolBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaNoticeBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["AppBarReplyOrQuotedStatusAreaNoticeBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TitleBarBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TitleBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TitleBarForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TitleBarButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TitleBarButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TitleBarButtonInactiveBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TitleBarButtonInactiveForegroundBrush"]).Color;
                
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

				((SolidColorBrush)Application.Current.Resources["ColumnViewBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.ColumnBackgroundBrushAlpha), ((SolidColorBrush)_ResourceDictionary["ColumnViewBackgroundBrush"]).Color.R, ((SolidColorBrush)_ResourceDictionary["ColumnViewBackgroundBrush"]).Color.G, ((SolidColorBrush)_ResourceDictionary["ColumnViewBackgroundBrush"]).Color.B);

                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarSelectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarSelectedForegroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnselectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarUnselectedForegroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarSymbolIconForegroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarDisabledSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarDisabledSymbolIconForegroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarTextblockForegroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnreadCountGridBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarUnreadCountGridBackgroundBrush"]).Color;
				((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnreadCountTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["ColumnViewControlBarUnreadCountTextblockForegroundBrush"]).Color;
                
                ((SolidColorBrush)Application.Current.Resources["TweetCheckBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetCheckBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCheckBoxBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetCheckBoxBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetDragBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetDragBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetDragForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetDragForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFocusBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFocusBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFocusSecondaryBorderBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetFocusSecondaryBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPlaceholderBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetPlaceholderBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPointerOverForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetSelectedPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPressedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedPressedBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetSelectedPressedBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)_ResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.R, ((SolidColorBrush)_ResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.G, ((SolidColorBrush)_ResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)_ResourceDictionary["TweetRetweetBackgroundBrush"]).Color.R, ((SolidColorBrush)_ResourceDictionary["TweetRetweetBackgroundBrush"]).Color.G, ((SolidColorBrush)_ResourceDictionary["TweetRetweetBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)_ResourceDictionary["TweetMentionBackgroundBrush"]).Color.R, ((SolidColorBrush)_ResourceDictionary["TweetMentionBackgroundBrush"]).Color.G, ((SolidColorBrush)_ResourceDictionary["TweetMentionBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetMyTweetBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)_ResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.R, ((SolidColorBrush)_ResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.G, ((SolidColorBrush)_ResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.B);

                ((SolidColorBrush)Application.Current.Resources["TweetRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetNameTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetNameTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetScreenNameTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetScreenNameTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetTextTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetTextTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetTextHyperlinkTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetTextHyperlinkTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetDateTimeTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetDateTimeTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSourceTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetSourceTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetOtherTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetOtherTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetRetweetTextblockForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetRetweetTextblockForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetQuotedStatusBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetQuotedStatusBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetTargetStatusBackgroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetTargetStatusBackgroundBrush"]).Color;
                
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarReplySymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetCommandBarReplySymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetCommandBarRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarDestroyRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetCommandBarDestroyRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetCommandBarFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarUrlSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetCommandBarUrlSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarMenuSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_ResourceDictionary["TweetGetGapTextblockForegroundBrush"]).Color;
            }
            catch
            {

                ((SolidColorBrush)Application.Current.Resources["FlyoutBackgroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["FlyoutBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["FlyoutBorderThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["FlyoutBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutSeparatorBackgroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["MenuFlyoutSeparatorBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["MenuFlyoutItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemDisabledTextblockForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["MenuFlyoutItemDisabledTextblockForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemPressedBackgroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["MenuFlyoutItemPressedBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemFocusVisualWhiteStrokeThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["MenuFlyoutItemFocusVisualWhiteStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemFocusVisualBlackStrokeThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["MenuFlyoutItemFocusVisualBlackStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemTextblockForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["MenuFlyoutItemTextblockForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ScrollBarRepeatButtonPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarRepeatButtonPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowPointerOverForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarArrowPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarRepeatButtonPressedBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarRepeatButtonPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowPressedForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarArrowPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowDisabledForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarArrowDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarArrowForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarThumbFillBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarThumbFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarThumbPointerOverFillBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarThumbPointerOverFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarThumbPressedFillBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarThumbPressedFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarTrackRectDisabledStrokeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarTrackRectDisabledStrokeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarPanningThumbDisabledStrokeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarPanningThumbDisabledStrokeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarTrackRectFillBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarTrackRectFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarTrackRectStrokeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarTrackRectStrokeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarPanningThumbBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ScrollBarPanningThumbBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TextBoxForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxSelectionHighlightBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxSelectionHighlightBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxButtonBorderBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxButtonBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxButtonBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxGlyphElementPointerOverForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxGlyphElementPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxButtonLayoutGridPressedForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxButtonLayoutGridPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxGlyphElementPressedForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxGlyphElementPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxGlyphElementForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxGlyphElementForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxHeaderContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxHeaderContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBackgroundElementDisabledBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxBackgroundElementDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementDisabledBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxBorderElementDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementDisabledBorderBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxBorderElementDisabledBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxContentElementDisabledForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxContentElementDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxPlaceholderTextContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxPlaceholderTextContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementPointerOverBorderBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxBorderElementPointerOverBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxPlaceholderTextContentPresenterFocusedForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxPlaceholderTextContentPresenterFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBackgroundElementFocusedBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxBackgroundElementFocusedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementFocusedBorderBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxBorderElementFocusedBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxContentElementFocusedForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxContentElementFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxHeaderContentPresenterForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TextBoxHeaderContentPresenterForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ButtonBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonBorderBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ButtonBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPointerOverBorderBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ButtonContentPresenterPointerOverBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPointerOverForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ButtonContentPresenterPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonRootGridPressedBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ButtonRootGridPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPressedBorderBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ButtonContentPresenterPressedBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPressedForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ButtonContentPresenterPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonRootGridDisabledBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ButtonRootGridDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ButtonContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterDisabledBorderBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ButtonContentPresenterDisabledBorderBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ProgressBarForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ProgressBarForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ProgressRingForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ProgressRingForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarButtonItemPressedForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["AppBarBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarBorderBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarBorderBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["PageBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["PageBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["AppBarTweetButtonBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarTweetButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarTweetButtonForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarTweetButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarCharacterCountForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarCharacterCountForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaTextBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaTextBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaNameBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaNameBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaScreenNameBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaScreenNameBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaQuotedRetweetSymbolBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaQuotedRetweetSymbolBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaReplySymbolBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaReplySymbolBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaNoticeBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaNoticeBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TitleBarBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TitleBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TitleBarForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TitleBarButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TitleBarButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TitleBarButtonInactiveBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TitleBarButtonInactiveForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["BottomBarBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonSelectedBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarTextblockButtonSelectedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonUnselectedBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarTextblockButtonUnselectedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarButtonSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarButtonSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarButtonUnselectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarButtonUnselectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarAppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarAppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetMultipulActionAppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetMultipulActionAppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetMultipulActionAppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetMultipulActionAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetMultipulActionAppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetMultipulActionAppBarButtonItemPressedForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["PullToRefreshCharacterBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["PullToRefreshCharacterBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ColumnViewBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.ColumnBackgroundBrushAlpha), ((SolidColorBrush)_DefaultResourceDictionary["ColumnViewBackgroundBrush"]).Color.R, ((SolidColorBrush)_DefaultResourceDictionary["ColumnViewBackgroundBrush"]).Color.G, ((SolidColorBrush)_DefaultResourceDictionary["ColumnViewBackgroundBrush"]).Color.B);

                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarSelectedForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ColumnViewControlBarSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnselectedForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ColumnViewControlBarUnselectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ColumnViewControlBarSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarDisabledSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ColumnViewControlBarDisabledSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarTextblockForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ColumnViewControlBarTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnreadCountGridBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ColumnViewControlBarUnreadCountGridBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnreadCountTextblockForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["ColumnViewControlBarUnreadCountTextblockForegroundBrush"]).Color;
                
                ((SolidColorBrush)Application.Current.Resources["TweetCheckBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetCheckBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCheckBoxBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetCheckBoxBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetDragBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetDragBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetDragForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetDragForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFocusBorderBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetFocusBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFocusSecondaryBorderBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetFocusSecondaryBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPlaceholderBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetPlaceholderBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPointerOverForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetSelectedPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPressedBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedPressedBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetSelectedPressedBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)_DefaultResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.R, ((SolidColorBrush)_DefaultResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.G, ((SolidColorBrush)_DefaultResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)_DefaultResourceDictionary["TweetRetweetBackgroundBrush"]).Color.R, ((SolidColorBrush)_DefaultResourceDictionary["TweetRetweetBackgroundBrush"]).Color.G, ((SolidColorBrush)_DefaultResourceDictionary["TweetRetweetBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)_DefaultResourceDictionary["TweetMentionBackgroundBrush"]).Color.R, ((SolidColorBrush)_DefaultResourceDictionary["TweetMentionBackgroundBrush"]).Color.G, ((SolidColorBrush)_DefaultResourceDictionary["TweetMentionBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetMyTweetBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)_DefaultResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.R, ((SolidColorBrush)_DefaultResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.G, ((SolidColorBrush)_DefaultResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.B);

                ((SolidColorBrush)Application.Current.Resources["TweetRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetNameTextblockForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetNameTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetScreenNameTextblockForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetScreenNameTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetTextTextblockForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetTextTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetTextHyperlinkTextblockForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetTextHyperlinkTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetDateTimeTextblockForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetDateTimeTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSourceTextblockForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetSourceTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetOtherTextblockForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetOtherTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetRetweetTextblockForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetRetweetTextblockForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetQuotedStatusBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetQuotedStatusBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetTargetStatusBackgroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetTargetStatusBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarReplySymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetCommandBarReplySymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetCommandBarRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarDestroyRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetCommandBarDestroyRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetCommandBarFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarUrlSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetCommandBarUrlSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarMenuSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)_DefaultResourceDictionary["TweetGetGapTextblockForegroundBrush"]).Color;

            }

            this.OnPropertyChanged(string.Empty);
        }

        private readonly List<string> supportedThemeNames = new List<string>()
		{
			"Dark",
			"Light",
            "Custom"
		};
        
        public void ChangeBackgroundAlpha()
        {
            ((SolidColorBrush)Application.Current.Resources["ColumnViewBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.ColumnBackgroundBrushAlpha), ((SolidColorBrush)Application.Current.Resources["ColumnViewBackgroundBrush"]).Color.R, ((SolidColorBrush)Application.Current.Resources["ColumnViewBackgroundBrush"]).Color.G, ((SolidColorBrush)Application.Current.Resources["ColumnViewBackgroundBrush"]).Color.B);

            ((SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color.R, ((SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color.G, ((SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color.B);
            ((SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color.R, ((SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color.G, ((SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color.B);
            ((SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"]).Color.R, ((SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"]).Color.G, ((SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"]).Color.B);
            ((SolidColorBrush)Application.Current.Resources["TweetMyTweetBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)Application.Current.Resources["TweetMyTweetBackgroundBrush"]).Color.R, ((SolidColorBrush)Application.Current.Resources["TweetMyTweetBackgroundBrush"]).Color.G, ((SolidColorBrush)Application.Current.Resources["TweetMyTweetBackgroundBrush"]).Color.B);
        }
    }
}
