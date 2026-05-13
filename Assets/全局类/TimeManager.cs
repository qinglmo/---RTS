using UnityEngine;
using UnityEngine.UI;
using System;

public class TimeManager : MonoBehaviour
{
    [Header("时间显示文本")]
    public Text dayText;       // 显示天数
    public Text hourMinText;   // 显示小时:分钟

    public Text PlayButtonText;  // 运行暂停按钮文本，点击后切换状态

    // 单例实例
    public static TimeManager Instance { get; private set; }

    // 时间比例配置
    private const float SecondsPerGameHour = 10f;   // 10秒现实时间 = 1小时游戏时间
    private const float MinutesPerGameHour = 60f;    // 60分钟 = 1小时
    private const float HoursPerDay = 24f;            // 24小时 = 1天

    // 游戏开始时的真实时间（秒）
    private float startRealTime;
    // 当前累计的游戏时间（受时间缩放影响）
    private float accumulatedGameTime;

    // 当前时间缩放
    public float CurrentTimeScale { get; private set; } = 1f;
    // 暂停状态
    public bool IsPaused { get; private set; } = false;
    // 暂停前保存的时间缩放
    private float savedTimeScale;

    // 上一次触发脉冲的游戏小时数
    private float lastPulseHour;

    // ====================== 【一小时脉冲事件】 ======================
    // 游戏内每过1小时，自动触发一次，所有脚本可订阅
    public static event Action OnOneHourPulse;
    private int lastPulseDay;
    // ====================== 【天脉冲事件】 ======================
    // 游戏内每过1天，自动触发一次，所有脚本可订阅
    public static event Action OnOneDayPulse;

    // ===============================================================

    // 获取从场景开始到现在的游戏内时间（秒，受时间缩放影响）
    public float GameTime => accumulatedGameTime + (Time.realtimeSinceStartup - startRealTime) * CurrentTimeScale;

    // 游戏内总小时数（含小数）
    public float TotalGameHours => GameTime / SecondsPerGameHour;
    // 游戏内天数（取整）
    public int GameDays => Mathf.FloorToInt(TotalGameHours / HoursPerDay);
    // 游戏内当天的小时数（0-23，取整）
    public int GameHours => Mathf.FloorToInt(TotalGameHours % HoursPerDay);
    // 游戏内当前小时的分钟数（0-59，取整）
    public int GameMinutes => Mathf.FloorToInt((TotalGameHours % 1f) * MinutesPerGameHour);

    private void Awake()
    {
        // 单例初始化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 初始化计时
        startRealTime = Time.realtimeSinceStartup;
        accumulatedGameTime = 0f;
        lastPulseHour = 0f; // 初始化脉冲时间
        lastPulseDay = 0; // 初始化脉冲时间
    }

    private void Start()
    {
        if (PlayButtonText != null)
            PlayButtonText.text = "暂停";
    }

    private void Update()
    {
        UpdateTimeDisplay();
        CheckHourPulse(); // 每帧检查是否到一小时
        CheckDayPulse(); // 每帧检查是否到一天
    }

    /// <summary>
    /// 检查是否到达一小时，触发脉冲事件
    /// </summary>
    private void CheckHourPulse()
    {
        if (IsPaused) return;

        float currentHour = TotalGameHours;
        // 整数小时变化 = 过了一小时
        if (Mathf.FloorToInt(currentHour) > Mathf.FloorToInt(lastPulseHour))
        {
            lastPulseHour = currentHour;
            OnOneHourPulse?.Invoke(); // 触发事件
        }
        
    }
    /// <summary>
    /// 检查是否到达一天，触发脉冲事件
    /// </summary>
    private void CheckDayPulse()
    {
        if (IsPaused) return;

        int currentDay = GameDays;
        // 整数天变化 = 过了一天
        if (currentDay > lastPulseDay)
        {
            lastPulseDay = currentDay;
            OnOneDayPulse?.Invoke(); // 触发事件
        }
    }
    
    /// <summary>
    /// 更新天和小时分钟的文本显示
    /// </summary>
    private void UpdateTimeDisplay()
    {
        if (dayText != null)
            dayText.text = $"{GameDays} 天";
        
        if (hourMinText != null)
            hourMinText.text = $"{GameHours:00}:{GameMinutes:00}";
    }

    /// <summary>
    /// 设置时间缩放（倍速）
    /// </summary>
    public void SetTimeScale(float scale)
    {
        // 暂停时按倍速 → 自动恢复播放
        if (IsPaused && scale > 0)
        {
            ResumeGame();
        }

        if (!IsPaused && scale <= 0)
        {
            Debug.LogWarning("时间缩放不能为0或负数，已设置为0.01");
            scale = 0.01f;
        }

        accumulatedGameTime = GameTime;
        startRealTime = Time.realtimeSinceStartup;
        CurrentTimeScale = scale;
        Time.timeScale = scale;
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        if (IsPaused) return;

        if (PlayButtonText != null)
            PlayButtonText.text = "继续";
        IsPaused = true;
        savedTimeScale = CurrentTimeScale;
        SetTimeScale(0f);
    }

    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        if (!IsPaused) return;

        if (PlayButtonText != null)
            PlayButtonText.text = "暂停";
        IsPaused = false;
        SetTimeScale(savedTimeScale);
    }

    /// <summary>
    /// 切换暂停/恢复状态
    /// </summary>
    public void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    // 便捷方法：正常速度
    public void SetNormalSpeed() => SetTimeScale(1f);
    // 2倍速
    public void SetDoubleSpeed() => SetTimeScale(2f);
    // 3倍速
    public void SetTripleSpeed() => SetTimeScale(3f);
}