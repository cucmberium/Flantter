using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;

namespace Flantter.MilkyWay.Models.Services
{
    public static class ExtendedExecutionHelper
    {
        private static ExtendedExecutionSession _session;

        public static bool IsRunning => _session != null;

        public static async Task<ExtendedExecutionResult> RequestSessionAsync(ExtendedExecutionReason reason, TypedEventHandler<object, ExtendedExecutionRevokedEventArgs> revoked = null)
        {   
            ClearSession();

            var newSession = new ExtendedExecutionSession();
            newSession.Reason = ExtendedExecutionReason.Unspecified;
            newSession.Revoked += SessionRevoked;

            if (revoked != null)
                newSession.Revoked += revoked;

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

        public static void ClearSession()
        {
            if (_session != null)
            {
                _session.Dispose();
                _session = null;
            }
        }

        private static void SessionRevoked(object sender, ExtendedExecutionRevokedEventArgs args)
        {
            if (_session != null)
            {
                _session.Dispose();
                _session = null;
            }

            switch (args.Reason)
            {
                case ExtendedExecutionRevokedReason.Resumed:
                    System.Diagnostics.Debug.WriteLine("Extended execution revoked due to returning to foreground.");
                    break;
                case ExtendedExecutionRevokedReason.SystemPolicy:
                    System.Diagnostics.Debug.WriteLine("Extended execution revoked due to system policy.");
                    break;
            }
        }
    }
}
