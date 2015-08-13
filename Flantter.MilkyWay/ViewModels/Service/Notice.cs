using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.ViewModels.Service
{
    public class Notice
    {
        private static Notice _Instance = new Notice();
        private Notice()
        {
            this.UserProfileShowCommand = new ReactiveCommand();
            this.MediaShowCommand = new ReactiveCommand();
            this.StatusDetailShowCommand = new ReactiveCommand();
        }

        public static Notice Instance
        {
            get { return _Instance; }
        }

        public ReactiveCommand UserProfileShowCommand { get; private set; }
        public ReactiveCommand MediaShowCommand { get; private set; }
        public ReactiveCommand StatusDetailShowCommand { get; private set; }
    }
}
