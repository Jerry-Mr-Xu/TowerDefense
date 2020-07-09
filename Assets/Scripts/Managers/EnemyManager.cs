using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static WaveConfig;
using static WaveConfig.EnemyGroupConfig;

public class EnemyManager : MonoBehaviour
{
    // 敌人的父容器
    [SerializeField]
    private Transform enemyParent;

    // 敌人链表
    [HideInInspector]
    public LinkedList<EnemyController> enemyLinkedList = new LinkedList<EnemyController>();

    private GOPoolManager goPoolManager;
    private MapBuilder mapBuilder;

    // 单例
    public static EnemyManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        goPoolManager = GOPoolManager.Instance;
        mapBuilder = MapBuilder.Instance;
    }

    /// <summary>
    /// 生成波次
    /// </summary>
    /// <param name="waveConfig">波次信息</param>
    public IEnumerator SpawnWave(WaveConfig waveConfig)
    {
        Debug.Log("Wave No." + waveConfig.index + " spawn!");
        EnemyGroupConfig[] enemyGroupConfigArray = waveConfig.enemyConfigArray;
        int enemyGroupCount = enemyGroupConfigArray.Length;
        for (int i = 0; i < enemyGroupCount; i++)
        {
            EnemyGroupConfig curEnemyGroupConfig = enemyGroupConfigArray[i];
            curEnemyGroupConfig.index = i;
            if (i + 1 < enemyGroupCount && enemyGroupConfigArray[i + 1].startTiming == StartTime.START_AFTER_PREVIOUS)
            {
                // 如果有下个敌人小队，并且下个小队是在当前小队生成完成后开始的
                yield return StartCoroutine(SpawnEnemyGroup(curEnemyGroupConfig));
            }
            else
            {
                // 如果没有下个敌人小队，或者下个小队是在当前小队生成时就开始的
                StartCoroutine(SpawnEnemyGroup(curEnemyGroupConfig));
            }
        }
    }

    /// <summary>
    /// 生成敌人小队
    /// </summary>
    /// <param name="enemyGroupConfig">敌人小队信息</param>
    private IEnumerator SpawnEnemyGroup(EnemyGroupConfig enemyGroupConfig)
    {
        Debug.Log("EnemyGroup No." + enemyGroupConfig.index + " start!");
        // 开始延迟
        yield return new WaitForSeconds(enemyGroupConfig.startDelay);

        Debug.Log("EnemyGroup No." + enemyGroupConfig.index + " spawn!");
        // 生成敌人
        for (int i = 0; i < enemyGroupConfig.enemyCount; i++)
        {
            Debug.Log("Enemy: " + enemyGroupConfig.enemyName.ToString() + " spawn!");
            Vector3 spawnPosition = mapBuilder.waypointCoordinateArray[0];
            Quaternion spawnRotation = Quaternion.LookRotation(mapBuilder.waypointCoordinateArray[1] - spawnPosition);
            EnemyController enemyController = goPoolManager.GetGO(enemyGroupConfig.enemyName, spawnPosition, spawnRotation, enemyParent).GetComponent<EnemyController>();
            enemyController.SetName(enemyGroupConfig.enemyName);
            enemyController.OnSelfDead += OnEnemyDead;
            enemyLinkedList.AddLast(enemyController);
            yield return new WaitForSeconds(enemyGroupConfig.spawnInterval);
        }
    }

    /// <summary>
    /// 当敌人死亡时
    /// </summary>
    /// <param name="enemyController">敌人</param>
    public void OnEnemyDead(EnemyController enemyController)
    {
        enemyController.OnSelfDead -= OnEnemyDead;
        enemyLinkedList.Remove(enemyController);
    }
}