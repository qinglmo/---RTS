using UnityEngine;

public class HealthFill : MonoBehaviour
{
    [Header("血量（瞬时变化）")]
    [Range(0f, 1f)]
    public float health = 1f;

    [Header("宏观倾斜（非常轻微）")]
    public float tiltSpring = 5f;          // 很弱的回复力
    public float tiltDamping = 2f;
    public float maxTilt = 0.05f;           // 最大倾斜幅度很小
    public float wallBounce = 0.5f;

    [Header("表面风暴参数（夸张波动）")]
    public float[] ampMin = { 0.05f, 0.04f, 0.03f, 0.02f };     // 最小振幅（大一些）
    public float[] ampMax = { 0.25f, 0.2f, 0.15f, 0.1f };       // 最大振幅（狂风暴雨）
    public float[] freqMin = { 10f, 20f, 40f, 80f };
    public float[] freqMax = { 30f, 60f, 120f, 200f };
    public float[] speedMin = { 15f, 25f, 40f, 60f };
    public float[] speedMax = { 40f, 70f, 120f, 200f };

    [Header("表面层控制")]
    public float surfaceLayer = 0.25f;       // 只影响上面25%的水体
    public float depthFalloff = 3f;           // 衰减陡度

    [Header("衰减时间")]
    public float stormDuration = 3f;           // 风暴持续约3秒
    private float stormTimer = 0f;

    // 运行时
    private float tilt;
    private float tiltVelocity;

    private float[] currentAmps = new float[4];
    private float[] targetAmps = new float[4];
    private float[] freqs = new float[4];
    private float[] speeds = new float[4];
    private float[] phases = new float[4];

    private SpriteRenderer sr;
    private MaterialPropertyBlock propBlock;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        propBlock = new MaterialPropertyBlock();

        for (int i = 0; i < 4; i++)
        {
            targetAmps[i] = 0f;
            currentAmps[i] = 0f;
            freqs[i] = Random.Range(freqMin[i], freqMax[i]);
            speeds[i] = Random.Range(speedMin[i], speedMax[i]);
            phases[i] = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeHealth(-0.1f);
        }

        // 宏观倾斜（很轻微）
        float accel = -tiltSpring * tilt - tiltDamping * tiltVelocity;
        tiltVelocity += accel * Time.deltaTime;
        tilt += tiltVelocity * Time.deltaTime;

        // 限制倾斜
        float currentMaxTilt = maxTilt * health;
        if (tilt > currentMaxTilt)
        {
            tilt = currentMaxTilt;
            tiltVelocity = -tiltVelocity * wallBounce;
        }
        else if (tilt < -currentMaxTilt)
        {
            tilt = -currentMaxTilt;
            tiltVelocity = -tiltVelocity * wallBounce;
        }

        // 风暴计时衰减
        if (stormTimer > 0)
        {
            stormTimer -= Time.deltaTime;
            // 目标振幅随时间线性衰减（也可用指数，但线性简单）
            float decayFactor = Mathf.Clamp01(stormTimer / stormDuration);
            for (int i = 0; i < 4; i++)
            {
                // targetAmps 已在扣血时设置，这里根据时间衰减到0
                // 更好的方式：让 targetAmps 自身衰减
            }
        }
        else
        {
            // 风暴结束，所有振幅归零
            for (int i = 0; i < 4; i++)
            {
                targetAmps[i] = 0f;
            }
        }

        // 平滑当前振幅
        for (int i = 0; i < 4; i++)
        {
            currentAmps[i] = Mathf.Lerp(currentAmps[i], targetAmps[i], Time.deltaTime * 10f); // 快速跟随
        }

        // 更新材质属性
        sr.GetPropertyBlock(propBlock);
        propBlock.SetFloat("_Fill", health);
        propBlock.SetFloat("_Tilt", tilt);
        propBlock.SetFloat("_TiltStrength", 0.5f); // 轻微

        propBlock.SetFloat("_Wave1Amp", currentAmps[0]);
        propBlock.SetFloat("_Wave2Amp", currentAmps[1]);
        propBlock.SetFloat("_Wave3Amp", currentAmps[2]);
        propBlock.SetFloat("_Wave4Amp", currentAmps[3]);

        propBlock.SetFloat("_Wave1Freq", freqs[0]);
        propBlock.SetFloat("_Wave2Freq", freqs[1]);
        propBlock.SetFloat("_Wave3Freq", freqs[2]);
        propBlock.SetFloat("_Wave4Freq", freqs[3]);

        propBlock.SetFloat("_Wave1Speed", speeds[0]);
        propBlock.SetFloat("_Wave2Speed", speeds[1]);
        propBlock.SetFloat("_Wave3Speed", speeds[2]);
        propBlock.SetFloat("_Wave4Speed", speeds[3]);

        propBlock.SetFloat("_Wave1Phase", phases[0]);
        propBlock.SetFloat("_Wave2Phase", phases[1]);
        propBlock.SetFloat("_Wave3Phase", phases[2]);
        propBlock.SetFloat("_Wave4Phase", phases[3]);

        propBlock.SetFloat("_SurfaceLayer", surfaceLayer);
        propBlock.SetFloat("_DepthFalloff", depthFalloff);
        sr.SetPropertyBlock(propBlock);
    }

    public void ChangeHealth(float delta)
    {
        health = Mathf.Clamp01(health + delta);
        if (delta < 0)
        {
            // 扣血时激发风暴
            stormTimer = stormDuration;

            // 冲击强度基于扣血量和当前血量
            float impact = Mathf.Abs(delta) * 10f * health;

            // 宏观倾斜：给一个非常小的随机方向冲击（可忽略）
            float direction = Random.value > 0.5f ? 1f : -1f;
            tiltVelocity += direction * impact * 0.1f; // 倾斜影响很小

            // 表面风暴：随机化各层波的振幅，并重新随机化频率、速度、相位
            for (int i = 0; i < 4; i++)
            {
                targetAmps[i] = Random.Range(ampMin[i], ampMax[i]) * impact * 0.5f; // 振幅与冲击强度相关
                // 也随机化频率速度，让每次风暴不同
                freqs[i] = Random.Range(freqMin[i], freqMax[i]);
                speeds[i] = Random.Range(speedMin[i], speedMax[i]);
                phases[i] = Random.Range(0f, Mathf.PI * 2f);
            }
        }
    }
}