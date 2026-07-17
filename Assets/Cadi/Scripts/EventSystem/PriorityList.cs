using System;
using System.Collections;
using System.Collections.Generic;

namespace Cadi.Scripts.EventSystem
{
    public class PriorityList<T> : IEnumerable<T>
    {
        private static readonly EqualityComparer<T> s_Comparer = EqualityComparer<T>.Default;

        private readonly struct Entry
        {
            public readonly T Item;
            public readonly int Priority;

            public Entry(T item, int priority)
            {
                Item = item;
                Priority = priority;
            }
        }

        private Entry[] m_Entries;
        private int m_Count;
        private int m_Version;

        private const int c_DefaultCapacity = 4;

        public int Count => m_Count;
        public int Capacity => m_Entries.Length;

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)m_Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return m_Entries[index].Item;
            }
        }

        public PriorityList()
        {
            m_Entries = Array.Empty<Entry>();
        }

        public PriorityList(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            m_Entries = capacity > 0 ? new Entry[capacity] : Array.Empty<Entry>();
        }

        public void Add(T item, int priority)
        {
            if (m_Count == m_Entries.Length)
                Grow();

            int insertAt = m_Count;

            for (int i = 0; i < m_Count; i++)
            {
                if (priority < m_Entries[i].Priority)
                {
                    insertAt = i;
                    break;
                }
            }

            if (insertAt < m_Count)
                Array.Copy(m_Entries, insertAt, m_Entries, insertAt + 1, m_Count - insertAt);

            m_Entries[insertAt] = new Entry(item, priority);
            m_Count++;
            m_Version++;
        }

        public bool AddUnique(T item, int priority)
        {
            if (Contains(item))
                return false;

            Add(item, priority);
            return true;
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < m_Count; i++)
            {
                if (s_Comparer.Equals(m_Entries[i].Item, item))
                    return true;
            }

            return false;
        }

        public bool Remove(T item)
        {
            for (int i = 0; i < m_Count; i++)
            {
                if (s_Comparer.Equals(m_Entries[i].Item, item))
                {
                    RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            if (m_Count <= 0)
                return;

            Array.Clear(m_Entries, 0, m_Count);
            m_Count = 0;
            m_Version++;
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            if (m_Entries.Length >= capacity)
                return;

            var newEntries = new Entry[capacity];

            if (m_Count > 0)
                Array.Copy(m_Entries, newEntries, m_Count);

            m_Entries = newEntries;
            m_Version++;
        }

        private void RemoveAt(int index)
        {
            m_Count--;

            if (index < m_Count)
                Array.Copy(m_Entries, index + 1, m_Entries, index, m_Count - index);

            m_Entries[m_Count] = default;
            m_Version++;
        }

        private void Grow()
        {
            int newCapacity = m_Entries.Length == 0 ? c_DefaultCapacity : m_Entries.Length * 2;
            EnsureCapacity(newCapacity);
        }

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        public struct Enumerator : IEnumerator<T>
        {
            private readonly PriorityList<T> m_List;
            private readonly Entry[] m_Entries;
            private readonly int m_Count;
            private readonly int m_Version;
            private int m_Index;

            internal Enumerator(PriorityList<T> list)
            {
                m_List = list;
                m_Entries = list.m_Entries;
                m_Count = list.m_Count;
                m_Version = list.m_Version;
                m_Index = -1;
            }

            public T Current
            {
                get
                {
                    if ((uint)m_Index >= (uint)m_Count)
                        throw new InvalidOperationException("Enumerator is not positioned on a valid element.");

                    return m_Entries[m_Index].Item;
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                if (m_Version != m_List.m_Version)
                    throw new InvalidOperationException("Collection was modified during enumeration.");

                m_Index++;
                return m_Index < m_Count;
            }

            public void Reset()
            {
                if (m_Version != m_List.m_Version)
                    throw new InvalidOperationException("Collection was modified during enumeration.");

                m_Index = -1;
            }

            public void Dispose()
            {
            }
        }
    }
}