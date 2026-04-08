using System.IO;
using UnityEngine;

public static class SavingSystem
{
    private static string GetPath(string key)
    {
        return Path.Combine(Application.persistentDataPath, key + ".json");
    }

    // ✅ SAVE GENERIC
    public static void Save<T>(string key, T data)
    {
        string json = JsonUtility.ToJson(data, true);
        string path = GetPath(key);

        File.WriteAllText(path, json);

        Debug.Log($"Saved {typeof(T).Name} to: {path}");
    }

    // ✅ LOAD GENERIC
    public static T Load<T>(string key)
    {
        string path = GetPath(key);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"No save file found at: {path}");
            return default;
        }

        string json = File.ReadAllText(path);
        T data = JsonUtility.FromJson<T>(json);

        Debug.Log($"Loaded {typeof(T).Name} from: {path}");

        return data;
    }

    // ✅ CHECK EXISTS
    public static bool Exists(string key)
    {
        return File.Exists(GetPath(key));
    }

    // ✅ DELETE
    public static void Delete(string key)
    {
        string path = GetPath(key);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted save file: {path}");
        }
    }
}