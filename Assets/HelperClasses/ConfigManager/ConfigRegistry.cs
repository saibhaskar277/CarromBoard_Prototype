using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Drop this in your bootstrap scene and assign all config ScriptableObjects.
/// It auto-registers them into ConfigManager on Awake.
/// </summary>
public class ConfigRegistry : MonoBehaviour
{
    [Header("Assign all game configs here")]
    [SerializeField] private List<ScriptableObject> configs = new();

    private void Awake()
    {
        RegisterAll();
    }

    public void RegisterAll()
    {
        foreach (var config in configs)
        {
            if (config == null)
                continue;

            ConfigManager.Register(config);
        }
    }
}

public static class ConfigManager
{
    private static readonly Dictionary<Type, ScriptableObject> configMap = new();

    public static void Register(ScriptableObject config)
    {
        if (config == null)
        {
            Debug.LogError("Trying to register null config");
            return;
        }

        Type type = config.GetType();
        configMap[type] = config;
    }

    public static void Register<T>(T config) where T : ScriptableObject
    {
        Register((ScriptableObject)config);
    }

    public static T GetConfig<T>(Func<T> fallback = null) where T : ScriptableObject
    {
        Type type = typeof(T);

        if (configMap.TryGetValue(type, out ScriptableObject config))
            return config as T;

        if (fallback != null)
        {
            T created = fallback.Invoke();
            if (created != null)
            {
                Register(created);
                return created;
            }
        }

        Debug.LogError($"Config not found: {type.Name}");
        return null;
    }

    public static bool HasConfig<T>() where T : ScriptableObject
    {
        return configMap.ContainsKey(typeof(T));
    }

    public static void Clear()
    {
        configMap.Clear();
    }
}
