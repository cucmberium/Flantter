using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Windows.Storage;

namespace Flantter.MilkyWay.Common
{
    public class FontServiceBase<Impl> : INotifyPropertyChanged
        where Impl : class, new()
    {
        public Dictionary<string, object> Dict { get; set; }

        protected FontServiceBase()
        {
            Dict = new Dictionary<string, object>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            var h = PropertyChanged;
            if (h != null) h(this, new PropertyChangedEventArgs(name));
        }

        private static Impl _instance;
        public static Impl Font { get { return _instance ?? (_instance = new Impl()); } }

        protected T GetValue<T>([CallerMemberName] string name = null)
        {
            return GetValue<T>(default(T), name);
        }

        protected T GetValue<T>(T defaultValue, [CallerMemberName] string name = null)
        {
            try
            {
                if (Dict.ContainsKey(name)) return (T)Dict[name];
                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        protected void SetValue<T>(T value, [CallerMemberName] string name = null)
        {
            try
            {
                if (Dict.ContainsKey(name))
                    Dict[name] = value;
                else
                    Dict.Add(name, value);
            }
            catch
            {
            }
        }
    }
}
