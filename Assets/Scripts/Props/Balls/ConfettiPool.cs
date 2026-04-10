using System.Collections.Generic;
using UnityEngine;

public static class ConfettiPool
{
    private static readonly Dictionary<int, Queue<GameObject>> Pools = new Dictionary<int, Queue<GameObject>>();
    private static readonly Dictionary<int, int> CreatedPerPrefab = new Dictionary<int, int>();
    private static Transform poolRoot;

    public static GameObject Rent(GameObject prefab, int maxInstances)
    {
        if (prefab == null)
        {
            return null;
        }

        EnsurePoolRoot();

        int key = prefab.GetInstanceID();
        if (!Pools.TryGetValue(key, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            Pools[key] = pool;
            CreatedPerPrefab[key] = 0;
        }

        while (pool.Count > 0)
        {
            GameObject pooled = pool.Dequeue();
            if (pooled != null)
            {
                return pooled;
            }
        }

        int createdCount = CreatedPerPrefab[key];
        if (createdCount >= maxInstances)
        {
            return null;
        }

        GameObject created = Object.Instantiate(prefab, poolRoot);
        created.SetActive(false);
        CreatedPerPrefab[key] = createdCount + 1;
        return created;
    }

    public static void Return(GameObject prefab, GameObject instance)
    {
        if (prefab == null || instance == null)
        {
            return;
        }

        int key = prefab.GetInstanceID();
        if (!Pools.TryGetValue(key, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            Pools[key] = pool;
        }

        instance.SetActive(false);
        pool.Enqueue(instance);
    }

    private static void EnsurePoolRoot()
    {
        if (poolRoot != null)
        {
            return;
        }

        GameObject root = new GameObject("ConfettiPool");
        Object.DontDestroyOnLoad(root);
        poolRoot = root.transform;
    }
}
