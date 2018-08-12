using System;
using System.Reactive.Linq;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Exceptions;
using Flantter.MilkyWay.Models.Filter;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts.Settings
{
    public class ColumnSettingSettingsFlyoutViewModel
    {
        private ResourceLoader _resourceLoader;

        public ColumnSettingSettingsFlyoutViewModel()
        {
            _resourceLoader = new ResourceLoader();

            CanChangeSetting = new ReactiveProperty<bool>(true);

            Name = new ReactiveProperty<string>();
            Filter = new ReactiveProperty<string>();
            DisableStartupRefresh = new ReactiveProperty<bool>();
            AutoRefresh = new ReactiveProperty<bool>();
            AutoRefreshTimerInterval = new ReactiveProperty<double>();
            FetchingNumberOfTweet = new ReactiveProperty<double>();

            ErrorMessage = new ReactiveProperty<string>();
            UpdateButtonEnabled = new ReactiveProperty<bool>(true);

            ColumnSetting = new ReactiveProperty<ColumnSetting>();
            ColumnSetting.Subscribe(x =>
            {
                if (x == null)
                    return;

                Filter.Value = x.Filter;
                Name.Value = x.Name;
                DisableStartupRefresh.Value = x.DisableStartupRefresh;
                AutoRefresh.Value = x.AutoRefresh;
                AutoRefreshTimerInterval.Value = x.AutoRefreshTimerInterval;
                FetchingNumberOfTweet.Value = x.FetchingNumberOfTweet;

                if (ColumnSetting.Value.Action == SettingSupport.ColumnTypeEnum.Home ||
                    ColumnSetting.Value.Action == SettingSupport.ColumnTypeEnum.Mentions || ColumnSetting.Value.Action ==
                    SettingSupport.ColumnTypeEnum.Favorites)
                    CanChangeSetting.Value = false;
                else
                    CanChangeSetting.Value = true;
            });


            Filter.CombineLatest(Name, (filter, name) => new {Filter = filter, Name = name})
                .Subscribe(x =>
                {
                    if (string.IsNullOrEmpty(x.Name))
                    {
                        ErrorMessage.Value =
                            _resourceLoader.GetString("SettingsFlyout_Settings_Column_Name_NameIsEmpty");
                        UpdateButtonEnabled.Value = false;
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(x.Filter))
                    {
                        ErrorMessage.Value =
                            _resourceLoader.GetString("SettingsFlyout_Settings_Column_Filter_FilterIsEmpty");
                        UpdateButtonEnabled.Value = false;
                        return;
                    }

                    try
                    {
                        Compiler.Compile(x.Filter, false);
                    }
                    catch (FilterCompileException e)
                    {
                        ErrorMessage.Value =
                            _resourceLoader.GetString(
                                "SettingsFlyout_Settings_Mute_MuteFilter_FilterCompileError") +
                            "\n" + _resourceLoader.GetString("Filter_CompileError_" + e.Error.ToString());
                        UpdateButtonEnabled.Value = false;
                        return;
                    }
                    catch (Exception e)
                    {
                        ErrorMessage.Value =
                            _resourceLoader.GetString(
                                "SettingsFlyout_Settings_Mute_MuteFilter_FilterCompileError") +
                            "\n" + e.Message;
                        UpdateButtonEnabled.Value = false;
                        return;
                    }

                    ErrorMessage.Value = _resourceLoader.GetString("SettingsFlyout_Settings_Column_SaveOK");
                    UpdateButtonEnabled.Value = true;
                });


            SaveColumnSettingCommand = new ReactiveCommand();
            SaveColumnSettingCommand.Subscribe(async x =>
            {
                ColumnSetting.Value.Name = Name.Value;
                ColumnSetting.Value.Filter = Filter.Value;
                ColumnSetting.Value.DisableStartupRefresh = DisableStartupRefresh.Value;
                ColumnSetting.Value.AutoRefresh = AutoRefresh.Value;
                ColumnSetting.Value.AutoRefreshTimerInterval = AutoRefreshTimerInterval.Value;
                ColumnSetting.Value.FetchingNumberOfTweet = (int) FetchingNumberOfTweet.Value;

                await AdvancedSettingService.AdvancedSetting.SaveToAppSettings();

                await Notice.Instance.ShowMessageDialogMessenger.Raise(new MessageDialogNotification
                {
                    Message = _resourceLoader.GetString("ConfirmDialog_UpdateColumnSettingSuccessfully"),
                    Title = "Message"
                });
            });
        }

        public ReactiveProperty<ColumnSetting> ColumnSetting { get; set; }

        public ReactiveProperty<bool> CanChangeSetting { get; set; }

        public ReactiveProperty<string> Name { get; set; }
        public ReactiveProperty<string> Filter { get; set; }
        public ReactiveProperty<bool> DisableStartupRefresh { get; set; }
        public ReactiveProperty<bool> AutoRefresh { get; set; }
        public ReactiveProperty<double> AutoRefreshTimerInterval { get; set; }
        public ReactiveProperty<double> FetchingNumberOfTweet { get; set; }

        public ReactiveProperty<string> ErrorMessage { get; set; }

        public ReactiveProperty<bool> UpdateButtonEnabled { get; set; }


        public ReactiveCommand SaveColumnSettingCommand { get; set; }
    }
}