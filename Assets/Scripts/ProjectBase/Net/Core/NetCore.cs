using BinaryReadWrite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NetCore
{
    public abstract class NetCore
    {
        Dictionary<uint,SyncUser> userDic = new Dictionary<uint, SyncUser>();
        Dictionary<int, SyncEntity> entityDic = new Dictionary<int, SyncEntity>();
        List<SyncEntity> syncEntitiesList = new List<SyncEntity>();
        int entityRuntimeID = 1;
        uint userID;
        SyncUser server;
        bool isServer;
        bool isClient;

        public void Updata()
        {

        }

        protected abstract void SendData(SyncUser user,byte[] bytes);

        public void Recive(SyncUser user, byte[] bytes)
        {
            NetMessagHandler.Recive(user,bytes);
        }

        public void Send<T>(SyncUser user,T message) where T : Message
        {
            SendData(user,NetMessagHandler.Send(message));
        }

        public void StartServer()
        {
            isServer = true;
            MessageInit();
        }

        public void StartClient()
        {
            isClient = true;
            MessageInit();
        }

        private void MessageInit()
        {
            NetMessagHandler.Register<S2C_AddUser>(AddUserHandler);
            NetMessagHandler.Register<S2C_RemoveUser>(RemoveUserHandler);
            NetMessagHandler.Register<S2C_AddEntity>(AddEntityHandler);
            NetMessagHandler.Register<S2C_RemoveEntity>(RemoveEntityHandler);
            NetMessagHandler.Register<S2C_EntityStateChange>(EntityStateChangeHandler);
            NetMessagHandler.Register<S2C_EntityMethodCall>(EntityMethodCallHandler);
            NetMessagHandler.Register<S2C_EntityComponentChange>(EntityComponentChangeHandler);
        }

        public uint UserJionServer(uint userID)
        {
            if (!isServer) return  0;
            if(userDic.TryGetValue(userID, out var user))
            {
                return userID;
            }
            else
            {
                SyncUser user1 = new SyncUser();
                userDic.Add(user1.userID, user1);
                foreach (var user2 in userDic.Values)
                {
                    Send(user2,new S2C_AddUser() { userID = user1.userID });
                }
                return user1.userID;
            }
        }

        public void UserLeaveServer(uint userID)
        {
            if (!isServer) return;
            if (userDic.TryGetValue(userID, out var user))
            {
                userDic.Remove(userID);
                //SendAll(new S2C_RemoveUser() {  userID = userID });
            }
        }

        public void EntitySync(uint id,SyncEntity entity)
        {
            if (isServer)
            {
                entity.runtimeID = entityRuntimeID++;
                //SendAll(new S2C_AddEntity() { entityID = SyncEntityFactory.GetEntityID(entity), ownUserID = server.userID });
            }
        }

        private void AddUserHandler(SyncUser user, S2C_AddUser msg,Reader reader)
        {
            if (isServer) return;
            userDic[msg.userID] = new SyncUser(msg.userID);
        }

        private void RemoveUserHandler(SyncUser user, S2C_RemoveUser msg, Reader reader)
        {
            if (isServer) return;
            userDic.Remove(msg.userID);
            for (int i = syncEntitiesList.Count - 1; i >= 0; i--)
            {
                if (syncEntitiesList[i].ownUserId == msg.userID)
                {
                    syncEntitiesList[i].End();
                    entityDic.Remove(syncEntitiesList[i].runtimeID);
                    syncEntitiesList.RemoveAt(i);
                }
            }
        }

        private void AddEntityHandler(SyncUser user, S2C_AddEntity msg, Reader reader)
        {
            if (isServer)
            {
                var entity = SyncEntityFactory.Gen(msg.entityID, entityRuntimeID++,reader);
                entityDic.Add(entity.runtimeID, entity);
                syncEntitiesList.Add(entity);
                //SendAll(new S2C_AddEntity() { entityID = msg.entityID, ownUserID = user.userID });
            }
            else
            {
                var entity = SyncEntityFactory.Gen(msg.entityID,msg.runtimeID,reader);
                entityDic.Add(entity.runtimeID, entity);
                syncEntitiesList.Add(entity);
            }
        }

        private void RemoveEntityHandler(SyncUser user, S2C_RemoveEntity msg, Reader reader)
        {
            if (isServer)
            {
                //SendAll(new S2C_RemoveEntity() { entityID = msg.entityID });
            }
            if (entityDic.Remove(msg.entityID))
            {
                for(int i = syncEntitiesList.Count -1; i >= 0; i--)
                {
                    if (syncEntitiesList[i].runtimeID == msg.entityID)
                    {
                        syncEntitiesList[i].End();
                        syncEntitiesList.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private void EntityStateChangeHandler(SyncUser user, S2C_EntityStateChange msg, Reader reader)
        {

        }

        private void EntityMethodCallHandler(SyncUser user, S2C_EntityMethodCall msg, Reader reader)
        {

        }

        private void EntityComponentChangeHandler(SyncUser user, S2C_EntityComponentChange msg, Reader reader)
        {

        }

    }
}
