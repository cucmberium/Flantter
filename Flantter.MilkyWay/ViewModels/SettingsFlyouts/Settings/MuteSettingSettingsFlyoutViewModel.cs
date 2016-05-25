using Flantter.MilkyWay.Models.Exceptions;
using Flantter.MilkyWay.Models.Filter;
using Flantter.MilkyWay.Setting;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Flantter.MilkyWay.ViewModels.SettingsFlyouts.Settings
{
    public class MuteSettingSettingsFlyoutViewModel
    {
        public MuteSettingSettingsFlyoutViewModel()
        {
            this.MuteFilterUpdateButtonEnabled = new ReactiveProperty<bool>();
            this.MuteFilterCompileErrorMessage = new ReactiveProperty<string>();
            this.MuteFilter = new ReactiveProperty<string>();
            this.MuteFilter.Subscribe(x => 
            {
                if (string.IsNullOrWhiteSpace(x))
                {
                    this.MuteFilterCompileErrorMessage.Value = new ResourceLoader().GetString("SettingsFlyout_Settings_Mute_MuteFilter_FilterIsEmpty");
                    this.MuteFilterUpdateButtonEnabled.Value = false;
                    return;
                }

                try
                {
                    Compiler.Compile(x, true);
                }
                catch (FilterCompileException e)
                {
                    this.MuteFilterCompileErrorMessage.Value = new ResourceLoader().GetString("SettingsFlyout_Settings_Mute_MuteFilter_FilterCompileError") + "\n" + new ResourceLoader().GetString("Filter_CompileError_" + e.Error.ToString());
                    this.MuteFilterUpdateButtonEnabled.Value = false;
                    return;
                }
                catch (Exception e)
                {
                    this.MuteFilterCompileErrorMessage.Value = new ResourceLoader().GetString("SettingsFlyout_Settings_Mute_MuteFilter_FilterCompileError") + "\n" + e.Message;
                    this.MuteFilterUpdateButtonEnabled.Value = false;
                    return;
                }

                this.MuteFilterCompileErrorMessage.Value = "";
                this.MuteFilterUpdateButtonEnabled.Value = true;
            });
        }

        public ReactiveProperty<string> MuteFilter { get; set; }

        public ReactiveProperty<string> MuteFilterCompileErrorMessage { get; set; }

        public ReactiveProperty<bool> MuteFilterUpdateButtonEnabled { get; set; }
    }
}
