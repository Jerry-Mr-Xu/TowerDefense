using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池管理者
/// </summary>
public class GOPoolManager : MonoBehaviour
{
    [SerializeField]
    private List<GOPoolConfig> configList;
    private Dictionary<GOName, GOPoolConfig> configMap = new Dictionary<GOName, GOPoolConfig>();
    // 存放一个对象池和key的map
    // key代表这个对象池中存放的东西
    private Dictionary<GOName, Queue<GameObject>> poolMap = new Dictionary<GOName, Queue<GameObject>>();

    // 单例
    public static GOPoolManager Instance { get; private set; }

    private void Awake()
    {
        // 初始化单例
        Instance = this;
    }

    private void Start()
    {
        // 根据配置初始化对象池
        foreach (GOPoolConfig config in configList)
        {
            // 先把配置表从List转成Dictionary
            configMap.Add(config.goName, config);

            Queue<GameObject> goQueue = new Queue<GameObject>(config.size);
            for (int i = 0; i < config.size; i++)
            {
                GameObject go = Instantiate(config.goPrefab, config.goParent);
                go.SetActive(false);
                goQueue.Enqueue(go);
            }

            poolMap.Add(config.goName, goQueue);
        }
    }

    /// <summary>
    /// 从对象池中获取对象
    /// </summary>
    /// <param name="goName">对象名称</param>
    /// <returns>对象池中获取的对象或新生成对象</returns>
    public GameObject GetGO(GOName goName)
    {
        return GetGO(goName, null);
    }

    public GameObject GetGO(GOName goName, Transform parent)
    {
        return GetGO(goName, Vector3.zero, Quaternion.identity, parent);
    }

    /// <summary>
    /// 从对象池中获取对象
    /// </summary>
    /// <param name="goName">对象名称</param>
    /// <param name="position">获取对象所在坐标</param>
    /// <param name="rotation">获取对象所向角度</param>
    /// <returns>对象池中对象或新生成对象</returns>
    public GameObject GetGO(GOName goName, Vector3 position, Quaternion rotation)
    {
        return GetGO(goName, position, rotation, null);
    }

    /// <summary>
    /// 从对象池中获取对象
    /// </summary>
    /// <param name="goName">对象名称</param>
    /// <param name="position">获取对象所在坐标</param>
    /// <param name="rotation">获取对象所向角度</param>
    /// <param name="parent">获取对象所依附的父容器</param>
    /// <returns>对象池中对象或新生成对象</returns>
    public GameObject GetGO(GOName goName, Vector3 position, Quaternion rotation, Transform parent)
    {
        // 首先判断对象池是否有这个对象
        if (!poolMap.ContainsKey(goName))
        {
            Debug.LogError("No such goPool called " + goName.ToString());
            return null;
        }

        Queue<GameObject> goQueue = poolMap[goName];
        if (goQueue.Count > 0)
        {
            // 如果对象池中有则直接拿
            GameObject go = goQueue.Dequeue();
            go.SetActive(true);
            go.transform.position = position;
            go.transform.rotation = rotation;
            if (parent != null)
            {
                go.transform.parent = parent;
            }
            return go;
        }
        else
        {
            // 否则重新创建
            GOPoolConfig config = configMap[goName];
            return Instantiate(config.goPrefab, position, rotation, parent != null ? parent : config.goParent);
        }
    }

    /// <summary>
    /// 回收对象到对象池
    /// </summary>
    /// <param name="goName">对象名称</param>
    /// <param name="needRecycleGO">需要被回收的对象</param>
    public void RecycleGO(GOName goName, GameObject needRecycleGO)
    {
        // 首先判断对象池是否有这个对象
        if (!poolMap.ContainsKey(goName))
        {
            Debug.LogError("No such goPool called " + goName.ToString());
            return;
        }

        Queue<GameObject> goQueue = poolMap[goName];
        GOPoolConfig config = configMap[goName];
        if (goQueue.Count >= config.size)
        {
            // 如果对象池已满则直接销毁
            Debug.Log("GOPool " + goName.ToString() + " is full!!!");
            Destroy(needRecycleGO);
            return;
        }

        needRecycleGO.SetActive(false);
        needRecycleGO.transform.parent = config.goParent;
        goQueue.Enqueue(needRecycleGO);
    }
}

/// <summary>
/// 对象池配置
/// </summary>
[System.Serializable]
public class GOPoolConfig
{
    public GOName goName;
    public GameObject goPrefab;
    public Transform goParent;
    public int size;
}

public enum GOName
{
    NONE,
    ROUTE,
    TOWER_BASE,
    TURRENT_MACHINE_GUN,

    ENEMY_BLUE_SPHERE,
    ENEMY_RED_SPHERE,
    ENEMY_YELLOW_SPHERE,
}