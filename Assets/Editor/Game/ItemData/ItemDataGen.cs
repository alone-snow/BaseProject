using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemDataGen
{
    [MenuItem("Tools/Game/������Ʒ����")]
    public static void Gen()
    {
        WriterManage.Init();
        string path = Application.dataPath + "/Editor/Game/ItemData/Data/ItemData.xlsx";
        AutoExcelTool.Triger("Game/ItemData", path, AutoType.sigle, GenType.allFile);
    }

    [MenuItem("Tools/Game/������Ʒ����(�����ɽű�)")]
    public static void GenNot()
    {
        WriterManage.Init();
        string path = Application.dataPath + "/Editor/Game/ItemData/Data/ItemData.xlsx";
        AutoExcelTool.Triger("Game/ItemData", path, AutoType.sigle, GenType.allFile, ifGenScript: false);
    }
}
