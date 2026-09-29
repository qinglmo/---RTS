using UnityEngine;

public class PhysicalWaterSurface : MonoBehaviour
{
    [Header("血量")]
    [Range(0f, 1f)]
    public float health = 1f;

    [Header("物理参数")]
    public int pointCount = 32;
    public float springConstant = 100f;   // 恢复力系数
    public float tension = 50f;            // 表面张力系数
    public float damping = 5f;              // 阻尼
    public float maxVelocity = 10f;

    [Header("冲击参数")]
    public float impactStrength = 5f;       // 冲击速度
    public float impactSpread = 0.2f;        // 冲击区域宽度

    [Header("边界反弹")]
    public float topBounce = 0.5f;           // 碰顶反弹系数
    public float bottomBounce = 0.5f;         // 碰底反弹系数

    private float[] y;        // 相对基准面的偏移
    private float[] vy;       // 速度
    private float[] x;        // 归一化位置 (0~1)
    private Texture2D heightTex;
    private MaterialPropertyBlock propBlock;
    public SpriteRenderer sr;
    public BaseUnit unit;
    void Start()
    {
        if(sr == null)
            sr = GetComponent<SpriteRenderer>();
        propBlock = new MaterialPropertyBlock();
        unit.attributes.OnHealthRateChanged += ChangeHealth;
        // 初始化质点
        y = new float[pointCount];
        vy = new float[pointCount];
        x = new float[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            x[i] = i / (float)(pointCount - 1);
            y[i] = 0f;
            vy[i] = 0f;
        }

        // 创建1D高度纹理
        heightTex = new Texture2D(pointCount, 1, TextureFormat.RFloat, false);
        heightTex.filterMode = FilterMode.Bilinear;
        heightTex.wrapMode = TextureWrapMode.Clamp;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        float[] forces = new float[pointCount];
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeHealth(-0.1f);
        }
        // 计算合力
        for (int i = 0; i < pointCount; i++)
        {
            float restore = -springConstant * y[i];
            float tensionForce = 0f;
            if (i > 0) tensionForce += tension * (y[i - 1] - y[i]);
            if (i < pointCount - 1) tensionForce += tension * (y[i + 1] - y[i]);
            float damp = -damping * vy[i];
            forces[i] = restore + tensionForce + damp;
        }

        // 更新速度和位置
        for (int i = 0; i < pointCount; i++)
        {
            vy[i] += forces[i] * dt;
            vy[i] = Mathf.Clamp(vy[i], -maxVelocity, maxVelocity);
            y[i] += vy[i] * dt;

            // 碰撞处理：绝对高度不能超出[0,1]
            float absHeight = health + y[i];
            if (absHeight > 1f)
            {
                y[i] = 1f - health;
                vy[i] = -vy[i] * topBounce;
            }
            else if (absHeight < 0f)
            {
                y[i] = -health;
                vy[i] = -vy[i] * bottomBounce;
            }
        }

        // 更新纹理
        for (int i = 0; i < pointCount; i++)
        {
            float h = Mathf.Clamp01(health + y[i]);
            heightTex.SetPixel(i, 0, new Color(h, 0, 0, 1));
        }
        heightTex.Apply();

        // 设置材质属性
        sr.GetPropertyBlock(propBlock);
        propBlock.SetTexture("_HeightTex", heightTex);
        propBlock.SetFloat("_Fill", health);
        sr.SetPropertyBlock(propBlock);
    }

    public void ChangeHealth(float delta)
    {
        health = Mathf.Clamp01(health + delta);
        if (delta < 0)  // 扣血时激发冲击
        {
            // 随机选择从左端（0）或右端（1）开始
            int side = Random.Range(0, 2);
            float center = side == 0 ? 0f : 1f;

            for (int i = 0; i < pointCount; i++)
            {
                float dist = Mathf.Abs(x[i] - center);
                if (dist < impactSpread)
                {
                    float factor = 1f - dist / impactSpread;          // 距离衰减
                    vy[i] += impactStrength * factor * Random.Range(0.9f, 1.1f); // 加一点随机让每次略有不同
                }
            }
        }
    }
}