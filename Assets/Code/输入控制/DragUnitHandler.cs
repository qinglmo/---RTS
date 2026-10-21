using System;
using UnityEngine;

/// <summary>
/// 纯鼠标拖拽检测：长按触发吸附，松开触发释放。
/// 
/// </summary>
public class DragUnitHandler: SingletonMono<DragUnitHandler>
{

    private bool EnableDrag=true;
    private bool isDragging;

    private float pressTime;
    private BaseUnit currentUnit;
    private Transform currentSprite;
    private Vector2Int startPos;

    private void Update()
    {
        // 忽略UI
        if (UnityEngine.EventSystems.EventSystem.current != null &&
            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            return;
        if(!EnableDrag)
            return;
        if (!isDragging)
        {
            if (Input.GetMouseButtonDown(0))
            {
                startPos = GamePosGetter.GetMouseGridPos();
                if (PickUpUnit())
                {
                    isDragging = true;
                }
            }
        }
        else
        {
            
            if (Input.GetMouseButtonUp(0))
            {
                ReleaseUnit();
                isDragging = false;
            }

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelDrag();
                isDragging = false;
            }
        }

        if(currentUnit != null&&currentSprite != null)
        {
            Vector2Int gridPos = GamePosGetter.GetMouseGridPos();
            currentSprite.position = GridManager.Instance.CellToWorld(gridPos);
        }
    }
    private bool PickUpUnit(){
        BaseUnit unit = GridEntityProvider.Instance.GetUnit(startPos);
        if (unit == null)
            return false; // 没有单位不开始拖拽

        currentUnit = unit;
        currentSprite = unit.movement.stepMover.playerSprite.transform;//获取单位的视觉对象
        
        return true;
    }
    private void ReleaseUnit(){
        Vector2Int gridPos = GamePosGetter.GetMouseGridPos();

        if (!GridManager.Instance.BoundsChecking(gridPos) || GridManager.Instance.IsOccupied(gridPos))
        {
            CancelDrag();
        }
        else
        {
            // 合法空格子 → 放下
            GridManager.Instance.Release(startPos, currentUnit);
            if(GridManager.Instance.TryOccupy(gridPos, currentUnit))
            {
                currentUnit.movement.stepMover.ChangeCurrentPos(gridPos);
                currentUnit.transform.position = GridManager.Instance.CellToWorld(gridPos);
                currentSprite.position = GridManager.Instance.CellToWorld(gridPos);
                currentUnit = null;
                currentSprite = null;
            }
            else
            {
                GridManager.Instance.TryOccupy(startPos, currentUnit) ;
                Debug.LogError("位置不合法，单位无法占用");
                CancelDrag();
            }
        }
    }
    private void CancelDrag(){

        //取消吸附单位，将其返回原位
        currentSprite.position = GridManager.Instance.CellToWorld(startPos);
        currentUnit = null;
        currentSprite = null;
    }

    public void SetEnableDrag(bool enable)
    {
        EnableDrag=enable;
        if(currentUnit!=null)
            CancelDrag();
    }
}