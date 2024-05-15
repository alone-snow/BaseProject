using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBoxList : IList
{
    public delegate void IBoxListChanged(Operation op, int itemIndex, object oldItem, object newItem);
    public event IBoxListChanged Callback;
    public enum Operation : byte
    {
        OP_ADD,
        OP_CLEAR,
        OP_INSERT,
        OP_REMOVEAT,
        OP_SET
    }

    public void AddAction(IBoxListChanged callback);
    public void RemoveAction(IBoxListChanged callback);
}
