using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CurrencyButton : MonoBehaviour
{
    [SerializeField] protected Button btn;
    [SerializeField] protected TextMeshProUGUI txt;
    public int index;
    protected UnityEvent<int> m_OnClikButton = new UnityEvent<int>();

    public UnityEvent<int> OnClikButton => m_OnClikButton;

    public UnityAction<int> OnClik;
    protected virtual void Awake()
    {
        btn?.onClick.AddListener(() =>
        {
            m_OnClikButton?.Invoke(index);
            OnClik?.Invoke(index);
        });
    }
    /// <summary>
    /// 初始化通用list单元素控件
    /// <para>name : 按钮显示的名字</para>
    /// <para>index : 元素在list中的索引</para>
    /// <para>callback : 点击后会触发，int元素的索引</para>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="index"></param>
    /// <param name="callback"></param>
    public virtual void Init(string name,int index,UnityAction<int> callback)
    {
        this.index = index;
        txt.text = name;
        OnClik = callback;
        
    }
}
