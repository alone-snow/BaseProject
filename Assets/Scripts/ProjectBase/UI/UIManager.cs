using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public interface IShowCallBack 
{
    public void Invoke(BasePanel panel);
}
public class ShowCallBack<T>: IShowCallBack where T : BasePanel
{
    public UnityAction<T> callback;
    public ShowCallBack(UnityAction<T> callback)
    {
        this.callback = callback;
    }

    public void Invoke(BasePanel panel)
    {
        callback?.Invoke(panel as T);
    }
}
/// <summary>
/// UI管理器
/// 1.管理所有显示的面板
/// 2.提供给外部 显示和隐藏等等接口
/// </summary>
public class UIManager : BaseManager<UIManager>
{

    //储存面板的列表
    private Dictionary<string, BasePanel> panelDic = new Dictionary<string, BasePanel>();
    private List<BasePanel> panelList = new List<BasePanel>();
    private List<BasePanel> visiblePanel = new List<BasePanel>();
    //堆面板 栈面板
    private BasePanel queueActivePanel;
    private BasePanel stackActivePanel;
    private Stack<BasePanel> stackPanel = new Stack<BasePanel>();
    private Queue<(BasePanel, IShowCallBack)> queuePanel = new Queue<(BasePanel, IShowCallBack)>();
    //面板资源文件
    public List<PanelData> panelDatas = new List<PanelData>();
    //uiRoot
    private Transform uiRoot;
    private Camera uiCamera;
    //ui更新事件
    private UnityAction panleUpdata;

    private int hideCount = 0;
    public bool isOnHide => hideCount != 0;

