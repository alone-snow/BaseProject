using BinaryReadWrite;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetCore
{
    public interface IHandler
    {
        public void Recive(SyncUser user,Reader reader);
        public void Send(Writer writer, Message message);
    }

    public class MessageHandler<T> : IHandler
    {
        public int id;
        public Action<SyncUser,T , Reader> handler;

        public void Recive(SyncUser user, Reader reader)
        {
            T msg = reader.Read<T>();
            handler?.Invoke(user, msg, reader);
        }

        public void Send(Writer writer, Message message)
        {
            writer.Write(id);
            writer.Write((T)message);
        }
    }

    public static class NetMessagHandler
    {
        public static Dictionary<int, IHandler> handlerDic = new Dictionary<int, IHandler>();
        public static Dictionary<Type, IHandler> handlerTypeDic = new Dictionary<Type, IHandler>();
        static int id;
        static Stack<Reader> readerStack = new Stack<Reader>();
        static Writer writer = new Writer();
        public static void Register<T>(Action<SyncUser,T,Reader> callabck) where T : Message
        {
            MessageHandler<T> handler = new MessageHandler<T>() { id = id++, handler = callabck };
            handlerDic[handler.id] = handler;
            handlerTypeDic[typeof(T)] = handler;
        }

        public static void Recive(SyncUser user, byte[] data)
        {
            if (data == null || data.Length == 0) return;
            Reader reader = readerStack.Count > 0 ? readerStack.Pop() : new Reader(null);
            reader.SetBuffer(data);
            int id = reader.ReadInt();
            if (handlerDic.TryGetValue(id, out var handler))
            {
                handler.Recive(user, reader);
            }
            readerStack.Push(reader);
        }

        public static byte[] Send<T>(T msg) where T : Message
        {
            if (handlerTypeDic.TryGetValue(msg.GetType(), out var handler))
            {
                writer.Reset();
                handler.Send(writer, msg);
                return writer.ToArray();
            }
            return null;
        }

        public static void Send<T>(T msg, Writer writer) where T : Message
        {
            if (handlerTypeDic.TryGetValue(msg.GetType(), out var handler))
            {
                handler.Send(writer, msg);
            }
        }
    }
}
