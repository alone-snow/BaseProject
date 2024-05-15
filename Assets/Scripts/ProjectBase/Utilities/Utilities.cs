using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class Utilities
{
    public static bool TryClearNullKey<Key,Value>(this Dictionary<Key,Value> dic,out Value value)
    {
        foreach(var kvp in dic)
        {
            if(kvp.Key == null)
            {
                dic.Remove(kvp.Key);
                value = kvp.Value;
                return true;
            }
        }
        value = default;
        return false;
    }

    static List<object> oldList = new List<object>();
    static List<object> newList = new List<object>();
    static int count;
    public static object Clone<T>(T obj)
    {
        //反射获取类型
        Type t = obj.GetType();
        //获取所有成员变量
        FieldInfo[] fieldInfos = t.GetFields();
        //实例化一个新对象
        object resurt;
        //判断是不是引用类型，查找直接返回或者储存
        if (t.IsByRef)
        {
            if(ChackRef(obj, out resurt)) return resurt;
            resurt = Activator.CreateInstance(t);
            oldList.Add(obj);
            newList.Add(resurt);
        }
        else
        {
            resurt = Activator.CreateInstance(t);
        }
        count++;
        //遍历所有对象一一赋值
        foreach (FieldInfo field in fieldInfos)
        {
            if (field.GetCustomAttribute<NotCloneAttribute>() != null) continue;

            field.SetValue(resurt, GetValue(field.GetValue(obj)));
        }
        count--;
        if(count == 0)
        {
            oldList.Clear();
            newList.Clear();
        }
        return resurt;
        //根据类型不同返回不同的克隆对象
        object GetValue(object objRe)
        {
            if (objRe == null) return null;
            //自定义克隆对象
            if (objRe is ICloneable clone)
            {
                object cloneObj;
                //判断对象是不是数组
                if (clone is Array array)
                {
                    if (ChackRef(array, out cloneObj)) return cloneObj;
                    //获取维度
                    int rank = array.Rank;
                    //获取维度长度
                    int[] lenghs = new int[rank];
                    for (int i = 0; i < rank; i++)
                    {
                        lenghs[i] = array.GetLength(i);
                    }
                    //创建数组
                    Array cloneArray = Array.CreateInstance(array.GetType().GetElementType(), lenghs);
                    oldList.Add(array);
                    newList.Add(cloneArray);
                    //声明一个维度指针
                    int[] ints = new int[rank];
                    //开始遍历赋值
                    ForEach(rank - 1, ints);
                    cloneObj = cloneArray;
                    //i是维度，每一次递归进入就是深入一层维度，直到到最后一层开始遍历全部长度赋值，然后退出本层
                    //上一层维度指针数加一，再进来最后一层遍历全部长度赋值，直到所有位置赋值
                    void ForEach(int i, int[] ints)
                    {
                        //遍历该维度全部长度
                        for (int j = 0; j < lenghs[i]; j++)
                        {
                            //调整维度指针的该维度
                            ints[i] = j;
                            //判断是不是最后一层维度，不是就深入
                            if (i > 0)
                            {
                                ForEach(i - 1, ints);
                            }
                            else
                            {
                                //最后一层维度，调用Array.SetValue赋值
                                object arrayObj = array.GetValue(ints);
                                cloneArray.SetValue(GetValue(arrayObj), ints);
                            }
                        }

                    }
                }
                else
                {
                    cloneObj = clone.Clone();
                }
                return cloneObj;
            }
            else if (objRe is IList ilist)
            {
                if (ChackRef(ilist, out object cloneObj)) return cloneObj;
                IList list = (IList)Activator.CreateInstance(objRe.GetType());
                oldList.Add(ilist);
                newList.Add(list);
                foreach (var value in ilist)
                {
                    list.Add(GetValue(value));
                }
                return list;
            }
            else if (objRe is IDictionary iDic)
            {
                if (ChackRef(iDic, out object cloneObj)) return cloneObj;
                IDictionary dic = (IDictionary)Activator.CreateInstance(objRe.GetType());
                oldList.Add(iDic);
                newList.Add(dic);
                foreach (DictionaryEntry value in iDic)
                {
                    dic.Add(GetValue(value.Key), GetValue(value.Value));
                }
                return dic;
            }
            else
            {
                return objRe;
            }
        }

        bool ChackRef(object obj,out object objResurt)
        {
            int index = oldList.IndexOf(obj);
            if (index != -1)
            {
                objResurt = newList[index];
                return true;
            }
            else
            {
                objResurt = null;
                return false;
            }
        }
    }

    public static bool ExDistance(int x, int y, int distance)
    {
        return x * x + y * y > distance * distance;
    }
}
