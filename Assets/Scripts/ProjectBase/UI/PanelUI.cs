using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class PanelUIData
{
    public string key;
    public int index = 0;
    public UnityEngine.Object obj;
}

public class PanelUIDataComparer : IComparer<PanelUIData>
{
    public int Compare(PanelUIData x, PanelUIData y)
    {
        return string.Compare(x.key, y.key, StringComparison.Ordinal);
    }
}

public class PanelUI : MonoBehaviour, ISerializationCallbackReceiver
{
    public string assetName;
    public string bundleName;
    public string componentName;
    public List<PanelUIData> list = new List<PanelUIData>();
    private readonly Dictionary<string, Object> _referenceDic = new Dictionary<string, Object>();

    public T GetReference<T>(string key) where T : Object
    {
        return _referenceDic.TryGetValue(key, out var obj) ? (T)obj : null;
    }

    public T GetReferenceComponent<T>(string key) where T : Component
    {
        return _referenceDic.TryGetValue(key, out var obj) ? (obj as GameObject).GetComponent<T>() : null;
    }

    public Object GetReference(string key)
    {
        return _referenceDic.TryGetValue(key, out var obj) ? obj : null;
    }

    public void OnAfterDeserialize()
    {
        _referenceDic.Clear();

        foreach (var referenceCollectorData in list)
        {
            if (_referenceDic.ContainsKey(referenceCollectorData.key))
            {
                continue;
            }

            _referenceDic.Add(referenceCollectorData.key, referenceCollectorData.obj);
        }
    }

    public void OnBeforeSerialize() { }
}
