using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels
{
    public class MainSwipeMenuViewModel
    {
        public MainSwipeMenuViewModel(ReadOnlyReactiveCollection<AccountViewModel> accounts)
        {
            this.SelectedAccount = new ReactiveProperty<AccountViewModel>();

            this.Notice = Services.Notice.Instance;
        }

        public Services.Notice Notice { get; set; }

        public ReactiveProperty<AccountViewModel> SelectedAccount { get; set; }
    }
}
