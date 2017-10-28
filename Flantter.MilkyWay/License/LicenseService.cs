using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Store;

namespace Flantter.MilkyWay.License
{
    public class LicenseService : INotifyPropertyChanged
    {
        private static LicenseService _instance;

        private LicenseService()
        {
#if DEBUG
            LicenseInformation = CurrentAppSimulator.LicenseInformation;
#else
            LicenseInformation = CurrentApp.LicenseInformation;
#endif

            LicenseInformation.LicenseChanged += LicenseInformation_LicenseChanged;
        }

        public static LicenseService License => _instance ?? (_instance = new LicenseService());

        public LicenseInformation LicenseInformation { get; }


        public bool AppDonationIsActive
        {
            get
            {
#if DEBUG
                return true;
#else
                return LicenseInformation.ProductLicenses["AppDonation"].IsActive;
#endif
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (!CoreApplication.MainView.Dispatcher.HasThreadAccess)
                return;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        private void LicenseInformation_LicenseChanged()
        {
            OnPropertyChanged();
        }

        public async Task<bool> PurchaseAppDonation()
        {
            try
            {
#if DEBUG
                var result = await CurrentAppSimulator.RequestProductPurchaseAsync("AppDonation");
#else
                var result = await CurrentApp.RequestProductPurchaseAsync("AppDonation");
#endif
                return result.Status == ProductPurchaseStatus.Succeeded;
            }
            catch
            {
            }

            return false;
        }
    }
}