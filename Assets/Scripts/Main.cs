using BinaryReadWrite;
using ExcelTool;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEditor.Progress;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //使用自建Writer和Reader
        WriterManage.Init();
        PlayerData player = new PlayerData() { id = 0 };
        //序列化
        Writer writer = new Writer();
        writer.Write(player);
        byte[] bytes = writer.ToArray();
        //反序列化
        Reader reader = new Reader(bytes);
        player = reader.ReadPlayerData();

        WriterManage.Init();
        List<ItemData> items = ItemData.Instance;

        //NetMgr.Instance.Init();
        //UIManager.Instance.ShowPanel<LoginPanel>();

        //UIManager.Instance.PreparePanel<TripleEliminationPanel>();
        //TEManager.Instance.Init(5, 6, 10, (o) =>
        //{
        //    foreach (var item in o)
        //    {
        //        Debug.Log(item);
        //    }
        //});
        //UIManager.Instance.ShowPanel<TripleEliminationPanel>();



    }
}
