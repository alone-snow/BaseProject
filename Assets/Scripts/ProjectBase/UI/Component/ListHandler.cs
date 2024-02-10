using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public delegate object GetObjectaction(out object obj);

public class ListHandler : UIBehaviour
{
    [Tooltip("��Ԫ�ؿؼ��� Transform")]
    public Transform content;
    [Tooltip("��Ӱ�ť")]
    public Button btnAdd;
    [Tooltip("�Ƴ���ť")]
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
    private List<CurrencyButton> currencyList = new List<CurrencyButton>();
    private IList list;
    private Type objType;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        btnAdd?.onClick.AddListener(AddItem);
        btnRemove?.onClick.AddListener(RemoveItem);
    }
    /// <summary>
    /// ��ʼ��list�й���
    /// <para>list : �йܵ�list</para>
    /// <para>creatButton : ��������Ԫ�ؿؼ�ʱ����������� <see cref="CurrencyButton.Init(string, int, UnityAction{int})"/> ������ʼ���ؼ���int Ϊ�ÿؼ������� list ��Ԫ�ص�����</para>
    /// <para>getObjextaction : ���Ԫ��ʱ���ô�ί�� ��ȡһ���Զ�����Ԫ�أ�Ϊ��ʱ���ͨ����������޲ι��촴����Ԫ��</para>
    /// <para>currencyButton : ����Ԫ�ؿؼ������Լ̳��Լ���չȻ����</para>
    /// <para>���ӣ�</para>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// public ListHandler lhPos
    /// List<Vector3> v3 = new List<Vector3>()
    /// 
    /// lhPos.Init( v3,
    ///             (c, i) => { 
    ///                         c.Init( v3[i].ToString(), i, (index) => { Debug.Log(v3[index]; )}); 
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
    public void Init<T>(List<T> list ,UnityAction<CurrencyButton,int> creatButton, GetObjectaction getObjectaction = null, CurrencyButton currencyButton = null) 
    {
        selectIndex = -1;
        this.list = list;
        objType = typeof(T);
        this.getObjectaction = getObjectaction;
        this.creatButton = creatButton;
        if (currencyButton != null) this.currencyButton = currencyButton;
        currencyButton = this.currencyButton;
        if(currencyList.Count > 0)
        {
            foreach (var item in currencyList)
            {
                PoolMgr.Instance.PushObj(item.gameObject.name, item.gameObject);
            }
            currencyList.Clear();
        }
        
        for(int i = 0; i < list.Count; i++)
        {
            PoolMgr.Instance.GetObj("ListHander_" + currencyButton.name, currencyButton.gameObject, (o) =>
            {
                CurrencyButton cb = o.GetComponent<CurrencyButton>();
                creatButton?.Invoke(cb, i);
                cb.OnClikButton.AddListener(ChoiceButton);              
                o.transform.SetParent(content,false);
                currencyList.Add(cb);
            });
        }
    }

    public void ChoiceButton(int index)
    {
        selectIndex = index;
        m_OnClikButton?.Invoke(index);
    }

    public void AddItem()
    {
        object obj = getObjectaction == null ? Activator.CreateInstance(objType) : getObjectaction?.Invoke(out obj);
        list.Add(obj);
        PoolMgr.Instance.GetObj("ListHander_" + currencyButton.name, currencyButton.gameObject, (o) =>
        {
            CurrencyButton cb = o.GetComponent<CurrencyButton>();
            creatButton?.Invoke(cb, list.Count - 1);
            cb.OnClikButton.AddListener(ChoiceButton);
            o.transform.SetParent(content, false);
            currencyList.Add(cb);
        });
        m_OnAddItem?.Invoke(obj);
    }

    public void RemoveItem()
    {
        if (list == null || list.Count == 0) return;
        object obj = list[list.Count - 1];
        if(selectIndex == -1)
        {
            list.RemoveAt(list.Count - 1);
            Destroy(currencyList[currencyList.Count - 1].gameObject);
            currencyList.RemoveAt(currencyList.Count - 1);
        }
        else
        {
            list.RemoveAt(selectIndex);
            Destroy(currencyList[selectIndex].gameObject);
            currencyList.RemoveAt(selectIndex);
            for(int i = selectIndex; i < currencyList.Count; i++)
            {
                currencyList[i].index = i;
            }
        }
        selectIndex = -1;
        m_OnRemoveItem?.Invoke(obj);
    }

    public CurrencyButton GetCurrencyButton(int index)
    {
        return currencyList[index];
    }

}
