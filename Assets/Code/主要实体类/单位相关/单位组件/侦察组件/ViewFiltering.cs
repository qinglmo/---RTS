using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface ITargetFiltering
{
    public List<IHasPosition> VisionFiltering(List<IHasPosition> targets, LayerMask obstacleLayer);
    public bool IsVisiableToOne(IHasPosition currentTarget, LayerMask obstacleLayer);
}

public class ViewFiltering :MonoBehaviour,ITargetFiltering
{
    /// <summary>
    /// 过滤掉看不到的敌人
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public List<IHasPosition> VisionFiltering(List<IHasPosition> targets, LayerMask obstacleLayer)
    {

        Vector2 startPosition = transform.position;
        var targetList = new List<IHasPosition>();
        foreach (var target in targets)
        {
            if (IsVisiable(startPosition, target.Position, obstacleLayer))
            {
                targetList.Add(target);
            }
        }
        return targetList;

    }
    /// <summary>
    /// 检查当前敌人是否在视野内
    /// </summary>
    /// <param name=""></param>
    /// <param name="obstacleLayer"></param>
    /// <returns></returns>
    public bool IsVisiableToOne(IHasPosition currentTarget, LayerMask obstacleLayer)
    {
        if(currentTarget as MonoBehaviour ==null)
            return false;
        Vector2 startPosition = transform.position;
        return IsVisiable(startPosition,currentTarget.Position, obstacleLayer);
    }
    private bool IsVisiable(Vector2 startPosition, Vector2 targetPosition, LayerMask obstacleLayer)
    {
        // 计算方向与距离
        Vector2 direction = targetPosition - startPosition;
        float distance = direction.magnitude - 1.3f;//需要排除目标

        // 如果距离为0，直接返回true（已在目标点）
        if (distance <= 0f)
            return true;

        // 发射2D射线，只检测 obstacleLayer 指定的层
        RaycastHit2D hit = Physics2D.Raycast(startPosition, direction, distance, obstacleLayer);

        // 如果没有碰到任何建筑/遮挡物，返回true；否则返回false
        return hit.collider == null;
    }
}

