// 泛型自动过期引用
using System.Collections;
using UnityEngine;

public class AutoExpireRef<T> where T : class
{
    private T _target;               // 持有的对象引用
    private float _expireTime;        // 过期时间（Time.time + duration）
    private MonoBehaviour _coroutineRunner; // 用于启动协程的 MonoBehaviour
    private Coroutine _coroutine;     // 用于停止协程

    // 构造函数：传入目标对象、存活时长、协程运行器
    public AutoExpireRef(T target, float lifetime, MonoBehaviour runner)
    {
        _target = target;
        _expireTime = Time.time + lifetime;
        _coroutineRunner = runner;

        // 启动协程，等待到期后清理
        _coroutine = runner.StartCoroutine(WaitAndExpire());
    }

    // 访问当前持有的对象（如果未过期则返回对象，否则返回 null）
    public T Target
    {
        get
        {
            // 如果已过期，直接返回 null（内部引用可能已被协程清空）
            if (_target == null || Time.time >= _expireTime)
                return null;
            return _target;
        }
    }

    // 手动重置过期时间（延长生命周期）
    public void ResetLifetime(float additionalLifetime)
    {
        if (_target == null) return;
        _expireTime = Time.time + additionalLifetime;
        // 注意：如果之前协程已结束，可能需要重新启动。这里简化处理，不重新启动，依靠 Target 访问时检查
    }

    // 强制立即过期
    public void ExpireNow()
    {
        _target = null;
        if (_coroutine != null)
        {
            _coroutineRunner.StopCoroutine(_coroutine);
            _coroutine = null;
        }
    }

    // 协程：等待到期后将 _target 置 null
    private IEnumerator WaitAndExpire()
    {
        float waitTime = _expireTime - Time.time;
        if (waitTime > 0)
            yield return new WaitForSeconds(waitTime);

        _target = null;
        _coroutine = null;
    }
}
/// <summary>
/// 自动过期的可空值类型包装器（基于时间）
/// </summary>
public class AutoExpireNullable<T> where T : struct
{
    private T? _value;           // 实际存储的值（可为 null）
    private float _expireTime;    // 过期时间戳（Time.time + lifetime）

    /// <param name="initialValue">初始值</param>
    /// <param name="lifetime">存活时长（秒）</param>
    public AutoExpireNullable(T initialValue, float lifetime)
    {
        _value = initialValue;
        _expireTime = Time.time + lifetime;
    }

    /// <summary>
    /// 获取当前值，如果已过期则返回 null
    /// </summary>
    public T? Value
    {
        get
        {
            // 如果已经手动置 null 或者当前时间超过过期时间，则返回 null
            if (!_value.HasValue || Time.time >= _expireTime)
                return null;
            return _value;
        }
        set
        {
            _value = value;
            // 当设置新值时，可以选择重置过期时间（根据需求）
            // 如果想在设置新值时延长生命周期，可以在这里调用 ResetLifetime
        }
    }

    /// <summary>
    /// 重置生命周期（从当前时间开始重新计时）
    /// </summary>
    public void ResetLifetime(float newLifetime)
    {
        if (_value.HasValue)
        {
            _expireTime = Time.time + newLifetime;
        }
    }

    /// <summary>
    /// 强制立即过期（内部值置 null）
    /// </summary>
    public void ExpireNow()
    {
        _value = null;
    }

    /// <summary>
    /// 是否已过期（无论值是否为 null，只要时间超过即视为过期）
    /// </summary>
    public bool IsExpired => !_value.HasValue || Time.time >= _expireTime;
}