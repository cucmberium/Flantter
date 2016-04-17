using Flantter.MilkyWay.Models.Exceptions;
using Flantter.MilkyWay.Models.Filter;
using Flantter.MilkyWay.Setting;
using Flantter.MilkyWay.ViewModels.Services;
using Flantter.MilkyWay.Views.Util;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts.Settings
{
    public class ColumnSettingSettingsFlyoutViewModel
    {
        public ColumnSettingSettingsFlyoutViewModel()
        {
            this.CanChangeSetting = new ReactiveProperty<bool>(true);

            this.Name = new ReactiveProperty<string>();
            this.Filter = new ReactiveProperty<string>();
            this.DisableStartupRefresh = new ReactiveProperty<bool>();
            this.AutoRefresh = new ReactiveProperty<bool>();
            this.AutoRefreshTimerInterval = new ReactiveProperty<double>();
            this.FetchingNumberOfTweet = new ReactiveProperty<int>();

            this.ErrorMessage = new ReactiveProperty<string>();
            this.UpdateButtonEnabled = new ReactiveProperty<bool>(true);

            this.ColumnSetting = new ReactiveProperty<ColumnSetting>();
            this.ColumnSetting.Subscribe(x =>
            {
                if (x == null)
                    return;

                this.Filter.Value = x.Filter;
                this.Name.Value = x.Name;
                this.DisableStartupRefresh.Value = x.DisableStartupRefresh;
                this.AutoRefresh.Value = x.AutoRefresh;
                this.AutoRefreshTimerInterval.Value = x.AutoRefreshTimerInterval;
                this.FetchingNumberOfTweet.Value = x.FetchingNumberOfTweet;

                if (this.ColumnSetting.Value.Action == SettingSupport.ColumnTypeEnum.Home || this.ColumnSetting.Value.Action == SettingSupport.ColumnTypeEnum.Mentions || this.ColumnSetting.Value.Action == SettingSupport.ColumnTypeEnum.DirectMessages || this.ColumnSetting.Value.Action == SettingSupport.ColumnTypeEnum.Events || this.ColumnSetting.Value.Action == SettingSupport.ColumnTypeEnum.Favorites)
                    this.CanChangeSetting.Value = false;
                else
                    this.CanChangeSetting.Value = true;
            });


            Observable.CombineLatest(this.Filter, this.Name, (filter, name) => new { Filter = filter, Name = name }).Subscribe(x =>
            {
                if (string.IsNullOrEmpty(x.Name))
                {
                    this.ErrorMessage.Value = new ResourceLoader().GetString("SettingsFlyout_Settings_Column_Filter_FilterIsEmpty");
                    this.UpdateButtonEnabled.Value = false;
                    return;
                }

                if (string.IsNullOrWhiteSpace(x.Filter))
                {
                    this.ErrorMessage.Value = new ResourceLoader().GetString("SettingsFlyout_Settings_Column_Name_NameIsEmpty");
                    this.UpdateButtonEnabled.Value = false;
                    return;
                }

                try
                {
                    Compiler.Compile(x.Filter);
                }
                catch (FilterCompileException e)
                {
                    this.ErrorMessage.Value = new ResourceLoader().GetString("SettingsFlyout_Settings_Mute_MuteFilter_FilterCompileError") + "\n" + new ResourceLoader().GetString("Filter_CompileError_" + e.Error.ToString());
                    this.UpdateButtonEnabled.Value = false;
                    return;
                }
                catch (Exception e)
                {
                    this.ErrorMessage.Value = new ResourceLoader().GetString("SettingsFlyout_Settings_Mute_MuteFilter_FilterCompileError") + "\n" + e.Message;
                    this.UpdateButtonEnabled.Value = false;
                    return;
                }

                this.ErrorMessage.Value = new ResourceLoader().GetString("SettingsFlyout_Settings_Column_SaveOK");
                this.UpdateButtonEnabled.Value = true;
            });


            this.SaveColumnSettingCommand = new ReactiveCommand();
            this.SaveColumnSettingCommand.Subscribe(async x => 
            {
                this.ColumnSetting.Value.Name = this.Name.Value;
                this.ColumnSetting.Value.Filter = this.Filter.Value;
                this.ColumnSetting.Value.DisableStartupRefresh = this.DisableStartupRefresh.Value;
                this.ColumnSetting.Value.AutoRefresh = this.AutoRefresh.Value;
                this.ColumnSetting.Value.AutoRefreshTimerInterval = this.AutoRefreshTimerInterval.Value;
                this.ColumnSetting.Value.FetchingNumberOfTweet = this.FetchingNumberOfTweet.Value;

                AdvancedSettingService.AdvancedSetting.SaveToAppSettings();

                await Services.Notice.Instance.ShowMessageDialogMessenger.Raise(new MessageDialogNotification() { Message = new ResourceLoader().GetString("ConfirmDialog_UpdateColumnSettingSuccessfully"), Title = "Message" });
                return;
            });
        }

        public ReactiveProperty<ColumnSetting> ColumnSetting { get; set; }

        public ReactiveProperty<bool> CanChangeSetting { get; set; }

        public ReactiveProperty<string> Name { get; set; }
        public ReactiveProperty<string> Filter { get; set; }
        public ReactiveProperty<bool> DisableStartupRefresh { get; set; }
        public ReactiveProperty<bool> AutoRefresh { get; set; }
        public ReactiveProperty<double> AutoRefreshTimerInterval { get; set; }
        public ReactiveProperty<int> FetchingNumberOfTweet { get; set; }

        public ReactiveProperty<string> ErrorMessage { get; set; }

        public ReactiveProperty<bool> UpdateButtonEnabled { get; set; }


        public ReactiveCommand SaveColumnSettingCommand { get; set; }
    }
}
