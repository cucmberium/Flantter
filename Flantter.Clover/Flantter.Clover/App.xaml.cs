using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Prism.Windows;

namespace Flantter.Clover
{
    sealed partial class App : PrismApplication
    {
        private bool _appLaunched;

        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            Resuming += OnResuming;
            UnhandledException += OnUnhandledException;

            RequestedTheme = ApplicationTheme.Light;
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: Suspend
            deferral.Complete();
        }

        private void OnResuming(object sender, object e)
        {
        }

        private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // 別変数に最初に入れないと次アクセスからNull , 一番最初のUnhandledExceptionじゃないとNull
            var stacktrace = e.Exception.StackTrace;
            Debug.WriteLine(stacktrace);
            e.Handled = true;
        }

        protected override async Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            await InitializeApplicationAsync();

            _appLaunched = true;
        }

        protected override async Task OnActivateApplicationAsync(IActivatedEventArgs args)
        {
            if (!_appLaunched)
            {
                await InitializeApplicationAsync();

                _appLaunched = true;
            }
        }

        private async Task InitializeApplicationAsync()
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size { Width = 300, Height = 500 });

            NavigationService.Navigate("Main", "");

            DeviceGestureService.GoBackRequested += (s, e) =>
            {
            };
        }
    }
}
