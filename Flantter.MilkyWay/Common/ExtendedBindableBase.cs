using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.Core;

namespace Flantter.MilkyWay.Common
{
    [WebHostHidden]
    public abstract class ExtendedBindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            var eventHandler = PropertyChanged;
            if (eventHandler == null)
                return;
            if (CoreApplication.MainView.Dispatcher.HasThreadAccess)
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            else
                CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => eventHandler(this, new PropertyChangedEventArgs(propertyName)))
                    .AsTask()
                    .Wait();
        }
    }
}