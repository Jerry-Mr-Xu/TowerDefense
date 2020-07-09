using System;
using UnityEngine;

public class MoveProperty : MonoBehaviour, BaseProperty
{
    // 移动速度
    public float moveSpeed = 1;

    // 在整条路径上前进的进度
    public float progress { get; private set; }
    // 当前位置
    public Vector3 curPosition { get; private set; }
    // 当前方向
    public Quaternion curRotation { get; private set; }

    // 路径点位置数组
    private Vector3[] waypointPosArray;
    // 开始和结束路径点坐标
    private Vector3 startPointPos, endPointPos;
    // 当前路径序号（从0开始）
    private int curRouteIndex;
    // 是否开始移动（设置完路径就开始）
    private bool isStartMove = false;

    // 移动监听
    public event Action<Vector3> OnMove;
    // 旋转监听
    public event Action<Quaternion> OnRotate;
    // 移动到终点监听
    public event Action OnArriveFinalPoint;

    private void Awake()
    {
        ResetValue();
    }

    /// <summary>
    /// 设置路径
    /// </summary>
    /// <param name="waypointPosArray">路径点数组</param>
    public void SetRoute(Vector3[] waypointPosArray)
    {
        this.waypointPosArray = waypointPosArray;
        if (waypointPosArray == null || waypointPosArray.Length < 2)
        {
            Debug.LogError("Route is null or only one waypoint!");
            return;
        }

        progress = 0;
        curPosition = startPointPos = waypointPosArray[0];
        curRotation = Quaternion.LookRotation(endPointPos - startPointPos);
        endPointPos = waypointPosArray[1];
        isStartMove = true;

        OnMove?.Invoke(curPosition);
        OnRotate?.Invoke(curRotation);
    }

    private void Update()
    {
        if (!isStartMove)
        {
            return;
        }

        // 当前前进方向
        Vector3 direction = (endPointPos - startPointPos).normalized;
        // 走完当前这一步所处的坐标
        Vector3 expectEndPos = curPosition + direction * moveSpeed * Time.deltaTime;
        // 是否走到当前这段路的终点
        bool isReachEndWaypoint = (expectEndPos - endPointPos).normalized.Equals(direction);
        if (isReachEndWaypoint)
        {
            // 如果超过路径终点则移动到路径终点
            expectEndPos = endPointPos;
            if (curRouteIndex == waypointPosArray.Length - 2)
            {
                // 如果到达的完整路径的终点
                OnArriveFinalPoint?.Invoke();
                return;
            }

            // 走到下一段路
            MoveToNextPath();
        }
        curPosition = expectEndPos;
        curRotation = Quaternion.LookRotation(direction);
        OnMove?.Invoke(curPosition);
        OnRotate?.Invoke(curRotation);

        CalculateProgress(expectEndPos);
    }

    /// <summary>
    /// 走到下一段路
    /// </summary>
    private void MoveToNextPath()
    {
        curRouteIndex++;

        startPointPos = waypointPosArray[curRouteIndex];
        endPointPos = waypointPosArray[curRouteIndex + 1];
    }

    /// <summary>
    /// 计算进度
    /// </summary>
    /// <param name="curPosition">当前位置</param>
    private void CalculateProgress(Vector3 curPosition)
    {
        // 当前路径的进度
        float curRouteProgress = (curPosition - startPointPos).magnitude / (endPointPos - startPointPos).magnitude;
        // 加上路径序号
        progress = curRouteProgress + curRouteIndex;
    }

    public void ResetValue()
    {
        progress = 0;
        curPosition = Vector3.zero;
        curRotation = Quaternion.identity;
        waypointPosArray = null;
        startPointPos = endPointPos = Vector3.zero;
        curRouteIndex = 0;
        isStartMove = false;
    }

    public void ClearObserver()
    {
        OnMove = null;
        OnRotate = null;
        OnArriveFinalPoint = null;
    }
}
