using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : BaseUnit
{
    

    
    [Header("子弹设置")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    protected override void Awake()
    {
        base.Awake();
        // ... 初始化其他组件
    }

    protected override void Start()
    {
        base.Start();
        movement.OnMoveCompleted += OnMoveCompleted;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (movement != null) movement.OnMoveCompleted -= OnMoveCompleted;
    }
    public void MoveToGridPos(Vector2Int target)
    {
        commandMovePosition = new Vector2Int(target.x, target.y);
    }

    // 公有发射方法
    public void ShootAt(Vector2 target)
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("子弹预制体未设置！");
            return;
        }

        Vector2 shootPos = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        Vector2 direction = target - shootPos;

        GameObject bulletObj = Instantiate(bulletPrefab, shootPos, Quaternion.identity);
        BulletBase bullet = bulletObj.GetComponent<BulletBase>();
        if (bullet != null)
        {
            bullet.Initialize(direction, Faction);
        }
        else
        {
            Debug.LogError("子弹预制体缺少脚本！");
        }
    }

    // 移动完成回调
    private void OnMoveCompleted()
    {

    }

}
