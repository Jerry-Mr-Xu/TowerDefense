using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // 本身在对象池中的名字
    private GOName enemyName;

    // 生命属性
    private HealthProperty healthProperty;
    // 移动属性
    private MoveProperty moveProperty;
    private Transform body;

    private GOPoolManager goPoolManager;
    private MapBuilder mapBuilder;
    private EnemyManager enemyManager;

    // 自身死亡监听
    public event Action<EnemyController> OnSelfDead;

    private void Awake()
    {
        goPoolManager = GOPoolManager.Instance;
        mapBuilder = MapBuilder.Instance;
        enemyManager = EnemyManager.Instance;

        body = transform.GetChild(0);
        healthProperty = GetComponent<HealthProperty>();
        moveProperty = GetComponent<MoveProperty>();
    }

    private void OnEnable()
    {
        healthProperty.OnDead += Dead;
        moveProperty.OnMove += Move;
        moveProperty.OnRotate += Rotate;
        moveProperty.OnArriveFinalPoint += ArriveFinalPoint;

        moveProperty.SetRoute(mapBuilder.waypointCoordinateArray);
    }

    /// <summary>
    /// 移动
    /// </summary>
    /// <param name="position">移动到的位置</param>
    public void Move(Vector3 position)
    {
        transform.position = position;
    }

    /// <summary>
    /// 旋转
    /// </summary>
    /// <param name="rotation">旋转到的位置</param>
    public void Rotate(Quaternion rotation)
    {
        transform.rotation = rotation;
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(float damage)
    {
        healthProperty.OnDamage(damage);
    }

    /// <summary>
    /// 获取属性
    /// </summary>
    /// <param name="propertyName">属性名称</param>
    /// <returns>属性值</returns>
    public float GetProperty(PropertyName propertyName)
    {
        switch (propertyName)
        {
            case PropertyName.MOVE_PROGRESS:
                {
                    return moveProperty.progress;
                }
            case PropertyName.HEALTH_POINTS:
                {
                    return healthProperty.curHealth;
                }
            case PropertyName.MOVE_SPEED:
                {
                    return moveProperty.moveSpeed;
                }
            default:
                {
                    return 0;
                }
        }
    }

    /// <summary>
    /// 获取实体所在的坐标
    /// </summary>
    public Vector3 GetBodyCoordinate()
    {
        return body.transform.position;
    }

    /// <summary>
    /// 给当前敌人设置名字，用于对象池回收
    /// </summary>
    /// <param name="enemyName">敌人名字</param>
    public void SetName(GOName enemyName)
    {
        this.enemyName = enemyName;
    }

    /// <summary>
    /// 到达终点
    /// </summary>
    private void ArriveFinalPoint()
    {
        Dead();
    }

    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        OnSelfDead?.Invoke(this);
        goPoolManager.RecycleGO(enemyName, gameObject);
    }

    private void OnDisable()
    {
        ResetProperties();
    }

    /// <summary>
    /// 重置属性
    /// </summary>    
    private void ResetProperties()
    {
        healthProperty.ResetValue();
        healthProperty.ClearObserver();

        moveProperty.ResetValue();
        moveProperty.ClearObserver();
    }
}

/// <summary>
/// 属性名称
/// </summary>
public enum PropertyName
{
    // 移动进度
    MOVE_PROGRESS,
    // 血量
    HEALTH_POINTS,
    // 移速
    MOVE_SPEED,
}
