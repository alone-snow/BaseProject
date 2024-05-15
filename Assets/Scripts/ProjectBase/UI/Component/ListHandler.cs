using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public delegate object GetObjectaction(out object obj);

public class ListHandler : UIBehaviour, IList
{
    [Tooltip("放元素控件的 Transform")]
    public Transform content;
    [Tooltip("添加按钮")]
    public Button btnAdd;
    [Tooltip("移除按钮")]
    public Button btnRemove;

    public CurrencyButton currencyButton;

    private UnityEvent<object> m_OnAddItem = new UnityEvent<object>();
    public UnityEvent<object> OnAddItem => m_OnAddItem;

    private UnityEvent<object> m_OnRemoveItem = new UnityEvent<object>();
    public UnityEvent<object> OnRemoveItem => m_OnRemoveItem;

    private UnityEvent<int> m_OnClikButton = new UnityEvent<int>();
    public UnityEvent<int> OnClikButton => m_OnClikButton;

    private UnityAction<CurrencyButton, int> creatButton;
    public GetObjectaction getObjectaction;

    private int selectIndex = -1;
    public List<CurrencyButton> currencyList = new List<CurrencyButton>();
    private IBoxList list;
    private Type objType;
    public BasePanel basePanel;
    public int Count => list.Count;

    public bool IsFixedSize => false;

    public bool IsReadOnly => false;

    public bool IsSynchronized => false;

    public object SyncRoot => null;

