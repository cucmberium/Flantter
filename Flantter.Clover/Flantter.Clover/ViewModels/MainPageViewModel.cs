using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flantter.Clover.Models;
using Prism.Windows.Mvvm;

namespace Flantter.Clover.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public MainPageViewModel()
        {
            Model = MainPageModel.Instance;
        }

        public MainPageModel Model { get; set; }
    }
}
