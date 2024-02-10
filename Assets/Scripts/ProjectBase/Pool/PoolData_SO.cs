using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoolData_SO",menuName = "Data_SO/PoolData_SO")]
public class PoolData_SO : ScriptableObject 
{
    public List<GameObjectPoolData> list = new List<GameObjectPoolData> ();
}
[Serializable]
public class GameObjectPoolData
{
    public int id;
    public string name;
    public GameObject gameObject;
}