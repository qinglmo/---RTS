using UnityEngine;

public class DirectionalBullet : BulletBase
{
    public float lifetime = 3f;

    private Vector2 direction;
    private Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    public override void Initialize(Vector2 dir, Faction faction)
    {
        direction = dir.normalized;
        Faction = faction;
        Destroy(gameObject, lifetime);
    }

    public override void Initialize(Vector2 dir, int damage, Faction faction)
    {
        direction = dir.normalized;
        this.damage = damage;
        Faction = faction;
        Destroy(gameObject, lifetime);
    }

    protected override void Move()
    {
        if (rb != null)
            rb.velocity = direction * speed;
        else
            transform.Translate(direction * speed * Time.deltaTime);
    }
}