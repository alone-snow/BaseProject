using BinaryReadWrite;
using NetCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetCore
{
    public interface IEntityGen
    {
        public SyncEntity Gen();
    }

    public class EntityGen<T> : IEntityGen where T : SyncEntity, new()
    {
        public SyncEntity Gen()
        {
            return new T();
        }
    }


    public class SyncEntityFactory
    {
        static List<IEntityGen> entityGenList = new List<IEntityGen>();
        static Dictionary<Type,int> entityDic = new Dictionary<Type,int>();

        public static void Register<T>()where T : SyncEntity, new()
        {
            entityGenList.Add(new EntityGen<T>());
            entityDic[typeof(T)] = entityGenList.Count - 1;
        }

        public static int GetEntityID<T>(T entity) where T : SyncEntity, new()
        {
            return entityDic[typeof(T)];
        }

        public static SyncEntity Gen(int entityID,int runTimeID,Reader reader)
        {
            if (entityID < 0 || entityID >= entityGenList.Count) return null;
            var entity = entityGenList[entityID].Gen();
            entity.runtimeID = runTimeID;
            entity.Deserialize(reader);
            return entity;
        }
    }
}
