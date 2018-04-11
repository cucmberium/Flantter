using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;

namespace Flantter.MilkyWay.Common
{
    public class AdvancedSettingServiceBase<TImpl> : INotifyPropertyChanged
        where TImpl : class, new()
    {
        private static TImpl _instance;

        protected AdvancedSettingServiceBase()
        {
            Dict = new ConcurrentDictionary<string, object>();
        }

        public static TImpl AdvancedSetting => _instance ?? (_instance = new TImpl());

        public ConcurrentDictionary<string, object> Dict { get; set; }

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
                Dict[name] = value;
            }
            catch
            {
            }
        }
    }
}