using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 波次信息数组
    [SerializeField]
    private WaveConfig[] waves;

    [HideInInspector]
    // 地图每一个坐标点是什么
    public GameObject[,] nodeGrid;
    // 波次
    private int waveIndex = 0;

    private EnemyManager enemyManager;

    // 单例
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        enemyManager = EnemyManager.Instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 按退出键退出游戏
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (waveIndex < waves.Length)
            {
                // 按空格键开始波次
                StartCoroutine(StartWave(waveIndex));
                waveIndex++;
            }
        }
    }

    /// <summary>
    /// 开始波次
    /// </summary>
    /// <param name="waveIndex">波次序号</param>
    public IEnumerator StartWave(int waveIndex)
    {
        WaveConfig waveConfig = waves[waveIndex];
        waveConfig.index = waveIndex;
        Debug.Log("Wave No." + waveIndex + " start!");
        yield return new WaitForSeconds(waveConfig.startDelay);
        StartCoroutine(enemyManager.SpawnWave(waveConfig));
    }
}

// 波次信息
[System.Serializable]
public class WaveConfig
{
    [HideInInspector]
    public int index;
    // 波次开始延迟时间（单位:s）
    public float startDelay;
    // 敌人小队信息数组
    public EnemyGroupConfig[] enemyConfigArray;

    // 敌人小队信息
    [System.Serializable]
    public class EnemyGroupConfig
    {
        [HideInInspector]
        public int index;
        // 敌人名称
        public GOName enemyName;
        // 敌人数量
        public int enemyCount;
        // 敌人生成间隔
        public float spawnInterval;
        // 开始延迟时间（单位:s）
        public float startDelay;
        // 开始时机
        public StartTime startTiming;

        public enum StartTime
        {
            START_WITH_PREVIOUS,
            START_AFTER_PREVIOUS,
        }
    }
}
