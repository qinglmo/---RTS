using UnityEngine;

public class PhysicalWaterSurface : MonoBehaviour
{
    [Header("Ѫ��")]
    [Range(0f, 1f)]
    public float health = 1f;

    [Header("��������")]
    public int pointCount = 32;
    public float springConstant = 100f;   // �ָ���ϵ��
    public float tension = 50f;            // ��������ϵ��
    public float damping = 5f;              // ����
    public float maxVelocity = 10f;

    [Header("�������")]
    public float impactStrength = 5f;       // ����ٶ�
    public float impactSpread = 0.2f;        // ����������

    [Header("�߽練��")]
    public float topBounce = 0.5f;           // ��������ϵ��
    public float bottomBounce = 0.5f;         // ���׷���ϵ��

    private float[] y;        // ��Ի�׼���ƫ��
    private float[] vy;       // �ٶ�
    private float[] x;        // ��һ��λ�� (0~1)
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
        // ��ʼ���ʵ�
        y = new float[pointCount];
        vy = new float[pointCount];
        x = new float[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            x[i] = i / (float)(pointCount - 1);
            y[i] = 0f;
            vy[i] = 0f;
        }

        // ����1D�߶�����
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
        // �������
        for (int i = 0; i < pointCount; i++)
        {
            float restore = -springConstant * y[i];
            float tensionForce = 0f;
            if (i > 0) tensionForce += tension * (y[i - 1] - y[i]);
            if (i < pointCount - 1) tensionForce += tension * (y[i + 1] - y[i]);
            float damp = -damping * vy[i];
            forces[i] = restore + tensionForce + damp;
        }

        // �����ٶȺ�λ��
        for (int i = 0; i < pointCount; i++)
        {
            vy[i] += forces[i] * dt;
            vy[i] = Mathf.Clamp(vy[i], -maxVelocity, maxVelocity);
            y[i] += vy[i] * dt;

            // ��ײ���������Ը߶Ȳ��ܳ���[0,1]
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

        // ��������
        for (int i = 0; i < pointCount; i++)
        {
            float h = Mathf.Clamp01(health + y[i]);
            heightTex.SetPixel(i, 0, new Color(h, 0, 0, 1));
        }
        heightTex.Apply();

        // ���ò�������
        sr.GetPropertyBlock(propBlock);
        propBlock.SetTexture("_HeightTex", heightTex);
        propBlock.SetFloat("_Fill", health);
        sr.SetPropertyBlock(propBlock);
    }

    public void ChangeHealth(float delta)
    {
        health = Mathf.Clamp01(health + delta);
        if (delta < 0)  // ��Ѫʱ�������
        {
            // ���ѡ�����ˣ�0�����Ҷˣ�1����ʼ
            int side = Random.Range(0, 2);
            float center = side == 0 ? 0f : 1f;

            for (int i = 0; i < pointCount; i++)
            {
                float dist = Mathf.Abs(x[i] - center);
                if (dist < impactSpread)
                {
                    float factor = 1f - dist / impactSpread;          // ����˥��
                    vy[i] += impactStrength * factor * Random.Range(0.9f, 1.1f); // ��һ�������ÿ�����в�ͬ
                }
            }
        }
    }
}