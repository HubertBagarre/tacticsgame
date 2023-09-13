using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    private static EventManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning($"{Instance} is already an instance of {typeof(EventManager)}", Instance);
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private readonly Dictionary<Type, List<Delegate>> m_events = new();
    private readonly List<Delegate> ephemeral_events = new();

    public delegate void EventCallback<in T>(T data);

    public static void AddListener<T>(EventCallback<T> callback, bool removeAfterInvoke = false)
    {
        if (Instance.m_events.TryGetValue(typeof(T), out var listeners))
        {
            if (!listeners.Contains(callback))
            {
                listeners.Add(callback);
            }
        }
        else
        {
            listeners = new List<Delegate> {callback};
            Instance.m_events.Add(typeof(T), listeners);
        }

        if (removeAfterInvoke) Instance.ephemeral_events.Add(callback);
    }

    public static void RemoveListener<T>(EventCallback<T> callback)
    {
        if (!Instance.m_events.TryGetValue(typeof(T), out var listeners)) return;

        if (listeners.Contains(callback)) listeners.Remove(callback);
        if (Instance.ephemeral_events.Contains(callback)) Instance.ephemeral_events.Remove(callback);
    }

    public static void RemoveListeners<T>()
    {
        if (!Instance.m_events.TryGetValue(typeof(T), out var listeners)) return;

        foreach (var callback in listeners.Where(callback => Instance.ephemeral_events.Contains(callback)))
        {
            Instance.ephemeral_events.Remove(callback);
        }

        Instance.m_events.Remove(typeof(T));
    }

    public static void Trigger<T>(T triggeredKey)
    {
        if (!Instance.m_events.TryGetValue(typeof(T), out var listeners)) return;

        var listenersToRemove = new List<Delegate>();
        foreach (var listener in listeners.ToArray())
        {
            listener.DynamicInvoke(triggeredKey);

            if (Instance.ephemeral_events.Contains(listener))
            {
                Instance.ephemeral_events.Remove(listener);
                listenersToRemove.Add(listener);
            }
        }

        foreach (var listener in listenersToRemove)
        {
            listeners.Remove(listener);
        }
        listenersToRemove.Clear();
    }
}