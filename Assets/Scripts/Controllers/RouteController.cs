using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteController : MonoBehaviour
{
    // 设置路径的宽高
    public void SetRouteSize(float x, float y, float z)
    {
        transform.localScale = new Vector3(x, y, z);
    }

    // 设置路径的位置
    public void SetRoutePos(Vector3 position)
    {
        transform.position = position;
    }
}
