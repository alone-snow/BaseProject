using Steamworks;
using System;
using UnityEngine;

public class SteamFridends : MonoBehaviour
{
    public static SteamFridends Instance { get; set; }
    public Action UpdateFridendListEvent;
    void Awake()
    {
        Instance = this;
        UpdateFridendListEvent += OnUpdateFridendListEvent;
        
    }
    /// <summary>
    /// 更新好友列表
    /// </summary>
    private void OnUpdateFridendListEvent()
    {

    }

}
