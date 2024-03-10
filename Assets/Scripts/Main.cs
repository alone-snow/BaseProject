using BinaryReadWrite;
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
        //NetMgr.Instance.Init();

        UIManager.Instance.ShowPanel<LoginPanel>();

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
