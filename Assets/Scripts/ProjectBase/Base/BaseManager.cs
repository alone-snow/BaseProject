using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//1.C#中 泛型的知识
//2.设计模式中 单例模式的知识
public class BaseManager<T> where T:new()
{
    private static T instance = new T();

    public static T Instance => instance;
}

