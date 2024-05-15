using BinaryReadWrite;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AutoData<T> where T : AutoData<T>,new()
{
    public static List<T> dataSet;
    public static List<T> Instance
    {
        get 
        {
            if (dataSet == null)
            {
                string name = (string)typeof(T).GetField("Path").GetValue(null);
                string path = Application.streamingAssetsPath + "/" + name;
                if (!File.Exists(path)) path = Application.persistentDataPath + "/" + name;
                if (!File.Exists(path))
                {
                    Debug.Log("不存在文件：" + name);
                }
                byte[] bytes = File.ReadAllBytes(path);
                Reader reader = new Reader(bytes);
                dataSet = reader.ReadList<T>();
            }
            return dataSet;
        }
    }

    public static void Dispose()
    {
        dataSet = null;
    }
}
