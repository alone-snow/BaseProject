using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IntegerTriger : UIBehaviour
{
    private int m_Value;

    [SerializeField] private TextMeshProUGUI content;
    [SerializeField] private Button up;
    [SerializeField] private Button down;

    private UnityEvent<int> m_ValueChanged = new UnityEvent<int>();

    public int value
    {
        get { return m_Value; }
        set 
        { 
            content.text = value.ToString();
            m_Value = value;
            m_ValueChanged?.Invoke(m_Value); 
        }
    }

    public UnityEvent<int> OnValueChanged => m_ValueChanged;

    protected override void Awake()
    {
        base.Awake();
        up.onClick.AddListener(() =>
        {
            value = m_Value + 1;
        });
        down.onClick.AddListener(() =>
        {
            value = m_Value - 1;
        });
    }
}
