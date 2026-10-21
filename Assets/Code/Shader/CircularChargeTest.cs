using UnityEngine;

public class CircularChargeTest : MonoBehaviour
{
    public float duration = 2f;

    private SpriteRenderer sr;
    private Material mat;
    private float timer;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        mat = sr.material;
    }

    void Update()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / duration);
        mat.SetFloat("_Progress", progress);
    }
}