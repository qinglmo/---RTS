using UnityEngine;

public interface IHasFaction
{
    Faction Faction { get; }  // 获取该对象所属的阵营
}
public interface IHasPosition
{
    Vector2 Position { get; }
    Vector2Int GridPos { get; }
}
public interface IOccupyEnity:IHasPosition
{

}
public interface IFactionMember : IHasFaction, IHasPosition
{
    
}
