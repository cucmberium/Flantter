using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;

namespace Flantter.MilkyWay.License
{
    public class LicenseService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (!Windows.ApplicationModel.Core.CoreApplication.MainView.Dispatcher.HasThreadAccess)
                return;

            var h = PropertyChanged;
            if (h != null) h(this, new PropertyChangedEventArgs(name));
        }


        private static LicenseService _instance;
        public static LicenseService License { get { return _instance ?? (_instance = new LicenseService()); } }
        private LicenseService()
        {
#if DEBUG
            this.LicenseInformation = CurrentAppSimulator.LicenseInformation;
#else
            this.LicenseInformation = CurrentApp.LicenseInformation;
#endif

            this.LicenseInformation.LicenseChanged += LicenseInformation_LicenseChanged;
        }


        private void LicenseInformation_LicenseChanged()
        {
            this.OnPropertyChanged();
        }

        public LicenseInformation LicenseInformation { get; private set; }


        public bool AppDonationIsActive
        {
            get
            {
                return this.LicenseInformation.ProductLicenses["AppDonation"].IsActive;
            }
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
                return (result.Status == ProductPurchaseStatus.Succeeded);
            }
            catch
            {
            }

            return false;
        }
    }
}
