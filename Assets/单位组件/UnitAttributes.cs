using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitAttributes
{
    [Header("基础属性")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int baseAttack = 1;
    [SerializeField] private int baseDefense = 1;

    [SerializeField] private float toughness = 0f;    // 耐受力（减免冲击力）
    public float impactQuantity = 0f;    // 受冲击量，累积受冲击量，超过一定值触发眩晕
    [SerializeField] private float impactQuantityMax = 5f;
    [SerializeField] private float stunHeal = 1f;//眩晕恢复时间
    [SerializeField] public float attackInterval = 1f;// 攻击间隔时间

    // 事件定义：当对应属性改变时触发，参数为改变后的新值
    public event Action<int,int> OnHealthChanged;
    public event Action<float> OnHealthRateChanged;//传递血量变化比例
    public event Action OnEscapeEvent;//血量低于一定程度触发逃跑。
    public event Action OnDestroyEvent;
    public event Action OnLifeFullyRestored;

    public event Action<int> OnAttackChanged;
    public event Action<int> OnDefenseChanged;
    public event Action<float> OnToughnessChanged;

    public event Action<float> OnStunned; // 眩晕事件，传递实际眩晕时间
    public event Action<float> OnimpactQuantityChanged;//冲击积累值

    public event Action OnTakeDamageChanged;//受击事件，通知受击动画。

    // 公开属性（通过属性访问保证触发事件）
    public int CurrentHealth
    {
        get => currentHealth;
        private set
        {
            int newValue = Mathf.Clamp(value, 0, MaxHealth);
            if (currentHealth != newValue)
            {
                OnHealthRateChanged?.Invoke((newValue-currentHealth)/(float)maxHealth);
                currentHealth = newValue;
                OnHealthChanged?.Invoke(currentHealth,maxHealth);
                if (currentHealth / (float)maxHealth < 0.5f)
                {
                    OnEscapeEvent?.Invoke();
                }
                if (currentHealth >= MaxHealth)
                {
                    OnLifeFullyRestored?.Invoke();
                }
                if(currentHealth==0)
                    OnDestroyEvent?.Invoke();
            }
        }
    }

    public int MaxHealth
    {
        get => maxHealth;
        set
        {
            int newValue = Mathf.Max(1, value); // 最大生命至少为1
            if (maxHealth != newValue)
            {
                maxHealth = newValue;
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
                // 若当前生命超出新最大值，则自动修正
                if (currentHealth > maxHealth)
                {
                    CurrentHealth = maxHealth; // 会触发 HealthChanged 事件
                }
            }
        }
    }

    public int BaseAttack
    {
        get => baseAttack;
        set
        {
            int newValue = Mathf.Max(0, value);
            if (baseAttack != newValue)
            {
                baseAttack = newValue;
                OnAttackChanged?.Invoke(baseAttack);
            }
        }
    }

    public int BaseDefense
    {
        get => baseDefense;
        set
        {
            int newValue = Mathf.Max(0, value);
            if (baseDefense != newValue)
            {
                baseDefense = newValue;
                OnDefenseChanged?.Invoke(baseDefense);
            }
        }
    }
    public float Toughness
    {
        get => toughness;
        private set
        {
            float newValue = Mathf.Max(0f, value);
            if (Math.Abs(toughness - newValue) > 0.001f)
            {
                toughness = newValue;
                OnToughnessChanged?.Invoke(toughness);
            }
        }
    }
    /// <summary>
    /// 初始化所有属性（通常用于外部设置初始值，会触发相应事件）
    /// </summary>
    public void Initialize(int health, int maxHealth, int attack, int defense)
    {
        // 先设置最大生命（依赖 MaxHealth 属性确保合法性）
        this.maxHealth = maxHealth;
        // 再设置当前生命（自动钳制）
        currentHealth = health;
        // 设置攻击防御
        baseAttack = attack;
        baseDefense = defense;
    }

    /// <summary>
    /// 受到伤害（自动扣除生命，不低于0）
    /// </summary>
    public void TakeDamage(int damage, float impactAmount)
    {
        if (damage > 0)
            CurrentHealth -= damage;
        // 计算实际冲击量
        float actualImpactQuantity = Mathf.Max(0f, impactAmount - Toughness);
        impactQuantity += actualImpactQuantity;
        if (impactQuantity >= impactQuantityMax)
        {
            impactQuantity = 0;
            OnStunned?.Invoke(stunHeal);
        }
        OnTakeDamageChanged?.Invoke();
    }

    /// <summary>
    /// 治疗（增加生命，不超过最大生命）
    /// </summary>
    public void Heal(int amount)
    {
        if (amount > 0)
            CurrentHealth += amount;
    }

    /// <summary>
    /// 直接设置当前生命值（用于特殊逻辑）
    /// </summary>
    public void SetHealth(int newHealth)
    {
        CurrentHealth = newHealth;
    }

}
