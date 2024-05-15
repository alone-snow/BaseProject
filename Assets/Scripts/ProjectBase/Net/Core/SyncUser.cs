using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetCore 
{
    public class SyncUser
    {
        public static uint ID = 1;
        public uint userID;

        public SyncUser() 
        {
            userID = ID++;
        }

        public SyncUser(uint userID)
        {
            this.userID = userID;
        }
    }
}


