using DG.Tweening;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 面板基类 
/// 帮助我门通过代码快速的找到所有的子控件
/// 方便我们在子类中处理逻辑 
/// 节约找控件的工作量
/// </summary>
public class BasePanel
{

    public CanvasGroup CanvasGroup;
    public Camera camera;

    public Transform UIRoot => gameObject.transform.parent;
    public GameObject gameObject;
    public PanelUI panelUI;

    public bool visible = false;
    public bool ifStart = false;

    [HideInInspector] public BasePanel parentPanel;
    [HideInInspector] public Dictionary<string, BasePanel> childrenPanels = new Dictionary<string, BasePanel>();

    protected UnityAction hidecallback; //委托or事件 的装载


    public virtual void Awake () {

    }

    public virtual void OnEnable() { }

    public virtual void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {

    }
    public virtual void OnDisable() { }
    /// <summary>
    /// 主要用于初始化按钮监听等等的内容
    /// </summary>
    public virtual void Init(PanelUI ui) { }


    public virtual void OnDestory() { }

    public virtual bool IsOutHide()
    {
        return true;
    }
    public virtual void OnOutHide() { }
    /// <summary>
    /// 展示自己
    /// </summary>
    public virtual void ShowMe()
    {
        
    }

    /// <summary>
    /// 隐藏自己
    /// </summary>
    /// <param name="callBack">完全隐藏后执行函数</param>
    public virtual void HideMe(UnityAction callBack)
    {

        hidecallback = callBack;
        hidecallback?.Invoke();
    }
    /// <summary>
    /// 显示子面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callBack"></param>
    public void ShowPanel<T>(UnityAction<T> callBack = null) where T : BasePanel,new ()
    {
        UIManager.Instance.ShowChilderPanel<T>(this, callBack);
    }
    /// <summary>
    /// 隐藏指定面板
    /// </summary>
    /// <param name="basePanel"></param>
    public void HidePanel(BasePanel basePanel = null)
    {
        if (basePanel == null) basePanel = this;
        UIManager.Instance.HidePanel(basePanel);
    }

  
    protected virtual void OnClick(string btnName)
    {

    }

    protected virtual void OnToggleValueChanged(string toggleName, bool value)
    {

    }

    protected virtual void OnInputFieldValueChanged(string inputFieldName, string value)
    {

    }

    protected virtual void OnDropdownValueChanged(string dropdownName, int value)
    {

    }

}
