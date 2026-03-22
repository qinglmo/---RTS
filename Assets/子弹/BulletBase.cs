using System.Collections.Generic;
using UnityEngine;

public abstract class BulletBase : MonoBehaviour, IHasFaction
{
    public Faction Faction { get; set; }

    [Header("Base Bullet Settings")]
    public float speed = 10f;
    public int damage = 1;
    public float impact = 0.2f;

    protected List<IBulletEffect> effects;

    protected virtual void Awake()
    {
        effects = new List<IBulletEffect>(GetComponents<IBulletEffect>());
    }

    protected abstract void Move();

    protected virtual void Update()
    {
        Move();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        var unit = other.GetComponent<IHasFaction>();
        if (unit == null)
        {
            Destroy(gameObject);
            return;
        }
        if (unit.Faction == Faction) return;

        DealDamage(unit);

        foreach (var effect in effects)
        {
            effect.OnHit(this, other);
        }

        Destroy(gameObject);
    }

    protected virtual void DealDamage(IHasFaction target)
    {
        if (target is BaseUnit unit)
            unit.attributes.TakeDamage(damage, impact);
        else if (target is BaseBuilding building)
            building.TakeDamage(damage);
    }

    // 供外部调用的初始化方法（方向模式）
    public abstract void Initialize(Vector2 dir, Faction faction);
    public abstract void Initialize(Vector2 dir, int damage, Faction faction);
}