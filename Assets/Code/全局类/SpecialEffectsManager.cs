
using System.Collections;
using UnityEngine;

public class SpecialEffectsManager : MonoBehaviour
{
    public GameObject specialEffectPrefab; // 绿色十字架
    public static SpecialEffectsManager instance;

    private void Awake()
    {
        instance = this;
    }

    // 根据治疗量创建治疗特效
    public void CreateHealEffect(BaseUnit unit, float healAmount)
    {
        if (unit == null || specialEffectPrefab == null || healAmount <= 0)
            return;

        // 根据治疗量确定特效数量
        int effectCount;

        if (healAmount >= 10)
            effectCount = 5;
        else if (healAmount >= 5)
            effectCount = 3;
        else
            effectCount = 1;

        // 单位脚底作为基点
        Vector3 basePosition = unit.transform.position + Vector3.down * 0.5f;

        for (int i = 0; i < effectCount; i++)
        {
            // 在基点左右各 0.5 单位的水平范围内随机生成
            float randomX = Random.Range(-0.5f, 0.5f);
            Vector3 spawnPosition = basePosition + Vector3.right * randomX;

            // 创建特效
            GameObject healEffect = Instantiate(
                specialEffectPrefab,
                spawnPosition,
                Quaternion.identity
            );

            // 设置特效尺寸
            healEffect.transform.localScale = Vector3.one * 0.5f;

            // 随机漂浮速度：每秒 3～5 单位
            float moveSpeed = Random.Range(3f, 5f);

            StartCoroutine(AnimateHealEffect(
                healEffect,
                spawnPosition,
                moveSpeed
            ));
        }
    }

    // 向上漂浮约 1 个单位后销毁
    private IEnumerator AnimateHealEffect(
        GameObject effect,
        Vector3 startPosition,
        float moveSpeed)
    {
        float duration = 1f / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (effect == null)
                yield break;

            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            effect.transform.position = startPosition
                + Vector3.up * progress;

            yield return null;
        }

        if (effect != null)
            Destroy(effect);
    }
}

//设计原稿:
//第一个档次,一个绿色十字特效,尺寸为0.2倍缩放;1滴血
//第二个档次,三个绿色十字特效,尺寸为0.2倍缩放;5滴血
//第三个档次,五绿色十字特效,尺寸为0.2倍缩放;10滴血

//基于单位位置正下0.5位置作为基点,左右各自衍生0.5单位作为水平线,在水平线上任意位置生成十字特效,然后向上漂浮,往上移动1格左右即可消失.移动速度在每s3格到每s5格之间.