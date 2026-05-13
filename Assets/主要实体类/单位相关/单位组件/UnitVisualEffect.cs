using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class UnitVisualEffect : MonoBehaviour
{

    public Transform unitImage_sprite;
    // 受击动画协程（图片闪烁红色并轻微震动）
    private Coroutine hitCoroutine;
    Vector3 basePos = Vector3.zero; // 直接使用零位作为基准

    private UnitAttributes attributes;  // 引用属性组件

    private void OnEnable()
    {
        // 获取同物体或父物体上的 UnitAttributes 组件
        attributes = GetComponentInParent<BaseUnit>().attributes;
        if (attributes != null)
        {
            attributes.OnTakeDamageChanged += PlayHitEffect;
        }
    }

    private void OnDisable()
    {
        if (attributes != null)
        {
            attributes.OnTakeDamageChanged -= PlayHitEffect;
        }
    }
    public void PlayHitEffect()
    {
        if (hitCoroutine != null) StopCoroutine(hitCoroutine);
            hitCoroutine = StartCoroutine(HitEffectCoroutine());
    }
    private IEnumerator HitEffectCoroutine()
    {
        if (unitImage_sprite == null) yield break;

        SpriteRenderer spriteRenderer = unitImage_sprite.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        Vector3 originalPos = unitImage_sprite.localPosition;

        // 闪烁红色并轻微偏移
        float duration = 0.2f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 颜色在红色和原色之间交替
            float t = elapsed / duration;
            spriteRenderer.color = Color.Lerp(Color.red, originalColor, t * 2f); // 快速变红再恢复
                                                                                 // 轻微震动：随机偏移
            unitImage_sprite.localPosition = originalPos + (Vector3)UnityEngine.Random.insideUnitCircle * 0.1f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 恢复原状
        spriteRenderer.color = originalColor;
        unitImage_sprite.localPosition = basePos;
        hitCoroutine = null;
    }
    public void AttackWindup(Vector2 direction, float duration)
    {
        StartCoroutine(SmoothMoveImage((Vector2)unitImage_sprite.localPosition + direction*0.5f, duration));
    }
    public void AttackRecovery(float duration)
    {
        StartCoroutine(SmoothMoveImage(Vector2.zero, duration));
    }
    public IEnumerator SmoothMoveImage(Vector3 targetLocalPos, float duration)
    {
        
        float elapsed = 0f;
        Vector3 start = unitImage_sprite.localPosition;
        while (elapsed < duration)
        {
            unitImage_sprite.localPosition = Vector3.Lerp(start, targetLocalPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        unitImage_sprite.localPosition = targetLocalPos;
    }
}
