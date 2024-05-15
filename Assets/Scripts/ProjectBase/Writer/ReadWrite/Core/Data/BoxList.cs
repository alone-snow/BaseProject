using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static IBoxList;


public class BoxList<T> : IList<T>, IReadOnlyList<T>,IBoxList
{
    public delegate void BoxListChanged(Operation op, int itemIndex, T oldItem, T newItem);

    readonly IList<T> objects;
    readonly IEqualityComparer<T> comparer;

    public int Count => objects.Count;
    public bool IsReadOnly => false;

    public bool IsFixedSize => false;

    public bool IsSynchronized => false;

    public object SyncRoot => null;

    object IList.this[int index] { get => this[index]; set => this[index] = (T)value; }

    public event BoxListChanged Callback;
    public event IBoxListChanged ICallback;

    public BoxList() : this(EqualityComparer<T>.Default) { }

    public BoxList(IEqualityComparer<T> comparer)
    {
        this.comparer = comparer ?? EqualityComparer<T>.Default;
        objects = new List<T>();
    }

    public BoxList(IList<T> objects, IEqualityComparer<T> comparer = null)
    {
        this.comparer = comparer ?? EqualityComparer<T>.Default;
        this.objects = objects;
    }

    public List<T> ToList => new List<T>(objects);

    event IBoxListChanged IBoxList.Callback
    {
        add
        {
            ICallback += value;
        }

        remove
        {
            ICallback -= value;
        }
    }

    public void AddAction(IBoxListChanged callback)
    {
        ICallback += callback;
    }

    public void RemoveAction(IBoxListChanged callback)
    {
        ICallback -= callback;
    }

    public void Reset()
    {
        objects.Clear();
    }

    void AddOperation(Operation op, int itemIndex, T oldItem, T newItem)
    {
        Callback?.Invoke(op, itemIndex, oldItem, newItem);
        ICallback?.Invoke(op,itemIndex, oldItem, newItem);
    }

    public void Add(T item)
    {
        objects.Add(item);
        AddOperation(Operation.OP_ADD, objects.Count - 1, default, item);
    }

    public void AddRange(IEnumerable<T> range)
    {
        foreach (T entry in range)
        {
            Add(entry);
        }
    }

    public void Clear()
    {
        objects.Clear();
        AddOperation(Operation.OP_CLEAR, 0, default, default);
    }

    public bool Contains(T item) => IndexOf(item) >= 0;

    public void CopyTo(T[] array, int index) => objects.CopyTo(array, index);

    public int IndexOf(T item)
    {
        for (int i = 0; i < objects.Count; ++i)
            if (comparer.Equals(item, objects[i]))
                return i;
        return -1;
    }

    public int FindIndex(Predicate<T> match)
    {
        for (int i = 0; i < objects.Count; ++i)
            if (match(objects[i]))
                return i;
        return -1;
    }

    public T Find(Predicate<T> match)
    {
        int i = FindIndex(match);
        return (i != -1) ? objects[i] : default;
    }

    public List<T> FindAll(Predicate<T> match)
    {
        List<T> results = new List<T>();
        for (int i = 0; i < objects.Count; ++i)
            if (match(objects[i]))
                results.Add(objects[i]);
        return results;
    }

    public void Insert(int index, T item)
    {
        objects.Insert(index, item);
        AddOperation(Operation.OP_INSERT, index, default, item);
    }

    public void InsertRange(int index, IEnumerable<T> range)
    {
        foreach (T entry in range)
        {
            Insert(index, entry);
            index++;
        }
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        bool result = index >= 0;
        if (result)
        {
            RemoveAt(index);
        }
        return result;
    }

    public void RemoveAt(int index)
    {
        T oldItem = objects[index];
        objects.RemoveAt(index);
        AddOperation(Operation.OP_REMOVEAT, index, oldItem, default);
    }

    public int RemoveAll(Predicate<T> match)
    {
        List<T> toRemove = new List<T>();
        for (int i = 0; i < objects.Count; ++i)
            if (match(objects[i]))
                toRemove.Add(objects[i]);

        foreach (T entry in toRemove)
        {
            Remove(entry);
        }

        return toRemove.Count;
    }

    public T this[int i]
    {
        get => objects[i];
        set
        {
            if (!comparer.Equals(objects[i], value))
            {
                T oldItem = objects[i];
                objects[i] = value;
                AddOperation(Operation.OP_SET, i, oldItem, value);
            }
        }
    }

    public Enumerator GetEnumerator() => new Enumerator(this);

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

    public int Add(object value)
    {
        Add((T)value);
        return Count - 1;
    }

    public bool Contains(object value)
    {
        return Contains((T)value);
    }

    public int IndexOf(object value)
    {
        return IndexOf((T)value);
    }

    public void Insert(int index, object value)
    {
        Insert(index, (T)value);
    }

    public void Remove(object value)
    {
        Remove((T)value);
    }

    public void CopyTo(Array array, int index)
    {
        var resurt = new T[Count];
        for (int i = 0; i < Count; i++)
        {
            resurt[i] = this[i];
        }
        array = resurt;
    }



    public struct Enumerator : IEnumerator<T>
    {
        readonly BoxList<T> list;
        int index;
        public T Current { get; private set; }

        public Enumerator(BoxList<T> list)
        {
            this.list = list;
            index = -1;
            Current = default;
        }

        public bool MoveNext()
        {
            if (++index >= list.Count)
            {
                return false;
            }
            Current = list[index];
            return true;
        }

        public void Reset() => index = -1;
        object IEnumerator.Current => Current;
        public void Dispose() { }
    }
}