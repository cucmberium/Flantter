using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        private readonly List<string> _supportedThemeNames = new List<string>
        {
            "Dark",
            "Light",
            "Custom"
        };

        private ThemeService()
        {
            ThemeString = SettingService.Setting.Theme.ToString();
            DefaultResourceDictionary.Source = new Uri("ms-appx:///Themes/Skins/Default.xaml", UriKind.Absolute);
        }

        public ResourceDictionary ResourceDictionary { get; private set; } = new ResourceDictionary();

        public ResourceDictionary DefaultResourceDictionary { get; } = new ResourceDictionary();

        public string ThemeString { get; set; }
        public static ThemeService Theme => _instance ?? (_instance = new ThemeService());

        protected T GetValue<T>([CallerMemberName] string name = null)
        {
            object value;
            bool result;

            result = ResourceDictionary.TryGetValue(name, out value);
            if (result)
                return (T) value;

            result = DefaultResourceDictionary.TryGetValue(name, out value);
            if (result)
                return (T) value;

            return value == null ? default(T) : (T) value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public async void ChangeTheme()
        {
            var name = SettingService.Setting.Theme;
            if (SettingService.Setting.UseCustomTheme &&
                !string.IsNullOrWhiteSpace(SettingService.Setting.CustomThemePath))
            {
                ThemeString = "Custom";

                try
                {
                    var theme = await ApplicationData.Current.LocalFolder.GetFileAsync("Theme.xaml");
                    using (var s = await theme.OpenStreamForReadAsync())
                    {
                        var read = await FileIO.ReadTextAsync(theme);
                        var obj = XamlReader.Load(read);
                        ResourceDictionary = obj as ResourceDictionary;
                    }
                }
                catch (Exception ex)
                {
                    ThemeString = _supportedThemeNames.Contains(name.ToString()) ? name.ToString() : "Dark";
                    ResourceDictionary.Source = new Uri("ms-appx:///Themes/Skins/" + ThemeString + ".xaml",
                        UriKind.Absolute);
                }
            }
            else
            {
                ThemeString = _supportedThemeNames.Contains(name.ToString()) ? name.ToString() : "Dark";
                ResourceDictionary.Source = new Uri("ms-appx:///Themes/Skins/" + ThemeString + ".xaml",
                    UriKind.Absolute);
            }

            try
            {
                ((SolidColorBrush)Application.Current.Resources["SearchBoxBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxBorderThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxDisabledBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxDisabledBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxDisabledTextThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxDisabledTextThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxDisabledBorderThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxDisabledBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxPointerOverTextThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxPointerOverTextThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxPointerOverBorderThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxPointerOverBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxFocusedBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxFocusedBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxFocusedTextThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxFocusedTextThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxFocusedBorderThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxFocusedBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxButtonForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxButtonForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxButtonPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxButtonPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxSeparatorSuggestionForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxSeparatorSuggestionForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxIMECandidateListSeparatorThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxIMECandidateListSeparatorThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SearchBoxForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchPointerOverOuterBorderStrokeThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchPointerOverOuterBorderStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchPointerOverSwitchKnobOffFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchPointerOverSwitchKnobOffFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchPressedOuterBorderFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchPressedOuterBorderFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchPressedSwitchKnobBoundsStrokeThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchPressedSwitchKnobBoundsStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchPressedSwitchKnobBoundsFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchPressedSwitchKnobBoundsFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchPressedSwitchKnobOffFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchPressedSwitchKnobOffFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledHeaderContentPresenterForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchDisabledHeaderContentPresenterForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledOffContentPresenterForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchDisabledOffContentPresenterForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledOnContentPresenterForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchDisabledOnContentPresenterForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledOuterBorderStrokeThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchDisabledOuterBorderStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledSwitchKnobBoundsFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchDisabledSwitchKnobBoundsFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledSwitchKnobBoundsStrokeThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchDisabledSwitchKnobBoundsStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledSwitchKnobOffFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchDisabledSwitchKnobOffFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledSwitchKnobOnFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchDisabledSwitchKnobOnFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchHeaderContentPresenterForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchHeaderContentPresenterForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchOuterBorderStrokeThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchOuterBorderStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchSwitchKnobBoundsStrokeThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchSwitchKnobBoundsStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchSwitchKnobOnFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchSwitchKnobOnFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchSwitchKnobOffFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ToggleSwitchSwitchKnobOffFillThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["SliderBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderPressedHorizontalThumbBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderPressedHorizontalThumbBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledHeaderContentPresenterForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderDisabledHeaderContentPresenterForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledHorizontalDecreaseRectFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderDisabledHorizontalDecreaseRectFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledHorizontalTrackRectFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderDisabledHorizontalTrackRectFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledVerticalDecreaseRectFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderDisabledVerticalDecreaseRectFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledVerticalTrackRectFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderDisabledVerticalTrackRectFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledHorizontalThumbBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderDisabledHorizontalThumbBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledVerticalThumbBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderDisabledVerticalThumbBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledTopTickBarFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderDisabledTopTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledBottomTickBarFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderDisabledBottomTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledLeftTickBarFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderDisabledLeftTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledRightTickBarFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderDisabledRightTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderPointerOverHorizontalTrackRectFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderPointerOverHorizontalTrackRectFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderPointerOverVerticalTrackRectFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderPointerOverVerticalTrackRectFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderPointerOverHorizontalThumbBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderPointerOverHorizontalThumbBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderPointerOverVerticalThumbBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderPointerOverVerticalThumbBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderHeaderContentPresenterForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderHeaderContentPresenterForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderTopTickBarFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderTopTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderHorizontalInlineTickBarFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderHorizontalInlineTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderBottomTickBarFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderBottomTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderLeftTickBarFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderLeftTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderVerticalInlineTickBarFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderVerticalInlineTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderRightTickBarFillThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SliderRightTickBarFillThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["FlyoutBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["FlyoutBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["FlyoutBorderThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["FlyoutBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutSeparatorBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["MenuFlyoutSeparatorBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["MenuFlyoutItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemDisabledTextblockForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["MenuFlyoutItemDisabledTextblockForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemPressedBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["MenuFlyoutItemPressedBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemFocusVisualWhiteStrokeThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["MenuFlyoutItemFocusVisualWhiteStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemFocusVisualBlackStrokeThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["MenuFlyoutItemFocusVisualBlackStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemTextblockForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["MenuFlyoutItemTextblockForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ScrollBarRepeatButtonPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarRepeatButtonPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowPointerOverForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarArrowPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarRepeatButtonPressedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarRepeatButtonPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarArrowPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowDisabledForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarArrowDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarArrowForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarThumbFillBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarThumbFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarThumbPointerOverFillBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarThumbPointerOverFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarThumbPressedFillBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarThumbPressedFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarTrackRectDisabledStrokeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarTrackRectDisabledStrokeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarPanningThumbDisabledStrokeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarPanningThumbDisabledStrokeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarTrackRectFillBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarTrackRectFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarTrackRectStrokeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarTrackRectStrokeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarPanningThumbBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ScrollBarPanningThumbBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TextBoxForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxSelectionHighlightBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxSelectionHighlightBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxButtonBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxButtonBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxButtonBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxGlyphElementPointerOverForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxGlyphElementPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxButtonLayoutGridPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxButtonLayoutGridPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxGlyphElementPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxGlyphElementPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxGlyphElementForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxGlyphElementForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxHeaderContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxHeaderContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBackgroundElementDisabledBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxBackgroundElementDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementDisabledBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxBorderElementDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementDisabledBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxBorderElementDisabledBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxContentElementDisabledForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxContentElementDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxPlaceholderTextContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxPlaceholderTextContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementPointerOverBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxBorderElementPointerOverBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxPlaceholderTextContentPresenterFocusedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxPlaceholderTextContentPresenterFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBackgroundElementFocusedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxBackgroundElementFocusedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementFocusedBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxBorderElementFocusedBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxContentElementFocusedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxContentElementFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxHeaderContentPresenterForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TextBoxHeaderContentPresenterForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ButtonBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ButtonBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPointerOverBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ButtonContentPresenterPointerOverBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPointerOverForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ButtonContentPresenterPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonRootGridPressedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ButtonRootGridPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPressedBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ButtonContentPresenterPressedBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ButtonContentPresenterPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonRootGridDisabledBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ButtonRootGridDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ButtonContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterDisabledBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ButtonContentPresenterDisabledBorderBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ProgressBarForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ProgressBarForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ProgressRingForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ProgressRingForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarButtonItemPressedForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["PivotButtonBorderBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotButtonBorderBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotButtonBorderBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotButtonBorderBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotButtonBorderPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotButtonBorderPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotButtonArrowPointerOverForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotButtonArrowPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotButtonArrowPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotButtonArrowPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotButtonBorderPressedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotButtonBorderPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotButtonArrowForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotButtonArrowForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderBorderBachgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderBorderBachgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderBorderBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderBorderBorderBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderItemForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemDisabledForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderItemDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemSelectedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderItemSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemSelectedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderItemSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemUnselectedPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderItemUnselectedPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemSelectedPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderItemSelectedPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemUnselectedPressedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderItemUnselectedPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemSelectedPressedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderItemSelectedPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemUnselectedPointerOverForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderItemUnselectedPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemSelectedPointerOverForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderItemSelectedPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemUnselectedPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderItemUnselectedPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemSelectedPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PivotHeaderItemSelectedPressedForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxItemForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemLayoutRootPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxItemLayoutRootPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterPointerOverForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxItemContentPresenterPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxItemContentPresenterPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterSelectedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxItemContentPresenterSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterSelectedUnfocusedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxItemContentPresenterSelectedUnfocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterSelectedPointerOverForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxItemContentPresenterSelectedPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterSelectedPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxItemContentPresenterSelectedPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterSelectedDisabledForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxItemContentPresenterSelectedDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemLayoutRootPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxItemLayoutRootPressedForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ComboBoxForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxBackgroundPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundPointerOverBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxBackgroundPointerOverBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundPressedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxBackgroundPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundPressedBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxBackgroundPressedBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxHighlightBackgroundBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxHighlightBackgroundBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundDisabledBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxBackgroundDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundDisabledBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxBackgroundDisabledBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxPlaceholderTextBlockDisabledForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxPlaceholderTextBlockDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxDropDownGlyphDisabledForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxDropDownGlyphDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxHighlightBackgroundFocusedBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxHighlightBackgroundFocusedBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxContentPresenterFocusedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxContentPresenterFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxPlaceholderTextBlockFocusedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxPlaceholderTextBlockFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxContentPresenterFocusedPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxContentPresenterFocusedPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxPlaceholderTextBlockFocusedPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxPlaceholderTextBlockFocusedPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxDropDownGlyphFocusedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxDropDownGlyphFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxDropDownGlyphFocusedPressedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxDropDownGlyphFocusedPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxPlaceholderTextBlockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxPlaceholderTextBlockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxDropDownGlyphForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxDropDownGlyphForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxPopupBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxPopupBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxPopupBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxPopupBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxScrollViewerForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ComboBoxScrollViewerForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["AppBarBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarBorderBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["PageBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PageBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["AppBarTweetButtonBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarTweetButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarTweetButtonForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarTweetButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarCharacterCountForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarCharacterCountForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarMessageForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarMessageForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarReplyOrQuotedStatusAreaBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaTextBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarReplyOrQuotedStatusAreaTextBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaNameBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarReplyOrQuotedStatusAreaNameBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaScreenNameBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarReplyOrQuotedStatusAreaScreenNameBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaQuotedRetweetSymbolBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarReplyOrQuotedStatusAreaQuotedRetweetSymbolBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaReplySymbolBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarReplyOrQuotedStatusAreaReplySymbolBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaNoticeBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["AppBarReplyOrQuotedStatusAreaNoticeBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TitleBarBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TitleBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TitleBarForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TitleBarButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TitleBarButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TitleBarButtonInactiveBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TitleBarButtonInactiveForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["BottomBarBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonSelectedBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarTextblockButtonSelectedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonUnselectedBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarTextblockButtonUnselectedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarButtonSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarButtonSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarButtonUnselectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarButtonUnselectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarAppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarAppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetMultipulActionAppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetMultipulActionAppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetMultipulActionAppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetMultipulActionAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetMultipulActionAppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetMultipulActionAppBarButtonItemPressedForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["PullToRefreshCharacterBrush"]).Color = ((SolidColorBrush)ResourceDictionary["PullToRefreshCharacterBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ColumnViewBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.ColumnBackgroundBrushAlpha), ((SolidColorBrush)ResourceDictionary["ColumnViewBackgroundBrush"]).Color.R, ((SolidColorBrush)ResourceDictionary["ColumnViewBackgroundBrush"]).Color.G, ((SolidColorBrush)ResourceDictionary["ColumnViewBackgroundBrush"]).Color.B);

                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarSelectedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ColumnViewControlBarSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnselectedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ColumnViewControlBarUnselectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ColumnViewControlBarSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarDisabledSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ColumnViewControlBarDisabledSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarTextblockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ColumnViewControlBarTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnreadCountGridBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ColumnViewControlBarUnreadCountGridBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnreadCountTextblockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["ColumnViewControlBarUnreadCountTextblockForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetCheckBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetCheckBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCheckBoxBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetCheckBoxBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetDragBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetDragBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetDragForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetDragForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFocusBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetFocusBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFocusSecondaryBorderBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetFocusSecondaryBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPlaceholderBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetPlaceholderBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPointerOverForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetSelectedPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPressedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedPressedBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetSelectedPressedBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)ResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.R, ((SolidColorBrush)ResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.G, ((SolidColorBrush)ResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)ResourceDictionary["TweetRetweetBackgroundBrush"]).Color.R, ((SolidColorBrush)ResourceDictionary["TweetRetweetBackgroundBrush"]).Color.G, ((SolidColorBrush)ResourceDictionary["TweetRetweetBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)ResourceDictionary["TweetMentionBackgroundBrush"]).Color.R, ((SolidColorBrush)ResourceDictionary["TweetMentionBackgroundBrush"]).Color.G, ((SolidColorBrush)ResourceDictionary["TweetMentionBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetMyTweetBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)ResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.R, ((SolidColorBrush)ResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.G, ((SolidColorBrush)ResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.B);

                ((SolidColorBrush)Application.Current.Resources["TweetRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetNameTextblockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetNameTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetScreenNameTextblockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetScreenNameTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetTextTextblockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetTextTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetTextHyperlinkTextblockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetTextHyperlinkTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetDateTimeTextblockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetDateTimeTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSourceTextblockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetSourceTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetOtherTextblockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetOtherTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetRetweetTextblockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetRetweetTextblockForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetQuotedStatusBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetQuotedStatusBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetTargetStatusBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetTargetStatusBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarReplySymbolIconForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetCommandBarReplySymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetCommandBarRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarDestroyRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetCommandBarDestroyRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetCommandBarFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarUrlSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetCommandBarUrlSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarMenuSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetCommandBarMenuSymbolIconForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetGapTextblockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetGapTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetGapBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["TweetGapBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["SettingsFlyoutHeaderForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SettingsFlyoutHeaderForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SettingsFlyoutBackgroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SettingsFlyoutBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SettingsFlyoutTextblockForegroundBrush"]).Color = ((SolidColorBrush)ResourceDictionary["SettingsFlyoutTextblockForegroundBrush"]).Color;
            }
            catch
            {
                ((SolidColorBrush)Application.Current.Resources["SearchBoxBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxBorderThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxDisabledBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxDisabledBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxDisabledTextThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxDisabledTextThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxDisabledBorderThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxDisabledBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxPointerOverTextThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxPointerOverTextThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxPointerOverBorderThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxPointerOverBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxFocusedBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxFocusedBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxFocusedTextThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxFocusedTextThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxFocusedBorderThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxFocusedBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxButtonForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxButtonForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxButtonPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxButtonPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxSeparatorSuggestionForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxSeparatorSuggestionForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxIMECandidateListSeparatorThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxIMECandidateListSeparatorThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SearchBoxForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SearchBoxForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchPointerOverOuterBorderStrokeThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchPointerOverOuterBorderStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchPointerOverSwitchKnobOffFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchPointerOverSwitchKnobOffFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchPressedOuterBorderFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchPressedOuterBorderFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchPressedSwitchKnobBoundsStrokeThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchPressedSwitchKnobBoundsStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchPressedSwitchKnobBoundsFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchPressedSwitchKnobBoundsFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchPressedSwitchKnobOffFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchPressedSwitchKnobOffFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledHeaderContentPresenterForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchDisabledHeaderContentPresenterForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledOffContentPresenterForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchDisabledOffContentPresenterForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledOnContentPresenterForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchDisabledOnContentPresenterForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledOuterBorderStrokeThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchDisabledOuterBorderStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledSwitchKnobBoundsFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchDisabledSwitchKnobBoundsFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledSwitchKnobBoundsStrokeThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchDisabledSwitchKnobBoundsStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledSwitchKnobOffFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchDisabledSwitchKnobOffFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchDisabledSwitchKnobOnFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchDisabledSwitchKnobOnFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchHeaderContentPresenterForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchHeaderContentPresenterForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchOuterBorderStrokeThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchOuterBorderStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchSwitchKnobBoundsStrokeThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchSwitchKnobBoundsStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchSwitchKnobOnFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchSwitchKnobOnFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ToggleSwitchSwitchKnobOffFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ToggleSwitchSwitchKnobOffFillThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["SliderBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderPressedHorizontalThumbBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderPressedHorizontalThumbBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledHeaderContentPresenterForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderDisabledHeaderContentPresenterForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledHorizontalDecreaseRectFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderDisabledHorizontalDecreaseRectFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledHorizontalTrackRectFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderDisabledHorizontalTrackRectFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledVerticalDecreaseRectFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderDisabledVerticalDecreaseRectFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledVerticalTrackRectFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderDisabledVerticalTrackRectFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledHorizontalThumbBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderDisabledHorizontalThumbBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledVerticalThumbBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderDisabledVerticalThumbBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledTopTickBarFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderDisabledTopTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledBottomTickBarFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderDisabledBottomTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledLeftTickBarFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderDisabledLeftTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderDisabledRightTickBarFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderDisabledRightTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderPointerOverHorizontalTrackRectFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderPointerOverHorizontalTrackRectFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderPointerOverVerticalTrackRectFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderPointerOverVerticalTrackRectFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderPointerOverHorizontalThumbBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderPointerOverHorizontalThumbBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderPointerOverVerticalThumbBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderPointerOverVerticalThumbBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderHeaderContentPresenterForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderHeaderContentPresenterForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderTopTickBarFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderTopTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderHorizontalInlineTickBarFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderHorizontalInlineTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderBottomTickBarFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderBottomTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderLeftTickBarFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderLeftTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderVerticalInlineTickBarFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderVerticalInlineTickBarFillThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SliderRightTickBarFillThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SliderRightTickBarFillThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["FlyoutBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["FlyoutBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["FlyoutBorderThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["FlyoutBorderThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutSeparatorBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["MenuFlyoutSeparatorBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["MenuFlyoutItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemDisabledTextblockForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["MenuFlyoutItemDisabledTextblockForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemPressedBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["MenuFlyoutItemPressedBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemFocusVisualWhiteStrokeThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["MenuFlyoutItemFocusVisualWhiteStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemFocusVisualBlackStrokeThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["MenuFlyoutItemFocusVisualBlackStrokeThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["MenuFlyoutItemTextblockForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["MenuFlyoutItemTextblockForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ScrollBarRepeatButtonPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarRepeatButtonPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowPointerOverForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarArrowPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarRepeatButtonPressedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarRepeatButtonPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarArrowPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowDisabledForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarArrowDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarArrowForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarArrowForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarThumbFillBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarThumbFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarThumbPointerOverFillBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarThumbPointerOverFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarThumbPressedFillBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarThumbPressedFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarTrackRectDisabledStrokeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarTrackRectDisabledStrokeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarPanningThumbDisabledStrokeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarPanningThumbDisabledStrokeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarTrackRectFillBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarTrackRectFillBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarTrackRectStrokeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarTrackRectStrokeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ScrollBarPanningThumbBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ScrollBarPanningThumbBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TextBoxForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxSelectionHighlightBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxSelectionHighlightBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxButtonBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxButtonBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxButtonBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxGlyphElementPointerOverForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxGlyphElementPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxButtonLayoutGridPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxButtonLayoutGridPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxGlyphElementPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxGlyphElementPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxGlyphElementForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxGlyphElementForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxHeaderContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxHeaderContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBackgroundElementDisabledBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxBackgroundElementDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementDisabledBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxBorderElementDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementDisabledBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxBorderElementDisabledBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxContentElementDisabledForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxContentElementDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxPlaceholderTextContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxPlaceholderTextContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementPointerOverBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxBorderElementPointerOverBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxPlaceholderTextContentPresenterFocusedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxPlaceholderTextContentPresenterFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBackgroundElementFocusedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxBackgroundElementFocusedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxBorderElementFocusedBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxBorderElementFocusedBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxContentElementFocusedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxContentElementFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TextBoxHeaderContentPresenterForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TextBoxHeaderContentPresenterForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ButtonBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ButtonBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPointerOverBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ButtonContentPresenterPointerOverBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPointerOverForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ButtonContentPresenterPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonRootGridPressedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ButtonRootGridPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPressedBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ButtonContentPresenterPressedBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ButtonContentPresenterPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonRootGridDisabledBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ButtonRootGridDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ButtonContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ButtonContentPresenterDisabledBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ButtonContentPresenterDisabledBorderBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ProgressBarForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ProgressBarForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ProgressRingForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ProgressRingForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarButtonItemPressedForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["PivotButtonBorderBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotButtonBorderBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotButtonBorderBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotButtonBorderBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotButtonBorderPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotButtonBorderPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotButtonArrowPointerOverForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotButtonArrowPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotButtonArrowPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotButtonArrowPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotButtonBorderPressedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotButtonBorderPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotButtonArrowForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotButtonArrowForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderBorderBachgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderBorderBachgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderBorderBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderBorderBorderBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderItemForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemDisabledForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderItemDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemSelectedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderItemSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemSelectedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderItemSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemUnselectedPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderItemUnselectedPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemSelectedPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderItemSelectedPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemUnselectedPressedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderItemUnselectedPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemSelectedPressedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderItemSelectedPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemUnselectedPointerOverForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderItemUnselectedPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemSelectedPointerOverForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderItemSelectedPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemUnselectedPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderItemUnselectedPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["PivotHeaderItemSelectedPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PivotHeaderItemSelectedPressedForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxItemForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemLayoutRootPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxItemLayoutRootPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterPointerOverForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxItemContentPresenterPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxItemContentPresenterPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterSelectedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxItemContentPresenterSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterSelectedUnfocusedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxItemContentPresenterSelectedUnfocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterSelectedPointerOverForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxItemContentPresenterSelectedPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterSelectedPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxItemContentPresenterSelectedPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemContentPresenterSelectedDisabledForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxItemContentPresenterSelectedDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxItemLayoutRootPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxItemLayoutRootPressedForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ComboBoxForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxBackgroundPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundPointerOverBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxBackgroundPointerOverBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundPressedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxBackgroundPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundPressedBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxBackgroundPressedBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxHighlightBackgroundBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxHighlightBackgroundBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundDisabledBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxBackgroundDisabledBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxBackgroundDisabledBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxBackgroundDisabledBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxContentPresenterDisabledForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxContentPresenterDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxPlaceholderTextBlockDisabledForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxPlaceholderTextBlockDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxDropDownGlyphDisabledForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxDropDownGlyphDisabledForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxHighlightBackgroundFocusedBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxHighlightBackgroundFocusedBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxContentPresenterFocusedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxContentPresenterFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxPlaceholderTextBlockFocusedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxPlaceholderTextBlockFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxContentPresenterFocusedPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxContentPresenterFocusedPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxPlaceholderTextBlockFocusedPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxPlaceholderTextBlockFocusedPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxDropDownGlyphFocusedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxDropDownGlyphFocusedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxDropDownGlyphFocusedPressedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxDropDownGlyphFocusedPressedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxPlaceholderTextBlockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxPlaceholderTextBlockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxDropDownGlyphForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxDropDownGlyphForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxPopupBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxPopupBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxPopupBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxPopupBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ComboBoxScrollViewerForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ComboBoxScrollViewerForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["AppBarBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarBorderBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["PageBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PageBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["AppBarTweetButtonBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarTweetButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarTweetButtonForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarTweetButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarCharacterCountForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarCharacterCountForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarMessageForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarMessageForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaTextBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaTextBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaNameBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaNameBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaScreenNameBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaScreenNameBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaQuotedRetweetSymbolBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaQuotedRetweetSymbolBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaReplySymbolBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaReplySymbolBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["AppBarReplyOrQuotedStatusAreaNoticeBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["AppBarReplyOrQuotedStatusAreaNoticeBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TitleBarBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TitleBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TitleBarForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TitleBarButtonBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TitleBarButtonForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TitleBarButtonInactiveBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TitleBarButtonInactiveForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TitleBarButtonInactiveForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["BottomBarBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonSelectedBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarTextblockButtonSelectedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarTextblockButtonUnselectedBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarTextblockButtonUnselectedBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarButtonSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarButtonSelectedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarButtonSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarButtonUnselectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarButtonUnselectedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarButtonUnselectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarAppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarAppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarAppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarAppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["BottomBarAppBarButtonItemPressedForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetMultipulActionAppBarButtonItemBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemDisabledForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetMultipulActionAppBarButtonItemDisabledForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetMultipulActionAppBarButtonItemForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetMultipulActionAppBarButtonItemPointerOverBackgroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemPointerOverForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetMultipulActionAppBarButtonItemPointerOverForegroundThemeBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetMultipulActionAppBarButtonItemPressedForegroundThemeBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetMultipulActionAppBarButtonItemPressedForegroundThemeBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["PullToRefreshCharacterBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["PullToRefreshCharacterBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["ColumnViewBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.ColumnBackgroundBrushAlpha), ((SolidColorBrush)DefaultResourceDictionary["ColumnViewBackgroundBrush"]).Color.R, ((SolidColorBrush)DefaultResourceDictionary["ColumnViewBackgroundBrush"]).Color.G, ((SolidColorBrush)DefaultResourceDictionary["ColumnViewBackgroundBrush"]).Color.B);

                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarSelectedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ColumnViewControlBarSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnselectedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ColumnViewControlBarUnselectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ColumnViewControlBarSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarDisabledSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ColumnViewControlBarDisabledSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarTextblockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ColumnViewControlBarTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnreadCountGridBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ColumnViewControlBarUnreadCountGridBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["ColumnViewControlBarUnreadCountTextblockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["ColumnViewControlBarUnreadCountTextblockForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetCheckBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetCheckBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCheckBoxBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetCheckBoxBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetDragBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetDragBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetDragForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetDragForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFocusBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetFocusBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetFocusSecondaryBorderBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetFocusSecondaryBorderBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPlaceholderBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetPlaceholderBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPointerOverForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetPointerOverForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetSelectedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetSelectedForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedPointerOverBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetSelectedPointerOverBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetPressedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetPressedBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSelectedPressedBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetSelectedPressedBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)DefaultResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.R, ((SolidColorBrush)DefaultResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.G, ((SolidColorBrush)DefaultResourceDictionary["TweetFavoriteBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)DefaultResourceDictionary["TweetRetweetBackgroundBrush"]).Color.R, ((SolidColorBrush)DefaultResourceDictionary["TweetRetweetBackgroundBrush"]).Color.G, ((SolidColorBrush)DefaultResourceDictionary["TweetRetweetBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetMentionBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)DefaultResourceDictionary["TweetMentionBackgroundBrush"]).Color.R, ((SolidColorBrush)DefaultResourceDictionary["TweetMentionBackgroundBrush"]).Color.G, ((SolidColorBrush)DefaultResourceDictionary["TweetMentionBackgroundBrush"]).Color.B);
                ((SolidColorBrush)Application.Current.Resources["TweetMyTweetBackgroundBrush"]).Color = Color.FromArgb(Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha), ((SolidColorBrush)DefaultResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.R, ((SolidColorBrush)DefaultResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.G, ((SolidColorBrush)DefaultResourceDictionary["TweetMyTweetBackgroundBrush"]).Color.B);

                ((SolidColorBrush)Application.Current.Resources["TweetRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetNameTextblockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetNameTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetScreenNameTextblockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetScreenNameTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetTextTextblockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetTextTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetTextHyperlinkTextblockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetTextHyperlinkTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetDateTimeTextblockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetDateTimeTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetSourceTextblockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetSourceTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetOtherTextblockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetOtherTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetRetweetTextblockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetRetweetTextblockForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetQuotedStatusBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetQuotedStatusBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetTargetStatusBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetTargetStatusBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarReplySymbolIconForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetCommandBarReplySymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetCommandBarRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarDestroyRetweetSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetCommandBarDestroyRetweetSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetCommandBarFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarUrlSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetCommandBarUrlSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetCommandBarDestroyFavoriteSymbolIconForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetCommandBarMenuSymbolIconForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetCommandBarMenuSymbolIconForegroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["TweetGapTextblockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetGapTextblockForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["TweetGapBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["TweetGapBackgroundBrush"]).Color;

                ((SolidColorBrush)Application.Current.Resources["SettingsFlyoutHeaderForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SettingsFlyoutHeaderForegroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SettingsFlyoutBackgroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SettingsFlyoutBackgroundBrush"]).Color;
                ((SolidColorBrush)Application.Current.Resources["SettingsFlyoutTextblockForegroundBrush"]).Color = ((SolidColorBrush)DefaultResourceDictionary["SettingsFlyoutTextblockForegroundBrush"]).Color;
            }

            OnPropertyChanged();
        }

        public void ChangeBackgroundAlpha()
        {
            ((SolidColorBrush) Application.Current.Resources["ColumnViewBackgroundBrush"]).Color = Color.FromArgb(
                Convert.ToByte(SettingService.Setting.ColumnBackgroundBrushAlpha),
                ((SolidColorBrush) Application.Current.Resources["ColumnViewBackgroundBrush"]).Color.R,
                ((SolidColorBrush) Application.Current.Resources["ColumnViewBackgroundBrush"]).Color.G,
                ((SolidColorBrush) Application.Current.Resources["ColumnViewBackgroundBrush"]).Color.B);

            ((SolidColorBrush) Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color = Color.FromArgb(
                Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha),
                ((SolidColorBrush) Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color.R,
                ((SolidColorBrush) Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color.G,
                ((SolidColorBrush) Application.Current.Resources["TweetFavoriteBackgroundBrush"]).Color.B);
            ((SolidColorBrush) Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color = Color.FromArgb(
                Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha),
                ((SolidColorBrush) Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color.R,
                ((SolidColorBrush) Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color.G,
                ((SolidColorBrush) Application.Current.Resources["TweetRetweetBackgroundBrush"]).Color.B);
            ((SolidColorBrush) Application.Current.Resources["TweetMentionBackgroundBrush"]).Color = Color.FromArgb(
                Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha),
                ((SolidColorBrush) Application.Current.Resources["TweetMentionBackgroundBrush"]).Color.R,
                ((SolidColorBrush) Application.Current.Resources["TweetMentionBackgroundBrush"]).Color.G,
                ((SolidColorBrush) Application.Current.Resources["TweetMentionBackgroundBrush"]).Color.B);
            ((SolidColorBrush) Application.Current.Resources["TweetMyTweetBackgroundBrush"]).Color = Color.FromArgb(
                Convert.ToByte(SettingService.Setting.TweetBackgroundBrushAlpha),
                ((SolidColorBrush) Application.Current.Resources["TweetMyTweetBackgroundBrush"]).Color.R,
                ((SolidColorBrush) Application.Current.Resources["TweetMyTweetBackgroundBrush"]).Color.G,
                ((SolidColorBrush) Application.Current.Resources["TweetMyTweetBackgroundBrush"]).Color.B);
        }
    }
}