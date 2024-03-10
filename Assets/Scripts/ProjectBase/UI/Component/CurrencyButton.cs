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
    /// ��ʼ��ͨ��list��Ԫ�ؿؼ�
    /// <para>name : ��ť��ʾ������</para>
    /// <para>index : Ԫ����list�е�����</para>
    /// <para>callback : �����ᴥ����intԪ�ص�����</para>
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
