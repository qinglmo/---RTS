using UnityEngine;

[CreateAssetMenu(fileName = "SplitBulletEffect", menuName = "Skills/Effects/SplitBullet")]
public class SplitBulletEffect : SkillEffect
{
    [Header("子弹预制体")]
    public GameObject splitBulletPrefab; // 需要挂载 SplitBulletProjectile 脚本

    public override void Execute(BaseUnit caster, Vector2Int targetCell)
    {
        if (splitBulletPrefab == null)
        {
            Debug.LogError("SplitBulletPrefab is missing!");
            return;
        }

        // 获取发射点（例如单位位置）
        Vector3 firePos = caster.transform.position;
        Vector3 targetPos = GridManager.Instance.CellToWorld(targetCell);

        // 实例化子弹
        GameObject bulletObj = Object.Instantiate(splitBulletPrefab, firePos, Quaternion.identity);
        SplitBullet bullet = bulletObj.GetComponent<SplitBullet>();
        if (bullet != null)
        {
            bullet.Initialize(targetPos, caster.Faction);
        }
    }
}