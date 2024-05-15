using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 资源加载模块
/// 1.异步加载
/// 2.委托和 lambda表达式
/// 3.协程
/// 4.泛型
/// </summary>
public class ResMgr : BaseManager<ResMgr>
{
    BinaryFormatter binaryFormatter = new BinaryFormatter();
    /// <summary>
    /// 加载通用Resources资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T Load<T>(string name ) where T:Object
    {
        T res = Resources.Load<T>(name);
        //如果对象是一个GameObject类型的 我把他实例化后 再返回出去 外部 直接使用即可
        if (res is GameObject)
            return GameObject.Instantiate(res);
        else//TextAsset AudioClip
            return res;
    }
    /// <summary>
    /// 加载json
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T LoadJson<T>(string name) where T : new()
    {
        return JsonMgr.Instance.LoadData<T>(name);
    }
    /// <summary>
    /// 加载二进制
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T LoadWithBinary<T>(string name) where T : class
    {
        string path = Application.streamingAssetsPath + "/" + name;
        if (!File.Exists(path)) path = Application.persistentDataPath + "/" + name;
        if (!File.Exists(path))
        {
            Debug.Log("不存在文件：" + name);
            return default;
        }
        using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
            return binaryFormatter.Deserialize(fileStream) as T;
        }
    }
    public T LoadWithWriter<T>(string name) where T : class
    {
        string path = Application.streamingAssetsPath + "/" + name;
        if (!File.Exists(path)) path = Application.persistentDataPath + "/" + name;
        if (!File.Exists(path))
        {
            Debug.Log("不存在文件：" + name);
            return default;
        }
        return WriterManage.Instance.Read<T>(File.ReadAllBytes(path));

    }
    /// <summary>
    /// 读取文本
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string[] LoadAllLines(string name)
    {
        string path = Application.streamingAssetsPath + "/" + name;
        if (!File.Exists(path)) path = Application.persistentDataPath + "/" + name;
        if (!File.Exists(path))
        {
            Debug.Log("不存在文件：" + name);
            return null;
        }
        return File.ReadAllLines(path);
    }
    /// <summary>
    /// 读取所有字节
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public byte[] LoadAllBytes(string name)
    {
        string path = Application.streamingAssetsPath + "/" + name;
        if (!File.Exists(path)) path = Application.persistentDataPath + "/" + name;
        if (!File.Exists(path))
        {
            Debug.Log("不存在文件：" + name);
            return null;
        }
        return File.ReadAllBytes(path);
    }
    /// <summary>
    /// 保存json
    /// </summary>
    /// <param name="data"></param>
    /// <param name="name"></param>
    public void SaveJson(object data,string name)
    {
        JsonMgr.Instance.SaveData(data,name);
    }
    /// <summary>
    /// 保存二进制（数据应为一般类型）
    /// </summary>
    /// <param name="data"></param>
    /// <param name="name"></param>
    public void SaveWithBinary(object data , string name, bool ifPersistent = true)
    {
        string path = null;
        if (ifPersistent)
        {
            path = Path.Combine(Application.persistentDataPath, name);
        }
        else
        {
            path = Path.Combine(Application.streamingAssetsPath, name);
        }
        string directoryName = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
        using (FileStream fileStream = File.Create(Application.streamingAssetsPath + "/" + name))
        {
            binaryFormatter.Serialize(fileStream,data);
        }
    }
    public void SaveWithWriter<T>(T data, string name,bool ifPersistent = true)
    {
        string path = null;
        if (ifPersistent)
        {
            path = Path.Combine(Application.persistentDataPath, name);
        }
        else
        {
            path = Path.Combine(Application.streamingAssetsPath, name);
        }
        string directoryName = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
        byte[] datas = WriterManage.Instance.Write(data);
        File.WriteAllBytes(path, datas);
    }
    //异步加载资源
    public void LoadAsync<T>(string name, UnityAction<T> callback) where T:Object
    {
        //开启异步加载的协程
        MonoMgr.Instance.StartCoroutine(ReallyLoadAsync(name, callback));
    }

    //真正的协同程序函数  用于 开启异步加载对应的资源
    private IEnumerator ReallyLoadAsync<T>(string name, UnityAction<T> callback) where T : Object
    {
        ResourceRequest r = Resources.LoadAsync<T>(name);
        
        yield return r;

        //if (r.asset == null)
        //{
        //    UnityWebRequest req = new UnityWebRequest(DataManager.Instance.Web, UnityWebRequest.kHttpVerbGET);
        //    req.downloadHandler = new DownloadHandlerFile(Application.persistentDataPath + name);
        //    yield return req.SendWebRequest();
        //}

        if (r.asset is GameObject)
            callback(GameObject.Instantiate(r.asset) as T);
        else
            callback(r.asset as T);
    }


}