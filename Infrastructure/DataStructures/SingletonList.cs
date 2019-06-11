using System;
using System.Collections;
using System.Collections.Generic;

namespace WebApiADO.NetCrudPagination.Infrastructure.DataStructures
{
    public class SingletonList<T> : IList<T>
    {
        private readonly T _item;

        public SingletonList(T item)
        {
            _item = item;
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return _item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            throw new NotSupportedException("Add not supported.");
        }

        public void Clear()
        {
            throw new NotSupportedException("Clear not supported.");
        }

        public bool Contains(T item)
        {
            if (item == null) return _item == null;

            return item.Equals(_item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");

            array[arrayIndex] = _item;
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException("Remove not supported.");
        }

        public int Count => 1;

        public bool IsReadOnly => true;

        public int IndexOf(T item)
        {
            return Contains(item) ? 0 : -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException("Insert not supported.");
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException("RemoveAt not supported.");
        }

        public T this[int index]
        {
            get
            {
                if (index == 0) return _item;

                throw new IndexOutOfRangeException();
            }
            set => throw new NotSupportedException("Set not supported.");
        }
    }
}