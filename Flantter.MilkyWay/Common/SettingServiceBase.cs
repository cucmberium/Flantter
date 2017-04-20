using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;
using Windows.Storage;

namespace Flantter.MilkyWay.Common
{
    public class SettingServiceBase<TImpl> : INotifyPropertyChanged
        where TImpl : class, new()
    {
        private static TImpl _instance;

        protected SettingServiceBase()
        {
            Cache = new Dictionary<string, object>();
        }

        public Dictionary<string, object> Cache { get; set; }
        public static TImpl Setting => _instance ?? (_instance = new TImpl());

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

        private T GetLocalValue<T>(string name, T defaultValue)
        {
            try
            {
                var values = ApplicationData.Current.LocalSettings.Values;
                if (values.ContainsKey(name))
                {
                    Cache[name] = (T) values[name];
                    return (T) Cache[name];
                }

                Cache[name] = defaultValue;
                return defaultValue;
            }
            catch
            {
                Cache[name] = defaultValue;
                return defaultValue;
            }
        }

        private void SetLocalValue<T>(string name, T value)
        {
            try
            {
                var settings = ApplicationData.Current.LocalSettings;
                if (settings.Values.ContainsKey(name))
                    settings.Values[name] = value;
                else
                    settings.Values.Add(name, value);
            }
            catch
            {
            }
        }

        private T GetRoamingValue<T>(string name, T defaultValue)
        {
            try
            {
                var values = ApplicationData.Current.RoamingSettings.Values;
                if (values.ContainsKey(name))
                {
                    Cache[name] = (T) values[name];
                    return (T) Cache[name];
                }
                Cache[name] = defaultValue;
                return defaultValue;
            }
            catch
            {
                Cache[name] = defaultValue;
                return defaultValue;
            }
        }

        private void SetRoamingValue<T>(string name, T value)
        {
            try
            {
                var settings = ApplicationData.Current.RoamingSettings;
                if (settings.Values.ContainsKey(name))
                    settings.Values[name] = value;
                else
                    settings.Values.Add(name, value);
            }
            catch
            {
            }
        }

        private T GetValueProxy<T>(string name, T defaultValue)
        {
            if (Cache.ContainsKey(name))
                return (T) Cache[name];

            var prop = GetType().GetRuntimeProperty(name);
            var attr = prop.GetCustomAttribute(typeof(LocalValueAttribute));
            return attr != null ? GetLocalValue(name, defaultValue) : GetRoamingValue(name, defaultValue);
        }

        private void SetValueProxy<T>(string name, T value)
        {
            Cache[name] = value;

            var prop = GetType().GetRuntimeProperty(name);
            var attr = prop.GetCustomAttribute(typeof(LocalValueAttribute));
            if (attr != null) SetLocalValue(name, value);
            else SetRoamingValue(name, value);
        }

        protected T GetValue<T>([CallerMemberName] string name = null)
        {
            return GetValueProxy(name, default(T));
        }

        protected T GetValue<T>(T defaultValue, [CallerMemberName] string name = null)
        {
            return GetValueProxy(name, defaultValue);
        }

        protected void SetValue<T>(T value, [CallerMemberName] string name = null)
        {
            SetValueProxy(name, value);
        }
    }

    public class LocalValueAttribute : Attribute
    {
    }
}