    public object this[int index] { get => list[index]; set => Set(value, index); }

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        btnAdd?.onClick.AddListener(AddItem);
        btnRemove?.onClick.AddListener(RemoveItem);
    }
    /// <summary>
    /// 初始化list托管器
    /// <para>list : 托管的list</para>
    /// <para>creatButton : 创建单个元素控件时触发，请调用 <see cref="CurrencyButton.Init(string, int, UnityAction{int})"/> 函数初始化控件，int 为该控件隶属于 list 中元素的索引</para>
    /// <para>getObjextaction : 添加元素时调用此委托 获取一个自定义新元素，为空时则会通过反射调用无参构造创建新元素</para>
    /// <para>currencyButton : 单个元素控件，可以继承自己扩展然后传入</para>
    /// <para>例子：</para>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public ListHandler lhPos
    /// List<Vector3> v3 = new List<Vector3>()
    /// 
    /// lhPos.InitList( v3,
    ///             (c, i) => { 
    ///                         c.InitList( v3[i].ToString(), i, (index) => { Debug.Log(v3[index]; )}); 
    ///                        },
    ///             (out object v3) => {
    ///                                 v3 = new Vector3(1,2,3); 
    ///                                 return v3; 
    ///                                 },
    ///             null);
    /// ]]>
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="creatButton"></param>
    /// <param name="getObjectaction"></param>
    /// <param name="currencyButton"></param>
    public BoxList<T> InitList<T>(IList<T> list ,UnityAction<CurrencyButton,int> creatButton, GetObjectaction getObjectaction = null, CurrencyButton currencyButton = null) 
    {
        Unload();
        selectIndex = -1;
        BoxList<T> box = new BoxList<T>(list);
        Load(box);
        objType = typeof(T);
        this.getObjectaction = getObjectaction;
        this.creatButton = creatButton;
        if (currencyButton != null) this.currencyButton = currencyButton;
        ReBuild();
        return box;
    }

    public void InitBox<T>(BoxList<T> list, UnityAction<CurrencyButton, int> creatButton, GetObjectaction getObjectaction = null, CurrencyButton currencyButton = null)
    {
        Unload();
        selectIndex = -1;
        Load(list);
        objType = typeof(T);
        this.getObjectaction = getObjectaction;
        this.creatButton = creatButton;
        if (currencyButton != null) this.currencyButton = currencyButton;
        ReBuild();
    }

    public T GetCurrencyButton<T>(int index) where T : CurrencyButton
    {
        return currencyList[index] as T;
    }

    protected override void OnDisable()
    {
        Unload();
    }

    private void Load(IBoxList list)
    {
        this.list = list;
        list.AddAction(BoxItemChange);
    }

    private void Unload()
    {
        if (list != null)
        {
            list.RemoveAction(BoxItemChange);
        }
    }

    private void BoxItemChange(IBoxList.Operation op,int index,object oldItem,object newItem)
    {
        switch (op)
        {
            case IBoxList.Operation.OP_ADD:
                PoolMgr.Instance.GetObj("ListHander_" + currencyButton.name, currencyButton.gameObject, (o) =>
                {
                    CurrencyButton cb = o.GetComponent<CurrencyButton>();
                    cb.basePanel = basePanel;
                    cb.OnClikButton.AddListener(ChoiceButton);
                    o.transform.SetParent(content, false);
                    currencyList.Add(cb);
                    creatButton?.Invoke(cb, index);
                });
                m_OnAddItem?.Invoke(newItem);
                break;
            case IBoxList.Operation.OP_REMOVEAT:
                PoolMgr.Instance.PushObj(currencyList[index].gameObject.name, currencyList[index].gameObject);
                currencyList.RemoveAt(index);
                for (int i = index; i < currencyList.Count; i++)
                {
                    currencyList[i].index = i;
                }
                if (selectIndex >= list.Count) selectIndex = list.Count - 1;
                m_OnRemoveItem?.Invoke(oldItem);
                break;
            case IBoxList.Operation.OP_CLEAR:
                if (currencyList.Count != 0)
                {
                    foreach (var item in currencyList)
                    {
                        PoolMgr.Instance.PushObj(item.gameObject.name, item.gameObject);
                    }
                    currencyList.Clear();
                }
                break;
            case IBoxList.Operation.OP_INSERT:
                PoolMgr.Instance.GetObj("ListHander_" + currencyButton.name, currencyButton.gameObject, (o) =>
                {
                    CurrencyButton cb = o.GetComponent<CurrencyButton>();
                    cb.basePanel = basePanel;
                    cb.OnClikButton.AddListener(ChoiceButton);
                    o.transform.SetParent(content, false);
                    o.transform.SetSiblingIndex(index);
                    currencyList.Add(cb);
                    creatButton?.Invoke(cb, index);
                });
                m_OnAddItem?.Invoke(newItem);
                break;
            case IBoxList.Operation.OP_SET:
                creatButton?.Invoke(currencyList[index], index);
                break;
        }
    }

    public void ReBuild()
    {
        if (currencyList.Count != 0)
        {
            foreach (var item in currencyList)
            {
                PoolMgr.Instance.PushObj(item.gameObject.name, item.gameObject);
            }
            currencyList.Clear();
        }

        for (int i = 0; i < list.Count; i++)
        {
            PoolMgr.Instance.GetObj("ListHander_" + currencyButton.name, currencyButton.gameObject, (o) =>
            {
                CurrencyButton cb = o.GetComponent<CurrencyButton>();
                cb.basePanel = basePanel;
                creatButton?.Invoke(cb, i);
                cb.OnClikButton.AddListener(ChoiceButton);
                o.transform.SetParent(content, false);
                currencyList.Add(cb);
            });
        }
    }

    public void ChoiceButton(int index)
    {
        selectIndex = index;
        m_OnClikButton?.Invoke(index);
    }

    public void AddItem<T>(T obj)
    {
        list.Add(obj);
    }

    public void AddItem()
    {
        object obj = getObjectaction == null ? Activator.CreateInstance(objType) : getObjectaction?.Invoke(out obj);
        AddItem(obj);
    }

    public void RemoveItem<T>(T obj)
    {
        int index = list.IndexOf(obj);
        list.RemoveAt(index);
    }

    public void RemoveItem()
    {
        if (list == null || list.Count == 0) return;
        object obj = list[list.Count - 1];
        if(selectIndex == -1)
        {
            list.RemoveAt(list.Count - 1);
        }
        else
        {
            list.RemoveAt(selectIndex);
        }
    }

    public void Clear()
    {
        list.Clear();
    }

    public CurrencyButton GetCurrencyButton(int index)
    {
        return currencyList[index];
    }

    public void Set(object obj, int index)
    {
        if (index >= Count)
        {
            return;
        }
        list[index] = obj;
    }

    public int Add(object value)
    {
        list.Add(value);
        return Count - 1;
    }

    public bool Contains(object value)
    {
        return IndexOf(value) >= 0;
    }

    public int IndexOf(object value)
    {
        for (int i = 0; i < list.Count; ++i)
            if (value == list[i])
                return i;
        return -1;
    }

    public void Insert(int index, object value)
    {
        list.Insert(index, value);
    }

    public void Remove(object value)
    {
        int index = list.IndexOf(value);
        RemoveAt(index);
    }

    public void RemoveAt(int index)
    {
        if (index == -1 || index > list.Count) return;
        list.RemoveAt(index);
    }

    public void CopyTo(Array array, int index)
    {
        list.CopyTo(array, index);
    }

    public Enumerator GetEnumerator() => new Enumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

    public struct Enumerator : IEnumerator
    {
        readonly ListHandler list;
        int index;
        public object Current { get; private set; }

        public Enumerator(ListHandler list)
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
