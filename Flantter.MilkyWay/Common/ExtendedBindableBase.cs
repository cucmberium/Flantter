using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Data;

namespace Flantter.MilkyWay.Common
{
    /// <summary>
    /// モデルを簡略化するための <see cref="INotifyPropertyChanged"/> の実装。
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class ExtendedBindableBase : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                if (CoreApplication.MainView.Dispatcher.HasThreadAccess)
                    eventHandler(this, new PropertyChangedEventArgs(propertyName));
                else
                    CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => eventHandler(this, new PropertyChangedEventArgs(propertyName))).AsTask().Wait();
            }
        }
    }
}
