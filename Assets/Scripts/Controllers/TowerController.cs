using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cakeslice;

public class TowerController : MonoBehaviour
{
    private Transform towerBase;
    // 塔基表面坐标
    private Transform towerBaseSurface;
    private Outline towerBaseOutline;

    private TurrentController turrent;

    private GameManager gameController;
    private GOPoolManager goPoolManager;

    private void Awake()
    {
        gameController = GameManager.Instance;
        goPoolManager = GOPoolManager.Instance;

        towerBase = transform.GetChild(0);
        towerBaseSurface = towerBase.GetChild(1);
        towerBaseOutline = towerBase.GetChild(0).GetComponent<Outline>();
    }

    /// <summary>
    /// 设置塔基大小
    /// </summary>
    public void SetTowerBaseSize(float x, float y, float z)
    {
        towerBase.transform.localScale = new Vector3(x, y, z);
    }

    private void OnMouseEnter()
    {
        if (turrent == null)
        {
            // 在没有塔的时候可选中
            changeSelectState(true);
        }
    }

    private void OnMouseExit()
    {
        changeSelectState(false);
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(0) && turrent == null)
        {
            // 点击左键并且目前没有塔则建塔
            BuildTurrent(GOName.TURRENT_MACHINE_GUN);
            // 建完塔后取消选中
            changeSelectState(false);
        }
        else if (Input.GetMouseButtonUp(1) && turrent != null)
        {
            // 点击右键并且目前有塔则回收塔
            RecycleTurrent();
            // 回收完塔后恢复选中
            changeSelectState(true);
        }
    }

    /// <summary>
    /// 建塔
    /// </summary>
    private void BuildTurrent(GOName turrentName)
    {
        turrent = goPoolManager.GetGO(turrentName, towerBaseSurface.position, Quaternion.identity, transform).GetComponent<TurrentController>();
        turrent.turrentName = turrentName;
    }

    /// <summary>
    /// 回收塔
    /// </summary>
    private void RecycleTurrent()
    {
        turrent.Recycle(goPoolManager);
        turrent = null;
    }

    /// <summary>
    /// 修改选中状态
    /// </summary>
    /// <param name="isSelected">是否被选中</param>
    private void changeSelectState(bool isSelected)
    {
        towerBaseOutline.enabled = isSelected;
    }
}