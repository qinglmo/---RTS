using UnityEngine;

public static class GamePosGetter
{
    public static Vector2Int GetMouseGridPos()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return GridManager.Instance.WorldToCell(worldPos);
    }
}