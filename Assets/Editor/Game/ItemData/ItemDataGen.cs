using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemDataGen
{
    [MenuItem("Tools/Game/生成物品数据")]
    public static void Gen()
    {
        WriterManage.Init();
        string path = Application.dataPath + "/Editor/Game/ItemData/Data/ItemData.xlsx";
        AutoExcelTool.Triger("Game/ItemData", path, AutoType.sigle, GenType.allFile);
    }

    [MenuItem("Tools/Game/生成物品数据(不生成脚本)")]
    public static void GenNot()
    {
        WriterManage.Init();
        string path = Application.dataPath + "/Editor/Game/ItemData/Data/ItemData.xlsx";
        AutoExcelTool.Triger("Game/ItemData", path, AutoType.sigle, GenType.allFile, ifGenScript: false);
    }
}
