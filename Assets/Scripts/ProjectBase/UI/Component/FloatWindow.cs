using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FloatWindow : UIBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject window;
    [HideInInspector] public UnityEvent<FloatWindow> onPointerEnter;
    [HideInInspector] public UnityEvent<FloatWindow> onPointerExit;
    private Dictionary<string, List<UIBehaviour>> controlDic = new Dictionary<string, List<UIBehaviour>>();

    protected override void Awake()
    {
        base.Awake();
        FindChildrenControl<Button>();
        FindChildrenControl<Image>();
        FindChildrenControl<Text>();
        FindChildrenControl<Toggle>();
        FindChildrenControl<Slider>();
        FindChildrenControl<ScrollRect>();
        FindChildrenControl<InputField>();
        FindChildrenControl<Dropdown>();
        FindChildrenControl<FloatWindow>();
        FindChildrenControl<IntegerTriger>();
    }

    /// <summary>
    /// 得到对应名字的对应控件脚本
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="controlName"></param>
    /// <returns></returns>
    public T GetControl<T>(string controlName) where T : UIBehaviour
    {
        if (controlDic.ContainsKey(controlName))
        {
            for (int i = 0; i < controlDic[controlName].Count; ++i)
            {
                if (controlDic[controlName][i] is T)
                    return controlDic[controlName][i] as T;
            }
        }

        return null;
    }

    /// <summary>
    /// 找到子对象的对应控件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private void FindChildrenControl<T>() where T : UIBehaviour
    {
        T[] controls = window.GetComponentsInChildren<T>();
        for (int i = 0; i < controls.Length; ++i)
        {
            string objName = controls[i].gameObject.name;
            if (controlDic.ContainsKey(objName))
                controlDic[objName].Add(controls[i]);
            else
                controlDic.Add(objName, new List<UIBehaviour>() { controls[i] });
        }
    }

    

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log(eventData.position);
        window.SetActive(true);
        onPointerEnter?.Invoke(this);


    }

    public void OnPointerExit(PointerEventData eventData)
    {
        window.SetActive(false);
        onPointerExit.Invoke(this);
    }
}
