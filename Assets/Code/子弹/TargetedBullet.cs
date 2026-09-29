using UnityEngine;

public class TargetedBullet : BulletBase
{
    public float lifetime = 5f;
    public float arriveThreshold = 0.2f;

    private Vector3 targetPosition;
    private bool hasArrived = false;

    // 用于兼容方向初始化（虚拟目标）
    private bool useVirtualTarget = false;
    private Vector2 direction;

    // 目标点初始化
    public void Initialize(Vector3 target, Faction faction)
    {
        targetPosition = target;
        Faction = faction;
        Destroy(gameObject, lifetime);
    }

    // 方向初始化（兼容原 SplitBullet 的方向重载）
    public override void Initialize(Vector2 dir, int damage, Faction faction)
    {
        useVirtualTarget = true;
        direction = dir.normalized;
        this.damage = damage;
        Faction = faction;
        targetPosition = transform.position + (Vector3)(direction * 1000f);
        Destroy(gameObject, lifetime);
    }

    public override void Initialize(Vector2 dir, Faction faction)
    {
        Initialize(dir, damage, faction);
    }

    protected override void Move()
    {
        if (hasArrived) return;

        Vector3 dir = (targetPosition - transform.position).normalized;
        transform.Translate(dir * speed * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, targetPosition) < arriveThreshold)
        {
            OnArrive();
        }
    }

    protected virtual void OnArrive()
    {
        if (hasArrived) return;
        hasArrived = true;

        foreach (var effect in effects)
        {
            effect.OnHit(this, null); // 到达时无碰撞目标
        }

        Destroy(gameObject);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (hasArrived) return;
        base.OnTriggerEnter2D(other);
    }
}