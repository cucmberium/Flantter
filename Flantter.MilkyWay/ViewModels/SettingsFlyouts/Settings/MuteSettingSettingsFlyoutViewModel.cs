using System;
using Windows.ApplicationModel.Resources;
using Flantter.MilkyWay.Models.Exceptions;
using Flantter.MilkyWay.Models.Filter;
using Reactive.Bindings;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts.Settings
{
    public class MuteSettingSettingsFlyoutViewModel
    {
        private ResourceLoader _resourceLoader;

        public MuteSettingSettingsFlyoutViewModel()
        {
            _resourceLoader = new ResourceLoader();

            MuteFilterUpdateButtonEnabled = new ReactiveProperty<bool>();
            MuteFilterCompileErrorMessage = new ReactiveProperty<string>();
            MuteFilter = new ReactiveProperty<string>();
            MuteFilter.Subscribe(x =>
            {
                if (string.IsNullOrWhiteSpace(x))
                {
                    MuteFilterCompileErrorMessage.Value =
                        _resourceLoader.GetString("SettingsFlyout_Settings_Mute_MuteFilter_FilterIsEmpty");
                    MuteFilterUpdateButtonEnabled.Value = false;
                    return;
                }

                try
                {
                    Compiler.Compile(x, true);
                }
                catch (FilterCompileException e)
                {
                    MuteFilterCompileErrorMessage.Value =
                        _resourceLoader.GetString("SettingsFlyout_Settings_Mute_MuteFilter_FilterCompileError") +
                        "\n" + _resourceLoader.GetString("Filter_CompileError_" + e.Error.ToString());
                    MuteFilterUpdateButtonEnabled.Value = false;
                    return;
                }
                catch (Exception e)
                {
                    MuteFilterCompileErrorMessage.Value =
                        _resourceLoader.GetString("SettingsFlyout_Settings_Mute_MuteFilter_FilterCompileError") +
                        "\n" + e.Message;
                    MuteFilterUpdateButtonEnabled.Value = false;
                    return;
                }

                MuteFilterCompileErrorMessage.Value = "";
                MuteFilterUpdateButtonEnabled.Value = true;
            });
        }

        public ReactiveProperty<string> MuteFilter { get; set; }

        public ReactiveProperty<string> MuteFilterCompileErrorMessage { get; set; }

        public ReactiveProperty<bool> MuteFilterUpdateButtonEnabled { get; set; }
    }
}