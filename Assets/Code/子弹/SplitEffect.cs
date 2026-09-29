using UnityEngine;

public interface IBulletEffect
{
    void OnHit(BulletBase bullet, Collider2D hitTarget);
}
public class SplitEffect : MonoBehaviour, IBulletEffect
{
    [Header("分裂参数")]
    public int fragmentCount = 8;
    public GameObject fragmentPrefab;

    [Header("爆炸效果")]
    public GameObject explodeVFX;

    private bool hasExploded = false;

    public void OnHit(BulletBase bullet, Collider2D hitTarget)
    {
        if (hasExploded) return;
        hasExploded = true;

        if (explodeVFX != null)
            Instantiate(explodeVFX, bullet.transform.position, Quaternion.identity);

        SpawnFragments(bullet);
    }

    private void SpawnFragments(BulletBase bullet)
    {
        if (fragmentPrefab == null || fragmentCount <= 0) return;

        float angleStep = 360f / fragmentCount;
        Vector2 baseDir = Vector2.right;

        for (int i = 0; i < fragmentCount; i++)
        {
            float angle = i * angleStep;
            Vector2 fragmentDir = Quaternion.Euler(0, 0, angle) * baseDir;

            GameObject fragObj = Instantiate(fragmentPrefab, bullet.transform.position, Quaternion.identity);
            DirectionalBullet frag = fragObj.GetComponent<DirectionalBullet>();
            if (frag != null)
            {
                frag.Initialize(fragmentDir, bullet.Faction);
            }
            else
            {
                Debug.LogError("Fragment prefab missing DirectionalBullet component!");
                Destroy(fragObj);
            }
        }
    }
}