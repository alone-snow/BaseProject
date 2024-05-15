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
        //�����ȡ����
        Type t = obj.GetType();
        //��ȡ���г�Ա����
        FieldInfo[] fieldInfos = t.GetFields();
        //ʵ����һ���¶���
        object resurt;
        //�ж��ǲ����������ͣ�����ֱ�ӷ��ػ��ߴ���
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
        //�������ж���һһ��ֵ
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
        //�������Ͳ�ͬ���ز�ͬ�Ŀ�¡����
        object GetValue(object objRe)
        {
            if (objRe == null) return null;
            //�Զ����¡����
            if (objRe is ICloneable clone)
            {
                object cloneObj;
                //�ж϶����ǲ�������
                if (clone is Array array)
                {
                    if (ChackRef(array, out cloneObj)) return cloneObj;
                    //��ȡά��
                    int rank = array.Rank;
                    //��ȡά�ȳ���
                    int[] lenghs = new int[rank];
                    for (int i = 0; i < rank; i++)
                    {
                        lenghs[i] = array.GetLength(i);
                    }
                    //��������
                    Array cloneArray = Array.CreateInstance(array.GetType().GetElementType(), lenghs);
                    oldList.Add(array);
                    newList.Add(cloneArray);
                    //����һ��ά��ָ��
                    int[] ints = new int[rank];
                    //��ʼ������ֵ
                    ForEach(rank - 1, ints);
                    cloneObj = cloneArray;
                    //i��ά�ȣ�ÿһ�εݹ�����������һ��ά�ȣ�ֱ�������һ�㿪ʼ����ȫ�����ȸ�ֵ��Ȼ���˳�����
                    //��һ��ά��ָ������һ���ٽ������һ�����ȫ�����ȸ�ֵ��ֱ������λ�ø�ֵ
                    void ForEach(int i, int[] ints)
                    {
                        //������ά��ȫ������
                        for (int j = 0; j < lenghs[i]; j++)
                        {
                            //����ά��ָ��ĸ�ά��
                            ints[i] = j;
                            //�ж��ǲ������һ��ά�ȣ����Ǿ�����
                            if (i > 0)
                            {
                                ForEach(i - 1, ints);
                            }
                            else
                            {
                                //���һ��ά�ȣ�����Array.SetValue��ֵ
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
