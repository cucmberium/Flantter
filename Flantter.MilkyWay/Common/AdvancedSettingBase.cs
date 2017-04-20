using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;

namespace Flantter.MilkyWay.Common
{
    public class AdvancedSettingServiceBase<TImpl> : INotifyPropertyChanged
        where TImpl : class, new()
    {
        protected AdvancedSettingServiceBase()
        {
            Dict = new Dictionary<string, object>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            try
            {
                if (!CoreApplication.MainView.Dispatcher.HasThreadAccess)
                    return;
            }
            catch
            {
                return;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private static TImpl _instance;
        public static TImpl AdvancedSetting => _instance ?? (_instance = new TImpl());

        protected T GetValue<T>([CallerMemberName] string name = null)
        {
            return GetValue(default(T), name);
        }

        protected T GetValue<T>(T defaultValue, [CallerMemberName] string name = null)
        {
            try
            {
                if (Dict.ContainsKey(name)) return (T) Dict[name];
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

        public Dictionary<string, object> Dict { get; set; }
    }
}
