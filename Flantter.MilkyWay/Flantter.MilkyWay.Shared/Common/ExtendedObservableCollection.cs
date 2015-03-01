using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Flantter.MilkyWay.Common
{
    public class ExtendedObservableCollection<T> : IList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private object _SyncRoot = new object();
        private object _Lock = new object();
        private Collection<T> _Collection;

        public bool DisableNotifyPropertyChanged { get; set; }
        public bool DisableNotifyCollectionChanged { get; set; }

        public ExtendedObservableCollection()
        {
            _Collection = new Collection<T>();
            DisableNotifyPropertyChanged = false;
            DisableNotifyCollectionChanged = false;
        }
        public ExtendedObservableCollection(IEnumerable<T> collection)
        {
            _Collection = new Collection<T>(collection.ToList());
            DisableNotifyPropertyChanged = false;
            DisableNotifyCollectionChanged = false;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public void FireCollectionChangedReset()
        {
            if (CollectionChanged == null)
                return;

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            else
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset))).AsTask().Wait();
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            if (DisableNotifyPropertyChanged)
                return;

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            else
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PropertyChanged(this, new PropertyChangedEventArgs(propertyName))).AsTask().Wait();
        }

        private void NotifyCollectionChanged(NotifyCollectionChangedAction action)
        {
            if (CollectionChanged == null)
                return;

            if (DisableNotifyCollectionChanged)
                return;

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action));
            else
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => CollectionChanged(this, new NotifyCollectionChangedEventArgs(action))).AsTask().Wait();
        }
        private void NotifyCollectionChanged(NotifyCollectionChangedAction action, object item)
        {
            if (CollectionChanged == null)
                return;

            if (DisableNotifyCollectionChanged)
                return;

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item));
            else
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item))).AsTask().Wait();
        }
        private void NotifyCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            if (CollectionChanged == null)
                return;

            if (DisableNotifyCollectionChanged)
                return;

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
            else
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index))).AsTask().Wait();
        }

        public int IndexOf(T item)
        {
            lock (_Lock)
                return _Collection.IndexOf(item);
        }
        public void Insert(int index, T item)
        {
            lock (_Lock)
                _Collection.Insert(index, item);

            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }
        public void RemoveAt(int index)
        {
            T item;
            lock (_Lock)
            {
                item = _Collection[index];
                _Collection.RemoveAt(index);
            }
            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }
        public T this[int index]
        {
            get
            {
                lock (_Lock)
                    return _Collection[index];
            }
            set
            {
                lock (_Lock)
                    _Collection[index] = value;

                NotifyPropertyChanged("Item[]");
                NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, value, index);
            }
        }
        public void Add(T item)
        {
            int count;
            lock (_Lock)
            {
                _Collection.Add(item);
                count = _Collection.Count;
            }

            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, count - 1);
        }
        public void Clear()
        {
            lock (_Lock)
                _Collection.Clear();
            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Reset);
        }
        public bool Contains(T item)
        {
            lock (_Lock)
                return _Collection.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_Lock)
                _Collection.CopyTo(array, arrayIndex);
        }
        public int Count
        {
            get 
            {
                lock (_Lock)
                    return _Collection.Count; 
            }
        }
        public bool Remove(T item)
        {
            int index = -1;
            bool response = false;

            lock (_Lock)
            {
                index = _Collection.IndexOf(item);

                if (index == -1)
                    return false;

                response = _Collection.Remove(item);
            }

            if (!response)
                return false;

            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);

            return true;
        }
        public IEnumerator<T> GetEnumerator()
        {
            lock (_Lock)
                return _Collection.GetEnumerator();
        }
        public bool IsSynchronized
        {
            get { return true; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_Lock)
                return _Collection.GetEnumerator();
        }

        public int Add(object value)
        {
            int count;
            lock (_Lock)
            {
                _Collection.Add((T)value);
                count = _Collection.Count;
            }

            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, value, count - 1);
            return count - 1;
        }

        public bool Contains(object value)
        {
            lock (_Lock)
                return _Collection.Contains((T)value);
        }

        public int IndexOf(object value)
        {
            lock (_Lock)
                return _Collection.IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            lock (_Lock)
                _Collection.Insert(index, (T)value);

            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            lock (_Lock)
                _Collection.Remove((T)value);

            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
        }

        object IList.this[int index]
        {
            get
            {
                lock (_Lock)
                    return _Collection[index];
            }
            set
            {
                lock (_Lock)
                    _Collection[index] = (T)value;

                NotifyPropertyChanged("Item[]");
                NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, value, index);
            }
        }

        public void CopyTo(Array array, int index)
        {
            lock (_Lock)
                _Collection.CopyTo((T[])array, index);
        }

        public object SyncRoot
        {
            get { return _SyncRoot; }
        }
    }
}