    //获取对象
    public UIManager()
    {
        //获取面板资源文件
        panelDatas = ResMgr.Instance.Load<PanelList_SO>("PanelList_SO").panelDatas;
        //UIRoot 让其过场景的时候 不被移除
        GameObject obj = ResMgr.Instance.Load<GameObject>("UIRoot");
        uiRoot = obj.transform;
        uiCamera = obj.GetComponentInChildren<Camera>();
        GameObject.DontDestroyOnLoad(obj);
        //开始面板更新
        MonoMgr.Instance.AddUpdateListener(Updata);
    }
    //面板更新
    private void Updata()
    {
        panleUpdata?.Invoke();
        if (Input.GetKeyDown(KeyCode.Escape) && !isOnHide) 
        {
            if (queueActivePanel != null)
            {
                if (queueActivePanel.IsOutHide())
                {
                    HidePanel(queueActivePanel, queueActivePanel.OnOutHide);
                }
            }
            else if (stackActivePanel != null) 
            {
                if(stackPanel.Count != 0 && stackActivePanel.IsOutHide())
                {
                    HidePanel(stackActivePanel, stackActivePanel.OnOutHide);
                }
            }else if (visiblePanel.Count > 1)
            {
                BasePanel panel = visiblePanel[visiblePanel.Count - 1];
                if (panel.IsOutHide())
                {
                    HidePanel(panel, panel.OnOutHide);
                }
            }
        }
    }
    /// <summary>
    /// 预加载面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void PreparePanel<T>() where T : BasePanel, new()
    {
        T panel = GetBasePanel<T>();
        CutFaterPanel(panel);
    }
    /// <summary>
    /// 堆栈面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callBack"></param>
    public void StackPanel<T>(UnityAction<T> callBack = null) where T : BasePanel, new()
    {
        PushPanel<T>(callBack);
    }
    /// <summary>
    /// 压入堆栈面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callBack"></param>
    public void PushPanel<T>(UnityAction<T> callBack = null) where T : BasePanel, new()
    {
        T panel = GetBasePanel<T>();
        BasePanel aPanel = stackActivePanel;
        stackActivePanel = panel;
        if (aPanel != null)
        {
            HidePanel(aPanel);
            stackPanel.Push(aPanel);
        }
        ShowPanel(panel, callBack);
    }
    /// <summary>
    /// 弹出堆栈面板
    /// </summary>
    public void PopPanel()
    {
        stackActivePanel = stackPanel.Pop();
        ShowPanel(stackActivePanel);
    }
    /// <summary>
    /// 堆面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callBack"></param>
    public void QueuePanel<T>(UnityAction<T> callBack = null) where T : BasePanel, new()
    {
        EnqueuePanel(callBack);
        if(queueActivePanel == null)
        {
            DequeuePanel();
        }
    }
    /// <summary>
    /// 压入堆面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callBack"></param>
    public void EnqueuePanel<T>(UnityAction<T> callBack = null) where T : BasePanel, new()
    {
        T panel = GetBasePanel<T>();
        queuePanel.Enqueue((panel, new ShowCallBack<T>(callBack)));
    }
    /// <summary>
    /// 弹出堆面板
    /// </summary>
    public void DequeuePanel()
    {
        if (queueActivePanel != null) return;
        IShowCallBack callback;
        (queueActivePanel, callback) = queuePanel.Dequeue();
        ShowPanel(queueActivePanel, callback);
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="T">面板脚本类型</typeparam>
    /// <param name="callBack">当面板预设体创建成功后 你想做的事</param>
    public void ShowPanel<T>(UnityAction<T> callBack = null) where T : BasePanel,new() 
    {
        T panel = GetBasePanel<T>();
        CutFaterPanel(panel);
        //panel.uiData = DataMgr.Instance.GetUIStr(panelName)
        M_ShowPanel(panel);
        if (callBack != null)
            callBack(panel);
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="T">面板脚本类型</typeparam>
    /// <param name="callBack">当面板预设体创建成功后 你想做的事</param>
    public void ShowPanel<T>(T panel, UnityAction<T> callBack = null) where T : BasePanel, new()
    {
        CutFaterPanel(panel);
        //panel.uiData = DataMgr.Instance.GetUIStr(panelName)
        M_ShowPanel(panel);
        if (callBack != null)
            callBack?.Invoke(panel);
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="T">面板脚本类型</typeparam>
    /// <param name="callBack">当面板预设体创建成功后 你想做的事</param>
    public void ShowPanel<T>(T panel, IShowCallBack showCallBack) where T : BasePanel, new()
    {
        CutFaterPanel(panel);
        //panel.uiData = DataMgr.Instance.GetUIStr(panelName)
        M_ShowPanel(panel);
        if (showCallBack != null)
            showCallBack?.Invoke(panel);
    }

    /// <summary>
    /// 显示子级面板
    /// </summary>
    /// <typeparam name="T">面板脚本类型</typeparam>
    /// <param name="parentPanel">面板父级</param>
    /// <param name="callBack">回调</param>
    public void ShowChilderPanel<T>(BasePanel parentPanel, UnityAction<T> callBack = null) where T : BasePanel, new() 
    {
        T panel = GetBasePanel<T>();
        SetFatherPanel(panel, parentPanel);
        //panel.uiData = DataMgr.Instance.GetUIStr(panelName)
        M_ShowPanel(panel);
        if (callBack != null)
            callBack?.Invoke(panel);
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    /// <param name="isFade">是否淡入淡出</param>
    public void HidePanel<T>(UnityAction callBack = null) where T : BasePanel, new()
    {
        T panel = GetBasePanel<T>();
        if (!panel.visible)
        {
            callBack?.Invoke();
        }
        M_HidePanel(panel, callBack);
    }

    /// <summary>
    /// 删除指定面板
    /// </summary>
    /// <param name="basePanel">指定面板</param>
    /// <param name="callBack">回调</param>
    /// <param name="isFade">是否淡入淡出</param>
    /// <param name="isClearParent">是否删除父面板的子面板（遍历父面板的子面板集时请设为false，然后手动删除）</param>
    public void HidePanel(BasePanel basePanel, UnityAction callBack = null)
    {
        if (!basePanel.visible)
        {
            callBack?.Invoke();
        }
        M_HidePanel(basePanel, callBack);
    }
    /// <summary>
    /// 得到某一个已经显示的主面板 方便外部使用
    /// </summary>
    public T GetPanel<T>() where T : BasePanel
    {
        string name = typeof(T).Name;
        if (panelDic.TryGetValue(name,out BasePanel panel))
        {
            return panel.visible ? panel as T : null;
        }
        return null;
    }
    /// <summary>
    /// 清除栈堆面板
    /// </summary>
    public void ClearAllQueueAndStack()
    {
        queuePanel.Clear();
        queueActivePanel = null;
        stackPanel.Clear();
        stackActivePanel = null;
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    /// <param name="ifClearQueueAndStack">是否清除栈堆面板</param>
    public void HideAllPanel(bool ifClearQueueAndStack = true)
    {
        if (ifClearQueueAndStack)
        {
            ClearAllQueueAndStack();
        }
        List<BasePanel> hidePanelList = new List<BasePanel>(visiblePanel);
        foreach (BasePanel bp in hidePanelList)
        {
            HidePanel(bp);
        }
    }
    /// <summary>
    /// 销毁所有面板
    /// </summary>
    /// <param name="excludeVisible">是否排除显示的面板</param>
    public void DestroyAllPanel(bool excludeVisible = true)
    {
        if (excludeVisible)
        {
            foreach (BasePanel bp in panelList)
            {
                if (bp.visible) 
                {
                    continue;
                };
                bp.OnDestory();
                GameObject.Destroy(bp.gameObject);
            }
            panelList.Clear();
            panelDic.Clear();
            foreach (BasePanel bp in visiblePanel)
            {
                panelList.Add(bp);
                panelDic.Add(bp.GetType().Name, bp);
            }
        }
        else
        {
            foreach (BasePanel bp in panelList)
            {
                HidePanel(bp);
                bp.OnDestory();
                GameObject.Destroy(bp.gameObject);
            }
            panelList.Clear();
            panelDic.Clear();
        }

    }

    private void M_ShowPanel(BasePanel panel)
    {
        panel.visible = true;
        panel.CanvasGroup.alpha = 1.0f;
        panel.CanvasGroup.blocksRaycasts = true;
        panel.CanvasGroup.interactable = true;
        visiblePanel.Add(panel);
        panleUpdata += panel.Update;
        panel.OnEnable();
        if (!panel.ifStart)
        {
            panel.ifStart = true;
            panel.Start();
        }
        panel.ShowMe();
    }

    private T GetBasePanel<T>() where T : BasePanel, new()
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            return panelDic[panelName] as T;
        }
        GameObject obj = GameObject.Instantiate(panelDatas.Find(i => i.name == panelName).gameObject);
        T panel = new T();
        InitBasePanel(obj, panel, panelName);
        return panel;
    }
    private BasePanel GetBasePanel(string panelName)
    {
        if (panelDic.ContainsKey(panelName))
        {
            return panelDic[panelName];
        }
        GameObject obj = GameObject.Instantiate(panelDatas.Find(i => i.name == panelName).gameObject);
        BasePanel panel = Activator.CreateInstance(Type.GetType(panelName)) as BasePanel;
        InitBasePanel(obj, panel, panelName);
        return panel;
    }
    private BasePanel InitBasePanel(GameObject obj,BasePanel panel,string panelName)
    {
        //得到预设体身上的面板脚本
        PanelUI panelUI = obj.GetComponent<PanelUI>();
        obj.GetComponent<Canvas>().worldCamera = uiCamera;
        panel.panelUI = panelUI;
        panel.Init(panelUI);
        panel.gameObject = obj;
        panel.CanvasGroup = obj.GetComponent<CanvasGroup>();
        panel.CanvasGroup.alpha = 0;
        panel.CanvasGroup.blocksRaycasts = false;
        panel.CanvasGroup.interactable = false;
        panelDic.Add(panelName, panel);
        panelList.Add(panel);
        obj.transform.SetParent(uiRoot);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
        (obj.transform as RectTransform).offsetMax = Vector2.zero;
        (obj.transform as RectTransform).offsetMin = Vector2.zero;
        obj.name = panelName;
        panel.Awake();
        return panel;
    }

    private void CutFaterPanel(BasePanel panel)
    {
        if(panel.parentPanel!= null)
        {
            panel.parentPanel.childrenPanels.Remove(panel.GetType().Name);
            panel.parentPanel = null;
            panel.gameObject.transform.SetParent(uiRoot);
        }
    }

    private void SetFatherPanel(BasePanel panelChildren,BasePanel panelFather)
    {
        if (panelChildren.parentPanel == panelFather) return;
        CutFaterPanel(panelChildren);
        panelChildren.parentPanel = panelFather;
        panelFather.childrenPanels.Add(panelChildren.GetType().Name, panelChildren);
        panelChildren.gameObject.transform.SetParent(panelFather.gameObject.transform);
    }


    private void M_HidePanel(BasePanel panel,UnityAction callBack)
    {
        hideCount++;
        foreach(var child in panel.childrenPanels)
        {
            M_HidePanel(child.Value, null);
        }
        CutFaterPanel(panel);
        visiblePanel.Remove(panel);
        panel.visible = false;
        panleUpdata -= panel.Update;
        panel.OnDisable();
        panel.HideMe(() =>
        {
            hideCount--;
            callBack?.Invoke();
            if (panel.visible == true) return;
            panel.CanvasGroup.alpha = 0;
            panel.CanvasGroup.blocksRaycasts = false;
            panel.CanvasGroup.interactable = false;
            ChackQueueAndStack(panel);
        });
    }

    private void ChackQueueAndStack(BasePanel panel)
    {
        if (queueActivePanel == panel) queueActivePanel = null;
        if (queuePanel.Count != 0 && queueActivePanel==null) 
        {
            DequeuePanel();
        }
        if (stackActivePanel == panel) stackActivePanel = null;
        if (stackPanel.Count != 0 && stackActivePanel == null)
        {
            PopPanel();
        }
    }

    /// <summary>
    /// 给控件添加自定义事件监听
    /// </summary>
    /// <param name="control">控件对象</param>
    /// <param name="type">事件类型</param>
    /// <param name="callBack">事件的响应函数</param>
    public static void AddCustomEventListener(UIBehaviour control, EventTriggerType type, UnityAction<BaseEventData> callBack)
    {
        EventTrigger trigger = control.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = control.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(callBack);

        trigger.triggers.Add(entry);
    }


}
