using UnityEngine;
using UnityEngine.UI;
public class SystemInfoPanel : MonoBehaviour
{
    //主要用来测试的，显示部分核心信息，实际游戏中不需要显示
    [SerializeField] private Text PlayerUnitCountText;
    [SerializeField] private Text PlayerOccupiedUnitCountText;
    public Faction PlayerFaction;
    public Faction EnemyFaction;

    [SerializeField] private Text EnemyUnitCountText;
    [SerializeField] private Text EnemyOccupiedUnitCountText;

    public void UpdateInfo()
    {
        int PlayerUnitCount=0;
        int PlayerOccupiedUnitCount=0;
        int EnemyUnitCount=0;
        int EnemyOccupiedUnitCount=0;
        foreach(var unit in FactionManager.Instance.FactionUnits[PlayerFaction])
        {
            PlayerUnitCount++;
            if (unit.movement.stepMover.IsOccupy == true)
            {
                PlayerOccupiedUnitCount++;
            }
        }
        foreach(var unit in FactionManager.Instance.FactionUnits[EnemyFaction])
        {
            EnemyUnitCount++;
            if (unit.movement.stepMover.IsOccupy == true)
            {
                EnemyOccupiedUnitCount++;
            }
        }
        PlayerUnitCountText.text=PlayerUnitCount.ToString();
        PlayerOccupiedUnitCountText.text=PlayerOccupiedUnitCount.ToString();
        EnemyUnitCountText.text=EnemyUnitCount.ToString();
        EnemyOccupiedUnitCountText.text=EnemyOccupiedUnitCount.ToString();
    }
}