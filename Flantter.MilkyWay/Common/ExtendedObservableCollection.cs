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

        public void InvokeCollectionChanged(NotifyCollectionChangedAction action)
        {
            if (CollectionChanged == null)
                return;

            if (action != NotifyCollectionChangedAction.Reset)
                throw new NotImplementedException();

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(action));
            else
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => CollectionChanged(this, new NotifyCollectionChangedEventArgs(action))).AsTask().Wait();
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null)
                return;

            if (DisableNotifyPropertyChanged)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyCollectionChanged(NotifyCollectionChangedAction action)
        {
            if (CollectionChanged == null)
                return;

            if (DisableNotifyCollectionChanged)
                return;

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(action));
        }
        private void NotifyCollectionChanged(NotifyCollectionChangedAction action, object item)
        {
            if (CollectionChanged == null)
                return;

            if (DisableNotifyCollectionChanged)
                return;

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item));
        }
        private void NotifyCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            if (CollectionChanged == null)
                return;

            if (DisableNotifyCollectionChanged)
                return;

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));
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

            
            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                NotifyPropertyChanged("Count");
                NotifyPropertyChanged("Item[]");
                NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
            }
            else
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NotifyPropertyChanged("Count");
                    NotifyPropertyChanged("Item[]");
                    NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
                }).AsTask().Wait();
            }
        }
        public void RemoveAt(int index)
        {
            T item;
            lock (_Lock)
            {
                item = _Collection[index];
                _Collection.RemoveAt(index);
            }

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                NotifyPropertyChanged("Count");
                NotifyPropertyChanged("Item[]");
                NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            }
            else
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NotifyPropertyChanged("Count");
                    NotifyPropertyChanged("Item[]");
                    NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
                }).AsTask().Wait();
            }
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

                if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                {
                    NotifyPropertyChanged("Item[]");
                    NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, value, index);
                }
                else
                {
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        NotifyPropertyChanged("Item[]");
                        NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, value, index);
                    }).AsTask().Wait();
                }
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

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                NotifyPropertyChanged("Count");
                NotifyPropertyChanged("Item[]");
                NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, count - 1);
            }
            else
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NotifyPropertyChanged("Count");
                    NotifyPropertyChanged("Item[]");
                    NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, count - 1);
                }).AsTask().Wait();
            }
        }
        public void Clear()
        {
            lock (_Lock)
                _Collection.Clear();

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                NotifyPropertyChanged("Count");
                NotifyPropertyChanged("Item[]");
                NotifyCollectionChanged(NotifyCollectionChangedAction.Reset);
            }
            else
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NotifyPropertyChanged("Count");
                    NotifyPropertyChanged("Item[]");
                    NotifyCollectionChanged(NotifyCollectionChangedAction.Reset);
                }).AsTask().Wait();
            }
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

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                NotifyPropertyChanged("Count");
                NotifyPropertyChanged("Item[]");
                NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            }
            else
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NotifyPropertyChanged("Count");
                    NotifyPropertyChanged("Item[]");
                    NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
                }).AsTask().Wait();
            }

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

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                NotifyPropertyChanged("Count");
                NotifyPropertyChanged("Item[]");
                NotifyCollectionChanged(NotifyCollectionChangedAction.Add, value, count - 1);
            }
            else
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NotifyPropertyChanged("Count");
                    NotifyPropertyChanged("Item[]");
                    NotifyCollectionChanged(NotifyCollectionChangedAction.Add, value, count - 1);
                }).AsTask().Wait();
            }

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

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                NotifyPropertyChanged("Count");
                NotifyPropertyChanged("Item[]");
                NotifyCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
            }
            else
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NotifyPropertyChanged("Count");
                    NotifyPropertyChanged("Item[]");
                    NotifyCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
                }).AsTask().Wait();
            }

        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            lock (_Lock)
                _Collection.Remove((T)value);

            if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
            {
                NotifyPropertyChanged("Count");
                NotifyPropertyChanged("Item[]");
            }
            else
            {
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NotifyPropertyChanged("Count");
                    NotifyPropertyChanged("Item[]");
                }).AsTask().Wait();
            }

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

                if (CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess)
                {
                    NotifyPropertyChanged("Item[]");
                    NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, value, index);
                }
                else
                {
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        NotifyPropertyChanged("Item[]");
                        NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, value, index);
                    }).AsTask().Wait();
                }

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
