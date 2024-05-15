using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using BinaryReadWrite;
using System.Collections.Generic;

public class WriterManage
{
    private static WriterManage instance = new WriterManage();
    public static WriterManage Instance => instance;

    Queue<Writer> writers = new Queue<Writer>();
    bool init;

    public static List<(string savePath, Assembly assembly)> assemblyList = new List<(string savePath, Assembly assembly)>()
    {
        (Application.dataPath + "/Scripts/ProjectBase/Writer/ReadWrite/",typeof(Writer).Assembly)
    };

    public static void Init()
    {
        if(!instance.init)
        {
            instance.init = true;
            foreach(var value in assemblyList)
            {
                instance.InitReaderWriter(value.assembly);
            }
        }
    }

    public byte[] Write<T>(T data)
    {
        Writer writer = writers.Count > 0 ? writers.Dequeue() : new Writer();
        writer.Write(data);

        byte[] result = writer.ToArray();
        
        writer.Reset();
        writers.Enqueue(writer);
        return result;
    }

    public T Read<T>(byte[] data)
    {
        Reader reader = new Reader(data);
        return reader.Read<T>();
    }

    public void InitReaderWriter(Assembly assembly)
    {
        Type writerType = typeof(Writer);
        Type writerGenericType = typeof(Writer<>);
        Type actionGenericType = typeof(Action<,>);
        Type readerType = typeof(Reader);
        Type readerGenericType = typeof(Reader<>);
        Type funcGenericType = typeof(Func<,>);
        var assemblyTypes = assembly.GetTypes().ToList();
        foreach (var type in assemblyTypes)
        {
            var methods = type.GetMethods();
            foreach (MethodInfo info in methods)
            {
                if (info.IsGenericMethod) continue;

                ParameterInfo[] pInfos = info.GetParameters();
                if (pInfos.Length == 2 && pInfos[0].ParameterType == writerType&& !pInfos[1].ParameterType.IsGenericType)
                {
                    
                    Type wgType = writerGenericType.MakeGenericType(pInfos[1].ParameterType);
                    Type agType = actionGenericType.MakeGenericType(writerType, pInfos[1].ParameterType);
                    var d = info.CreateDelegate(agType);
 
                    wgType.GetField("write").SetValue(null, d);
                }
                else if (pInfos.Length == 1 && pInfos[0].ParameterType == readerType && !info.ReturnType.IsGenericType)
                {
                    Type rgType = readerGenericType.MakeGenericType(info.ReturnType);
                    Type fgType = funcGenericType.MakeGenericType(readerType, info.ReturnType);
                    var d = info.CreateDelegate(fgType);
                    rgType.GetField("read").SetValue(null, d);
                }
            }
        }
    }
}
