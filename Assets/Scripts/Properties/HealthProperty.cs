using System;
using UnityEngine;

public class HealthProperty : MonoBehaviour, BaseProperty
{
    // 最大血量
    public float maxHealth = 100;

    // 当前血量
    public float curHealth { get; private set; }
    // 当前血量百分比
    public float curHealthPercent { get; private set; }

    public event Action<float> OnHealthChanged;
    public event Action OnDead;

    private void Awake()
    {
        ResetValue();
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void OnDamage(float damage)
    {
        curHealth -= damage;
        if (curHealth <= 0)
        {
            curHealth = 0;
            OnDead?.Invoke();
        }
        curHealthPercent = curHealth / maxHealth;
        OnHealthChanged?.Invoke(curHealthPercent);
    }

    /// <summary>
    /// 受到治疗
    /// </summary>
    /// <param name="heal">治疗值</param>
    public void OnHeal(float heal)
    {
        curHealth = Math.Min(maxHealth, curHealth + heal);
        curHealthPercent = curHealth / maxHealth;
        OnHealthChanged?.Invoke(curHealthPercent);
    }

    public void ResetValue()
    {
        curHealth = maxHealth;
        curHealthPercent = 1;
    }

    public void ClearObserver()
    {
        OnHealthChanged = null;
        OnDead = null;
    }
}
