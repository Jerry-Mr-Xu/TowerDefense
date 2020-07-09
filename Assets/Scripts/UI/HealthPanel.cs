using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPanel : MonoBehaviour
{
    private RectTransform healthPanel;
    private RectTransform healthFill;
    private HealthProperty healthProperty;
    private Transform pivot;

    private void Awake()
    {
        healthPanel = GetComponent<RectTransform>();
        healthFill = transform.GetChild(1).GetComponent<RectTransform>();
        healthProperty = GetComponentInParent<HealthProperty>();
        pivot = transform.parent;
    }

    private void OnEnable()
    {
        healthProperty.OnHealthChanged += OnHealthPercentChanged;
        OnHealthPercentChanged(healthProperty.curHealthPercent);
    }

    /// <summary>
    /// 当生命值百分比改变的时候
    /// </summary>
    /// <param name="healthPercent">生命值百分比</param>
    public void OnHealthPercentChanged(float healthPercent)
    {
        healthFill.localScale = new Vector3(healthPercent, 1, 1);
    }

    private void Update()
    {
        pivot.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }

    private void OnDisable()
    {
        healthProperty.OnHealthChanged -= OnHealthPercentChanged;
    }
}
