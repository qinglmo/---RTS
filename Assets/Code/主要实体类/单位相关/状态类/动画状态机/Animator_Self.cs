using UnityEngine;

/// <summary>
/// 同一时刻只持有一个动画的播放器。
/// 负责时间推进与生命周期调度，不关心动画具体做了什么。
/// </summary>
public class Animator_Self : MonoBehaviour
{
    public Animation_Self Current { get; private set; }
    public bool IsPlaying => Current != null;
    public float Progress { get; private set; }

    float startTime;
    bool useUnscaledTime;

    float Now => useUnscaledTime ? Time.unscaledTime : Time.time;

    /// <summary>播放一个动画。若当前有动画，先中断它。</summary>
    public void Play(Animation_Self anim)
    {
        if (anim == null) return;

        // 打断当前
        if (Current != null)
        {
            Current.OnInterrupt();
            Current = null;
        }

        Current = anim;
        useUnscaledTime = anim.UseUnscaledTime;
        startTime = Now;
        Progress = 0f;

        anim.OnStartInit();

        // 时长为 0，立即完成
        if (anim.Duration <= 0f)
        {
            anim.UpdateAnimation();
            Progress = 1f;
            Finish();
            return;
        }

        anim.UpdateAnimation();
    }

    /// <summary>主动停止当前动画，触发中断。</summary>
    public void Stop()
    {
        if (Current == null) return;

        Current.OnInterrupt();
        Current = null;
        Progress = 0f;
    }

    void Update()
    {
        if (Current == null) return;

        float t = (Now - startTime) / Current.Duration;

        if (t >= 1f)
        {
            Progress = 1f;
            Current.UpdateAnimation();
            Finish();
            return;
        }

        Progress = t;
        Current.UpdateAnimation();
    }

    void Finish()
    {
        var anim = Current;
        Current = null;
        anim.OnComplete();
    }
}