using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基础属性控件
/// </summary>
public interface BaseProperty
{
    /// <summary>
    /// 重置属性值
    /// </summary>
    void ResetValue();

    /// <summary>
    /// 清除所有观察者
    /// </summary>
    void ClearObserver();
}
