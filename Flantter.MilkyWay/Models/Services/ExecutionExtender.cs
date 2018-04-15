using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.System.Power;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Flantter.MilkyWay.Setting;
using Reactive.Bindings.Extensions;

namespace Flantter.MilkyWay.Models.Services
{
    public class ExecutionExtender
    {
        private ExtendedExecutionSession _session;
        private bool _initialized;

        private ExecutionExtender()
        {
        }

        public static ExecutionExtender Instance { get; } = new ExecutionExtender();
        public bool IsRunning => _session != null;

        public void Initialize()
        {
            if (_initialized)
                return;

            SettingService.Setting.ObserveProperty(x => x.ExtendedExecution).Skip(1).Subscribe(x => UpdateState());
            Observable.FromEventPattern<object>(
                    h => PowerManager.BatteryStatusChanged += h,
                    h => PowerManager.BatteryStatusChanged -= h)
                .Subscribe(e => UpdateState());
            Observable.FromEventPattern<object>(
                    h => Application.Current.Resuming += h,
                    h => Application.Current.Resuming -= h)
                .Subscribe(e => UpdateState());

            UpdateState();

            _initialized = true;
        }

        public async void UpdateState()
        {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop" &&
                SettingService.Setting.ExtendedExecution &&
                PowerManager.BatteryStatus == BatteryStatus.NotPresent)
                await RequestSessionAsync();
            else
                ClearSession();
        }

        public async Task<ExtendedExecutionResult> RequestSessionAsync()
        {
            ClearSession();

            var newSession = new ExtendedExecutionSession();
            newSession.Reason = ExtendedExecutionReason.Unspecified;
            newSession.Revoked += SessionRevoked;

            var result = await newSession.RequestExtensionAsync();

            switch (result)
            {
                case ExtendedExecutionResult.Allowed:
                    _session = newSession;
                    break;
                default:
                    newSession.Dispose();
                    break;
            }

            return result;
        }


        public void ClearSession()
        {
            if (_session != null)
            {
                _session.Dispose();
                _session = null;
            }
        }

        private void SessionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            if (_session != null)
            {
                _session.Dispose();
                _session = null;
            }

            switch (args.Reason)
            {
                case ExtendedExecutionRevokedReason.Resumed:
                    Debug.WriteLine("Extended execution revoked due to returning to foreground.");
                    break;
                case ExtendedExecutionRevokedReason.SystemPolicy:
                    Debug.WriteLine("Extended execution revoked due to system policy.");
                    break;
            }
        }
    }
}