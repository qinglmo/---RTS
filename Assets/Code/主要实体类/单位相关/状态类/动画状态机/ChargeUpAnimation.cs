
using UnityEngine;

/// <summary>
/// 蓄力动画：让圆形贴图逐渐沿 X 轴收缩。
/// </summary>
public class ChargeUpAnimation : Animation_Self
{
    private readonly Transform target;
    private SpriteRenderer sr;
    private Material mat;
    private readonly float duration;
    
    public override float Duration => duration;
    private float elapsedTime;

    public ChargeUpAnimation(Transform target,SpriteRenderer sr, float duration)
    {
        this.target = target;
        this.sr = sr;
        this.duration = Mathf.Max(0f, duration);
    }

    public override void OnStartInit()
    {
        mat = sr.material;
        elapsedTime = 0f;
        mat.SetFloat("_Progress", 0f);
    }

    public override void UpdateAnimation()
    {
        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / duration);
        mat.SetFloat("_Progress", progress);
    }

    public override void OnComplete()
    {
        mat.SetFloat("_Progress", 1f);
    }

    public override void OnInterrupt()
    {
        mat.SetFloat("_Progress", 1f);//打断即消失
    }
}