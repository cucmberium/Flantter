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

            CollectionChanged(this, new NotifyCollectionChangedEventArgs(action));
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
            return _Collection.IndexOf(item);
        }
        public void Insert(int index, T item)
        {
            _Collection.Insert(index, item);

            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }
        public void RemoveAt(int index)
        {
            T item;

            item = _Collection[index];
            _Collection.RemoveAt(index);

            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }
        public T this[int index]
        {
            get
            {
                return _Collection[index];
            }
            set
            {
                _Collection[index] = value;

                NotifyPropertyChanged("Item[]");
                NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, value, index);
            }
        }
        public void Add(T item)
        {
            int count;
            _Collection.Add(item);
            count = _Collection.Count;

            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, count - 1);
        }
        public void Clear()
        {
            _Collection.Clear();
            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Reset);
        }
        public bool Contains(T item)
        {
            return _Collection.Contains(item);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            _Collection.CopyTo(array, arrayIndex);
        }
        public int Count
        {
            get 
            {
                return _Collection.Count; 
            }
        }
        public bool Remove(T item)
        {
            int index = -1;
            bool response = false;

            index = _Collection.IndexOf(item);

            if (index == -1)
                return false;

            response = _Collection.Remove(item);

            if (!response)
                return false;

            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);

            return true;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _Collection.GetEnumerator();
        }
        public bool IsSynchronized
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Collection.GetEnumerator();
        }

        public int Add(object value)
        {
            int count;
            _Collection.Add((T)value);
            count = _Collection.Count;

            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, value, count - 1);
            return count - 1;
        }

        public bool Contains(object value)
        {
            return _Collection.Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return _Collection.IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
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
            _Collection.Remove((T)value);

            NotifyPropertyChanged("Count");
            NotifyPropertyChanged("Item[]");
        }

        object IList.this[int index]
        {
            get
            {
                return _Collection[index];
            }
            set
            {
                _Collection[index] = (T)value;

                NotifyPropertyChanged("Item[]");
                NotifyCollectionChanged(NotifyCollectionChangedAction.Replace, value, index);
            }
        }

        public void CopyTo(Array array, int index)
        {
            _Collection.CopyTo((T[])array, index);
        }

        public object SyncRoot
        {
            get { return _SyncRoot; }
        }
    }
}
