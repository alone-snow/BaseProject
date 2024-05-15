using BinaryReadWrite;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetCore
{
    public class SyncEntity
    {
        public NetCore net;
        public int runtimeID;
        public uint ownUserId;

        public int state;
        public void ChangeState(int state)
        {
            this.state |= state;
        }

        public void Serialize(Writer writer)
        {

        }

        public void Deserialize(Reader reader)
        {

        }

        public void Start()
        {

        }

        public void Updata()
        {

        }

        public void End()
        {

        }
    }
}
