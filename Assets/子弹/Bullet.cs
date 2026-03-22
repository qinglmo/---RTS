using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour,IHasFaction
{
    public Faction Faction { get; set; }

    public float speed = 10f;
    public float lifetime = 3f;
    public int damage = 1;
    public float impact = 0.2f;

    private Vector2 direction;

    private Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // 通过发射者初始化（用于分裂子弹传递原发射者）
    public void Initialize(Vector2 dir, Faction faction)
    {

        direction = dir.normalized;
        Faction = faction;
        Destroy(gameObject, lifetime);
    }
    public void Initialize(Vector2 dir,int damage, Faction faction)
    {

        direction = dir.normalized;
        this.damage = damage;
        Faction = faction;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        rb.velocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var unit=other.GetComponent<IFactionMember>();
        if (unit == null)
        {
            Destroy(gameObject);
            return;
        }
        if (unit.Faction==Faction) return;

        if(unit as BaseUnit)
        {
            BaseUnit unit2 = unit as BaseUnit;
            unit2.attributes.TakeDamage(damage, impact);
        }
        if(unit as BaseBuilding)
        {
            other.GetComponent<BaseBuilding>()?.TakeDamage(damage);
        }
        
        Destroy(gameObject);
    }
}
