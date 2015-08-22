﻿using Flantter.MilkyWay.Models;
using Flantter.MilkyWay.Setting;
using Microsoft.Practices.Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Flantter.MilkyWay.ViewModels
{
    public class MainPageViewModel : ViewModel
    {
        private MainPageModel _MainPageModel { get; set; }

        public ReadOnlyReactiveCollection<AccountViewModel> Accounts { get; private set; }
        public ReactiveProperty<bool> TitleBarVisivility { get; private set; }
        public ReactiveProperty<bool> AppBarIsOpen { get; private set; }

        #region Constructor
        public MainPageViewModel()
        {
            this._MainPageModel = MainPageModel.Instance;
            this.Accounts = this._MainPageModel.ReadOnlyAccounts.ToReadOnlyReactiveCollection(x => new AccountViewModel(x));

            // 設定によってTitlebarの表示を変える
            this.TitleBarVisivility = SettingService.Setting.ObserveProperty(x => x.TitleBarVisibility).ToReactiveProperty();

            this.AppBarIsOpen = new ReactiveProperty<bool>(false);

            #region Command

            Services.Notice.Instance.TweetCommand.Subscribe(x => 
            {
                var accountViewModel = x as AccountViewModel;
                if (accountViewModel == null)
                    return;

                this.AppBarIsOpen.Value = true;
            });

            #endregion
        }
        #endregion

        #region Destructor
        ~MainPageViewModel()
        {
        }
        #endregion

        #region Others
        public override void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);

            Task.Run(() => this._MainPageModel.Initialize());
        }
        #endregion
    }
}
