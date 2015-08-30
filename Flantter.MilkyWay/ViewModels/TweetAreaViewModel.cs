using Flantter.MilkyWay.Models;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;

namespace Flantter.MilkyWay.ViewModels
{
    public class TweetAreaViewModel
    {
        private MainPageModel _MainPageModel { get; set; }
        private TweetAreaModel _TweetAreaModel { get; set; }

        public ReadOnlyReactiveCollection<AccountViewModel> Accounts { get; private set; }

        public ReactiveProperty<AccountViewModel> SelectedAccount { get; set; }

        public ReactiveProperty<string> Text { get; set; }
        public ReactiveProperty<string> CharacterCount { get; set; }

        public TweetAreaViewModel(ReadOnlyReactiveCollection<AccountViewModel> accounts)
        {
            this._MainPageModel = MainPageModel.Instance;
            this._TweetAreaModel = new TweetAreaModel();

            this.Accounts = accounts;
            this.SelectedAccount = new ReactiveProperty<AccountViewModel>(Accounts.First());

            this.Text = this._TweetAreaModel.ToReactivePropertyAsSynchronized(x => x.Text);
            this.CharacterCount = this._TweetAreaModel.ObserveProperty(x => x.CharacterCount).Select(x => x.ToString()).ToReactiveProperty();

        }


    }
}
