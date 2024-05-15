using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IFloatWindow 
{
    public bool ifShow {  get; set; }
    public void Hide();
}

public abstract class FloatWindow<T> : UIBehaviour,IFloatWindow
{
    private static IFloatWindow single;
    public bool ifShow { get; set; }

    [HideInInspector] public RectTransform rt;
    [HideInInspector] public BasePanel panel;
    [HideInInspector] public Rect panelRect;
    [HideInInspector] public T item;
    [HideInInspector] public int type;

    protected override void Awake()
    {
        rt = transform as RectTransform;
    }

    public void Show(T obj, BasePanel panel, UnityAction<IFloatWindow> callback, int type = 0)
    {
        if (single != null && single.ifShow) single.Hide();
        PoolMgr.Instance.GetObj("FloatWindow_" + this.name, this.gameObject, (o) =>
        {
            o.transform.SetParent(panel.gameObject.transform, false);
            FloatWindow<T> fw = o.GetComponent<FloatWindow<T>>();
            fw.rt.anchorMax = Vector2.zero;
            fw.rt.anchorMin = Vector2.zero;
            fw.rt.pivot = Vector2.right;
            fw.ifShow = true;
            fw.panel = panel;
            fw.type = type;
            fw.item = obj;
            fw.panelRect = (panel.gameObject.transform as RectTransform).rect;
            //LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            fw.MoveAtMouse();
            single = fw;
            fw.OnShow();
            callback?.Invoke(fw);
        });
    }

    protected abstract void OnShow();

    public void Hide() 
    {
        if (ifShow)
        {
            PoolMgr.Instance.PushObj("FloatWindow_" + this.name, this.gameObject);
            ifShow = false;
            OnHide();
        }
    }

    protected abstract void OnHide();

    private void Update()
    {
        if(type == 0)
        {
            MoveAtMouse();
        }else if(type == 1)
        {
            if(Input.GetMouseButtonDown(0)|| Input.GetMouseButtonDown(1))
            {
                Hide();
            }
        }
    }

    private void MoveAtMouse()
    {
        Vector2 pos = Input.mousePosition;
        if (pos.x < rt.rect.width) pos.x = rt.rect.width;
        if (pos.y + rt.rect.height > panelRect.height) pos.y = panelRect.height - rt.rect.height;
        rt.anchoredPosition = pos;
    }
}
