using UnityEngine;

/// <summary>
/// 动画实现的基类。
/// 动画本身不关心时间怎么走，只关心：
/// 给我一个 progress，我把效果应用上去。
/// </summary>
public abstract class Animation_Self
{
    /// <summary>动画总时长（秒）。<= 0 表示立即完成。</summary>
    public abstract float Duration { get; }

    /// <summary>是否使用不受 timeScale 影响的真实时间。</summary>
    public virtual bool UseUnscaledTime => false;

    /// <summary>初始化。被 Play 时调用一次。</summary>
    public virtual void OnStartInit() { }

    /// <summary>持续化。每帧调用，progress 从 0 到 1。</summary>
    public abstract void UpdateAnimation();

    /// <summary>结束。progress 到达 1 时调用一次。</summary>
    public virtual void OnComplete() { }

    /// <summary>中断。被新动画替换或被 Stop 时调用一次。</summary>
    public virtual void OnInterrupt() { }
}