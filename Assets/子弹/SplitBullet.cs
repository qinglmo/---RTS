using System.Collections.Generic;
using UnityEngine;

public interface IBullet
{

}
public class SplitBullet : MonoBehaviour,IHasFaction
{
    public Faction Faction { get; set; }

    [Header("飞行参数")]
    public float speed = 10f;
    public float lifetime = 5f;          // 最长存活时间

    [Header("分裂参数")]
    public int fragmentCount = 8;         // 分裂出的子弹数量
    public GameObject fragmentPrefab;      // 普通子弹预制体（需有 Bullet 组件）

    [Header("爆炸效果（可选）")]
    public GameObject explodeVFX;          // 爆炸特效

    [Header("伤害属性")]
    public int damage = 1;                 // 直接命中伤害
    public float impact = 0f;              // 直接命中冲击力

    private Vector3 targetPosition;
    private bool hasExploded = false;

    private Vector2 direction;
    /// <summary>
    /// 初始化子弹：设置目标点和发射者
    /// </summary>
    /// <param name="target">目标世界坐标</param>
    /// <param name="owner">发射者对象</param>
    public void Initialize(Vector3 target, Faction faction)
    {
        targetPosition = target;
        Faction = faction;
        Destroy(gameObject, lifetime); // 超时自动销毁（不爆炸，可根据需求调整）
    }
    public void Initialize(Vector2 dir, int damage, Faction faction)
    {

        direction = dir.normalized;
        this.damage = damage;
        Faction = faction;
        Destroy(gameObject, lifetime);
    }
    private void Update()
    {
        if (hasExploded) return;

        // 向目标移动
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // 检测是否到达目标点（距离小于阈值）
        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;
        var unit = other.GetComponent<IFactionMember>();
        if (unit == null)
        {
            Destroy(gameObject);
            return;
        }
        if (unit.Faction == Faction) return;

        if (unit as BaseUnit)
        {
            BaseUnit unit2 = unit as BaseUnit;
            unit2.attributes.TakeDamage(damage, impact);
        }
        if (unit as BaseBuilding)
        {
            other.GetComponent<BaseBuilding>()?.TakeDamage(damage);
        }
        Explode();
        Destroy(gameObject);

    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // 播放特效
        if (explodeVFX != null)
        {
            Instantiate(explodeVFX, transform.position, Quaternion.identity);
        }

        // 生成分裂子弹
        SpawnFragments();

        // 销毁自身
        Destroy(gameObject);
    }

    /// <summary>
    /// 生成分裂子弹，沿圆形均匀分布
    /// </summary>
    private void SpawnFragments()
    {
        if (fragmentPrefab == null || fragmentCount <= 0) return;

        float angleStep = 360f / fragmentCount;
        Vector2 baseDir = Vector2.right; // 起始方向，可自定义

        for (int i = 0; i < fragmentCount; i++)
        {
            float angle = i * angleStep;
            Vector2 fragmentDir = Quaternion.Euler(0, 0, angle) * baseDir;

            GameObject fragObj = Instantiate(fragmentPrefab, transform.position, Quaternion.identity);
            Bullet frag = fragObj.GetComponent<Bullet>();
            if (frag != null)
            {
                // 将原发射者标签传给碎片子弹，确保碎片不会伤害发射者
                frag.Initialize(fragmentDir, Faction);
            }
            else
            {
                Debug.LogError("Fragment prefab missing Bullet component!");
                Destroy(fragObj);
            }
        }
    }
}