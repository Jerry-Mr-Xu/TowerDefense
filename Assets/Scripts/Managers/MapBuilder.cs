using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// 地图生成器
/// </summary>
public class MapBuilder : MonoBehaviour
{
    // 正在使用中塔和路径的父容器
    [SerializeField]
    private Transform towerParent, routeParent;

    [Space]
    // 地图宽高
    public int colCount = 10;
    public int rowCount = 10;
    public float mapThickness = 0.2f;
    // 塔基宽高
    public float cellWidth = 1, cellHeight = 1;
    // 水平垂直间距
    public float horSpace = 0.5f, verSpace = 0.5f;
    [Space]
    // 路径点坐标
    public Vector2Int[] waypointPosArray;

    private GOPoolManager goPoolManager;
    private GameManager gameManager;

    public Vector3[] waypointCoordinateArray { get; private set; }

    public static MapBuilder Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        goPoolManager = GOPoolManager.Instance;

        waypointCoordinateArray = new Vector3[waypointPosArray.Length];
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        BuildMap();
    }

    /// <summary>
    /// 生成地图
    /// </summary>
    private void BuildMap()
    {
        gameManager.nodeGrid = new GameObject[colCount, rowCount];

        // 生成怪物路径
        BuildRoute();

        // 生成塔基
        BuildTowerBase();
    }

    /// <summary>
    /// 生成路径
    /// </summary>
    private void BuildRoute()
    {
        if (waypointPosArray.Length < 2)
        {
            Debug.LogError("waypoint count is less then 2!!");
            return;
        }

        // 初始化起点和终点
        Vector2Int startWaypointPos = waypointPosArray[0];
        Vector2Int finalWaypointPos = waypointPosArray[waypointPosArray.Length - 1];

        waypointCoordinateArray[0] = GetCenterCoordinateByPos(startWaypointPos);

        // 判断起点及终点合法性
        if (startWaypointPos.x != 0 && startWaypointPos.x != colCount - 1 && startWaypointPos.y != 0 && startWaypointPos.y != rowCount - 1)
        {
            Debug.LogError("StartWaypoint: " + startWaypointPos + " is invalid!");
            return;
        }
        if (finalWaypointPos.x != 0 && finalWaypointPos.x != colCount - 1 && finalWaypointPos.y != 0 && finalWaypointPos.y != rowCount - 1)
        {
            Debug.LogError("FinalWaypoint: " + finalWaypointPos + " is invalid!");
            return;
        }

        // 遍历所有路径点来生成路径
        for (int i = 1; i < waypointPosArray.Length; i++)
        {
            Vector2Int endWaypointPos = waypointPosArray[i];
            waypointCoordinateArray[i] = GetCenterCoordinateByPos(endWaypointPos);

            if (endWaypointPos.x < 0 || endWaypointPos.x >= colCount || endWaypointPos.y < 0 || endWaypointPos.y >= rowCount)
            {
                // 路径点超出地图范围
                Debug.LogError("Waypoint: " + endWaypointPos + " is out of map range!");
                return;
            }
            if (startWaypointPos.x != endWaypointPos.x && startWaypointPos.y != endWaypointPos.y)
            {
                // 路径点非水平或者垂直
                Debug.LogError("Waypoint1: " + endWaypointPos + " with Waypoint2: " + startWaypointPos + " is neither same column nor same row!");
                return;
            }

            if (startWaypointPos.Equals(endWaypointPos))
            {
                // 如果有两个路径点坐标相同则跳过
                startWaypointPos = endWaypointPos;
                continue;
            }

            // 生成路径
            // 是否最后一条路径
            Boolean isFinalRoute = endWaypointPos == waypointPosArray[waypointPosArray.Length - 1];
            // 是否横向路径
            Boolean isHorizontalRoute = startWaypointPos.y == endWaypointPos.y;
            // 按照离原点距离由近及远排序后的路径点
            Vector2Int[] afterSortPosArray = SortByOriginDistance(startWaypointPos, endWaypointPos);
            // 路径GO
            RouteController routeController = goPoolManager.GetGO(GOName.ROUTE, routeParent).GetComponent<RouteController>();
            if (isHorizontalRoute)
            {
                // 是否从左到右的路径
                Boolean isLeftToRight = startWaypointPos.x < endWaypointPos.x;
                // 如果是从右往左并且不是最后一条路径则往右移一个塔基的宽度，避免路径重叠
                routeController.SetRoutePos(GetRealCoordinateByPos(afterSortPosArray[0]) + (isLeftToRight || isFinalRoute ? Vector3.zero : Vector3.right * cellWidth));

                // 如果是最后一条路径则路径宽度增加一个塔基宽度，因为之前路径都是包括起点不包括终点但最后路径既包括起点也包括终点
                float routeWidth = (afterSortPosArray[1].x - afterSortPosArray[0].x) * (cellWidth + horSpace) + (isFinalRoute ? cellWidth : 0);
                routeController.SetRouteSize(routeWidth, mapThickness, cellHeight);
            }
            else
            {
                // 是否从下到上的路径
                Boolean isBottomToTop = startWaypointPos.y < endWaypointPos.y;
                // 如果是从上往下并且不是最后一条路径则往上移一个塔基的高度，避免路径重叠
                routeController.SetRoutePos(GetRealCoordinateByPos(afterSortPosArray[0]) + (isBottomToTop || isFinalRoute ? Vector3.zero : Vector3.up * cellHeight));

                // 如果是最后一条路径则路径高度增加一个塔基高度，因为之前路径都是包括起点不包括终点但最后路径既包括起点也包括终点
                float routeHeight = (afterSortPosArray[1].y - afterSortPosArray[0].y) * (cellHeight + verSpace) + (isFinalRoute ? cellHeight : 0);
                routeController.SetRouteSize(cellWidth, mapThickness, routeHeight);
            }

            // 设置地图坐标
            if (isHorizontalRoute)
            {
                for (int x = afterSortPosArray[0].x; x <= afterSortPosArray[1].x; x++)
                {
                    gameManager.nodeGrid[x, endWaypointPos.y] = routeController.gameObject;
                }
            }
            else
            {
                for (int y = afterSortPosArray[0].y; y <= afterSortPosArray[1].y; y++)
                {
                    gameManager.nodeGrid[endWaypointPos.x, y] = routeController.gameObject;
                }
            }

            startWaypointPos = endWaypointPos;
        }
    }

    /// <summary>
    /// 生成塔基
    /// </summary>
    private void BuildTowerBase()
    {
        for (int i = 0; i < colCount; i++)
        {
            for (int j = 0; j < rowCount; j++)
            {
                // 遍历生成塔基
                if (gameManager.nodeGrid[i, j] == null)
                {
                    // 如果这个位置没有被占据则生成塔基
                    TowerController towerController = goPoolManager.GetGO(GOName.TOWER_BASE, GetRealCoordinateByPos(i, j), Quaternion.identity, towerParent).GetComponent<TowerController>();
                    towerController.SetTowerBaseSize(cellWidth, mapThickness, cellHeight);
                    gameManager.nodeGrid[i, j] = towerController.gameObject;

                }
            }
        }
    }

    /// <summary>
    /// 通过虚拟位置获取原子中间世界坐标
    /// </summary>
    /// <param name="pos">虚拟位置</param>
    /// <returns>原子中间世界坐标</returns>
    private Vector3 GetCenterCoordinateByPos(Vector2Int pos)
    {
        return GetRealCoordinateByPos(pos.x, pos.y) + Vector3.forward * cellHeight / 2 + Vector3.right * cellWidth / 2;
    }

    /// <summary>
    /// 通过虚拟位置获取真实世界坐标
    /// </summary>
    /// <param name="pos">虚拟位置</param>
    /// <returns>真实世界坐标</returns>
    private Vector3 GetRealCoordinateByPos(Vector2Int pos)
    {
        return GetRealCoordinateByPos(pos.x, pos.y);
    }

    /// <summary>
    /// 通过虚拟位置获取真实世界坐标
    /// </summary>
    /// <param name="x">虚拟横向位置</param>
    /// <param name="y">虚拟纵向位置</param>
    /// <returns>真实世界坐标</returns>
    private Vector3 GetRealCoordinateByPos(int x, int y)
    {
        // 地图真实宽高的一半
        float halfMapRealWidth = (colCount * cellWidth + (colCount - 1) * horSpace) / 2;
        float halfMapRealHeight = (rowCount * cellHeight + (rowCount - 1) * verSpace) / 2;

        return new Vector3(x * (cellWidth + horSpace) - halfMapRealWidth, 0, y * (cellHeight + verSpace) - halfMapRealHeight);
    }

    /// <summary>
    /// 根据离原点的距离排列坐标数组
    /// </summary>
    /// <param name="sourcePosArray">源坐标数组</param>
    /// <returns>距原点由近及远排列好的坐标数组</returns>
    private Vector2Int[] SortByOriginDistance(params Vector2Int[] sourcePosArray)
    {
        // 先拷贝一份防止改变源数据
        Vector2Int[] afterSortArray = new Vector2Int[sourcePosArray.Length];
        sourcePosArray.CopyTo(afterSortArray, 0);

        // 根据离原点的距离排序
        Array.Sort<Vector2Int>(afterSortArray, (first, second) => first.x + first.y - second.x - second.y);
        return afterSortArray;
    }
}
