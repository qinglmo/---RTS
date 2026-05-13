using UnityEngine;

public class CameraController2D : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 10f;          // 移动速度

    [Header("缩放设置")]
    public float zoomSpeed = 2f;           // 缩放灵敏度
    public float minZoom = 3f;             // 最小正交尺寸（最近）
    public float maxZoom = 10f;            // 最大正交尺寸（最远）

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
            cam.orthographic = true;
    }

    private void LateUpdate()
    {
        MoveCamera_Fixed();
        ZoomCamera();
    }

    // 不受 timeScale 影响的相机移动
    private void MoveCamera_Fixed()
    {
        float h = 0;
        float v = 0;

        // 直接读按键，完全跳过 Input.GetAxis 的平滑系统
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h = 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  h = -1;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    v = 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  v = -1;

        Vector3 move = new Vector3(h, v, 0) * moveSpeed * Time.unscaledDeltaTime;
        transform.Translate(move, Space.World);
    }

    // 滚轮缩放本来就不受 timeScale 影响
    private void ZoomCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newSize = cam.orthographicSize - scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
}