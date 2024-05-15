using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Message
{

}

public struct S2C_AddUser: Message
{
    public uint userID;
}

public struct S2C_RemoveUser: Message
{
    public uint userID;
}

public struct S2C_AddEntity : Message
{
    public int entityID;
    public int runtimeID;
    public uint ownUserID;
}

public struct S2C_RemoveEntity: Message
{
    public int entityID;
}

public struct S2C_EntityStateChange: Message
{
    public int entityID;
    public int state;
}

public struct S2C_EntityMethodCall: Message
{
    public int entityID;
    public int methodID;
}

public struct S2C_EntityComponentChange : Message
{
    public int entityID;
    public int componentID;
}