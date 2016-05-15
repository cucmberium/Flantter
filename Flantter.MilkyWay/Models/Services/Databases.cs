using Flantter.MilkyWay.Setting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flantter.MilkyWay.Models.Services
{
    public class Databases
    {
        private static Databases _Instance = new Databases();
        private Databases() { }

        public static Databases Instance
        {
            get { return _Instance; }
        }
        
        public void Initialize()
        {
        }

        public void Free()
        {
        }
    }
}
