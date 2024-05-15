using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CreateAssetMenu(fileName = "PanelList_SO",menuName ="Data_SO/PanelList_SO")]
public class PanelList_SO : ScriptableObject
{
    public List<PanelData> panelDatas = new List<PanelData>();
    public void RigisterPanel(GameObject basePanel)
    {
        PanelData panelData = panelDatas.Find(i=>i.name == basePanel.name);
        if(panelData == null)
        {
            panelData = new PanelData();
            panelDatas.Add(panelData);
            panelData.name = basePanel.name;
        }
        panelData.gameObject = basePanel;
        Debug.Log("×¢²á³É¹¦");
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        EditorUtility.ClearDirty(this);
#endif
    }
}
[Serializable]
public class PanelData
{
    public string name;
    public GameObject gameObject;
}