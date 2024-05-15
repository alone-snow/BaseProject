using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExtraButton : Button
{
    private UnityEvent m_OnPointerDown = new UnityEvent();
    private UnityEvent m_OnPointerUp = new UnityEvent();
    private UnityEvent m_OnPointerEnter = new UnityEvent();
    private UnityEvent m_OnPointerExit = new UnityEvent();
    public UnityEvent onPointerDown
    {
        get { return m_OnPointerDown; }
        set { m_OnPointerDown = value; }
    }
    public UnityEvent onPointerUp
    {
        get { return m_OnPointerUp; }
        set { m_OnPointerUp = value; }
    }
    public UnityEvent onPointerEnter
    {
        get { return m_OnPointerEnter; }
        set { m_OnPointerEnter = value; }
    }
    public UnityEvent onPointerExit
    {
        get { return m_OnPointerExit; }
        set { m_OnPointerExit = value; }
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        m_OnPointerDown.Invoke();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        m_OnPointerUp.Invoke();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        m_OnPointerEnter.Invoke();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        m_OnPointerExit.Invoke();
    }
}
