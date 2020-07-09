using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurrentController : MonoBehaviour
{
    [SerializeField]
    // 攻击距离
    private float attackRange = 5;
    [SerializeField]
    // 关注的敌人属性
    private PropertyName focusEnemyProperty = PropertyName.MOVE_PROGRESS;
    [SerializeField]
    // 攻击伤害
    private float attackDamage = 5;
    [SerializeField]
    // 攻击频率（单位：次每秒）
    private float attackRate = 3;
    [SerializeField]
    // 旋转速度
    private float rotateSpeed = 10;
    [Space]
    [SerializeField]
    // 多久后复位
    private float resetAfterLastFire = 1f;
    [SerializeField]
    // 开火效果持续时间
    private float fireEffectDisplayTime = 0.03f;

    [HideInInspector]
    public GOName turrentName;
    private Transform turrentHead;
    // 火线
    private LineRenderer fireLine;
    // 火光
    private Light fireLight;

    // 从上次开火开始到现在等待的时间
    private float waitTimeAfterLastFire = 0;
    // 目标敌人
    private EnemyController targetEnemy;

    private EnemyManager enemyManager;

    private void Awake()
    {
        enemyManager = EnemyManager.Instance;

        turrentHead = transform.GetChild(1);

        fireLine = turrentHead.GetChild(0).GetComponent<LineRenderer>();
        fireLight = fireLine.GetComponent<Light>();
    }

    private void Start()
    {
        InitParams();
    }

    /// <summary>
    /// 初始化参数
    /// </summary>
    private void InitParams()
    {
        DisableFireEffect();
        turrentHead.rotation = Quaternion.LookRotation(Vector3.forward);

        waitTimeAfterLastFire = 0;
        targetEnemy = null;
    }

    private void Update()
    {
        FindTargetEnemy();

        if (targetEnemy != null)
        {
            LookAtTarget();
        }
        else if (waitTimeAfterLastFire >= resetAfterLastFire)
        {
            ResetRotation();
        }
        if (IsReadyToFire())
        {
            OpenFire();
        }
        if (waitTimeAfterLastFire >= fireEffectDisplayTime)
        {
            DisableFireEffect();
        }
    }

    /// <summary>
    /// 看向目标敌人
    /// </summary>
    private void LookAtTarget()
    {
        Quaternion resultRotation = Quaternion.LookRotation(targetEnemy.GetBodyCoordinate() - turrentHead.position);
        turrentHead.rotation = Quaternion.Lerp(turrentHead.rotation, resultRotation, Time.deltaTime * rotateSpeed);
    }

    /// <summary>
    /// 转向复位
    /// </summary>
    private void ResetRotation()
    {
        Quaternion resultRotation = Quaternion.LookRotation(Vector3.forward);
        turrentHead.rotation = Quaternion.Lerp(turrentHead.rotation, resultRotation, Time.deltaTime * rotateSpeed);
    }

    /// <summary>
    /// 是否可以开火
    /// </summary>
    private bool IsReadyToFire()
    {
        waitTimeAfterLastFire += Time.deltaTime;
        if (waitTimeAfterLastFire >= 1 / attackRate && targetEnemy != null)
        {
            // 如果等待时间超过了开火间隔并且有目标，则可以开火
            waitTimeAfterLastFire = 0;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 开火
    /// </summary>
    private void OpenFire()
    {
        EnableFireEffect();
        targetEnemy.TakeDamage(attackDamage);
    }

    /// <summary>
    /// 打开开火效果
    /// </summary>
    private void EnableFireEffect()
    {
        fireLine.SetPosition(0, fireLine.transform.position);
        fireLine.SetPosition(1, targetEnemy.GetBodyCoordinate());
        fireLine.enabled = true;

        fireLight.enabled = true;
    }

    /// <summary>
    /// 关闭开火效果
    /// </summary>
    private void DisableFireEffect()
    {
        fireLine.enabled = false;
        fireLight.enabled = false;
    }

    /// <summary>
    /// 寻找目标敌人
    /// </summary>
    private void FindTargetEnemy()
    {
        targetEnemy = null;
        // 记录所关注属性的最大值
        float focusPropertyMaxValue = 0;
        Vector3 selfGroundCoordinate = CoordinateUtil.GetGroundCoordinate(transform.position);
        foreach (EnemyController enemy in enemyManager.enemyLinkedList)
        {
            Vector3 enemyGroundCoordinate = CoordinateUtil.GetGroundCoordinate(enemy.transform.position);
            if (Vector3.Distance(selfGroundCoordinate, enemyGroundCoordinate) < attackRange)
            {
                // 如果敌人在射程内
                float focusPropertyValue = enemy.GetProperty(focusEnemyProperty);
                if (focusPropertyValue > focusPropertyMaxValue)
                {
                    // 更新记录，更换目标
                    focusPropertyMaxValue = focusPropertyValue;
                    targetEnemy = enemy;
                }
            }
        }
    }

    /// <summary>
    /// 回收自身
    /// </summary>
    public void Recycle(GOPoolManager goPoolManager)
    {
        InitParams();
        goPoolManager.RecycleGO(turrentName, gameObject);
    }
}