using System;
using System.Collections.Generic;

public static class EventManager
{
    private static readonly Dictionary<Type, Delegate> eventTable = new();

    public static void AddListner<T>(Action<T> listener) where T : IGameEvent
    {
        Type type = typeof(T);

        if (eventTable.TryGetValue(type, out Delegate existing))
        {
            eventTable[type] = Delegate.Combine(existing, listener);
        }
        else
        {
            eventTable[type] = listener;
        }
    }

    public static void RemoveListner<T>(Action<T> listener) where T : IGameEvent
    {
        Type type = typeof(T);

        if (!eventTable.TryGetValue(type, out Delegate existing))
            return;

        Delegate newDelegate = Delegate.Remove(existing, listener);

        if (newDelegate == null)
            eventTable.Remove(type);
        else
            eventTable[type] = newDelegate;
    }

    public static void RaiseEvent<T>(T gameEvent) where T : IGameEvent
    {
        Type type = typeof(T);

        if (eventTable.TryGetValue(type, out Delegate existing))
        {
            if (existing is Action<T> callback)
            {
                callback.Invoke(gameEvent);
            }
        }
    }

    internal static void AddListner<T>()
    {
        throw new NotImplementedException();
    }
}


public interface IGameEvent
{

}