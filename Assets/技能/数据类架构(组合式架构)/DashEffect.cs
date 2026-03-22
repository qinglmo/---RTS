using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

//[CreateAssetMenu(fileName = "DashEffect", menuName = "Skills/Effects/Dash")]
//public class DashEffect : SkillEffect
//{
//    public int damage;
//    public float impact;
//    public override void Execute(BaseUnit caster, Vector2Int targetCell)
//    {
//        caster.StartCoroutine(DashCoroutine(caster, targetCell));
//    }
//    private System.Collections.IEnumerator DashCoroutine(BaseUnit caster, Vector2Int targetCell)
//    {
//        // 实现冲刺逻辑
//        // 获取英雄当前格子位置
//        Vector2Int startPos = caster.movement.stepMover.CurrentPos;

//        // 确定冲刺方向（必须是水平或垂直）
//        Vector2Int direction = Vector2Int.zero;
//        if (targetCell.x == startPos.x)
//            direction = new Vector2Int(0, targetCell.y > startPos.y ? 1 : -1);
//        else if (targetCell.y == startPos.y)
//            direction = new Vector2Int(targetCell.x > startPos.x ? 1 : -1, 0);
//        else
//            yield break; // 非直线，退出

//        // 生成路径格子列表（从起点之后到目标格子，包括目标）
//        List<Vector2Int> pathCells = new List<Vector2Int>();
//        Vector2Int current = startPos + direction;
//        while (true)
//        {
//            pathCells.Add(current);
//            if (current == targetCell)
//                break;
//            current += direction;
//        }

//        // 记录已经处理过的格子（避免重复击退）
//        HashSet<Vector2Int> processedCells = new HashSet<Vector2Int>();

//        // 起点本身不触发击退
//        processedCells.Add(startPos);

//        // 冲刺总距离
//        float totalDistance = Vector2.Distance(startPos, targetCell);
//        float dashSpeed = 10f; // 可根据需要调整
//        float elapsed = 0f;

//        Vector3 startWorld = caster.transform.position;
//        Vector3 targetWorld = (Vector3)(Vector2)targetCell;

//        while (elapsed < totalDistance / dashSpeed)
//        {
//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed * dashSpeed / totalDistance);
//            caster.transform.position = Vector3.Lerp(startWorld, targetWorld, t);

//            // 获取当前所处的格子（基于世界坐标取整）
//            Vector2Int currentCell = GridManager.Instance.SnapToGrid(caster.transform.position);

//            // 如果当前格子未被处理且位于路径上，则触发击退
//            if (!processedCells.Contains(currentCell) && pathCells.Contains(currentCell))
//            {
//                processedCells.Add(currentCell);
//                BaseUnit targetUnit = GridManager.Instance.GetUnit(currentCell);
//                if (targetUnit != null)
//                {
//                    var attributes= targetUnit.gameObject.GetComponent<UnitAttributes>();
//                    attributes.TakeDamage(damage,impact);
//                    // 尝试击退，如果失败则停止冲刺
//                    bool success = targetUnit.movement.stepMover.TryShiftFromDirection(direction);
//                    if (!success)
//                    {
//                        attributes.TakeDamage(damage, impact);
//                        // 击退失败，英雄停在当前格子（注意要调整位置）
//                        // 可以将 finalPos 设为 currentCell，然后退出循环
//                        break;
//                    }
//                }
//            }

//            yield return null;
//        }

//        // 确保最终位置精确
//        caster.transform.position = targetWorld;
//        caster.movement.stepMover.TeleportToGrid(targetCell);
//        Debug.Log($"{name} 冲刺到 {targetCell}");
//    }
//}
