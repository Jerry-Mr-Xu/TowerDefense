using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoordinateUtil
{
    /// <summary>
    /// 获取任意坐标的地面坐标，即去掉y轴
    /// </summary>
    /// <param name="originCoordinate">任意坐标</param>
    public static Vector3 GetGroundCoordinate(Vector3 originCoordinate)
    {
        return new Vector3(originCoordinate.x, 0, originCoordinate.z);
    }
}
