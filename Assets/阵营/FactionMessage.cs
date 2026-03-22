using System.Collections.Generic;
using UnityEngine;

public struct EnemySpottedMessage
{
    public IFactionMember spotter;
    public IFactionMember enemy;
    public float timestamp;
}
[CreateAssetMenu(menuName = "Events/EnemySpottedChannel")]
public class EnemySpottedChannel : ScriptableObject
{
    public System.Action<EnemySpottedMessage> OnEventRaised;

    public void RaiseEvent(EnemySpottedMessage msg)
    {
        OnEventRaised?.Invoke(msg);
    }
}
public class FactionMessage : MonoBehaviour
{
    public static FactionMessage Instance;
    private Dictionary<Faction, EnemySpottedChannel> teamChannels = new Dictionary<Faction, EnemySpottedChannel>();

    void Awake() => Instance = this;

    public EnemySpottedChannel GetChannel(Faction fation)
    {
        if (!teamChannels.ContainsKey(fation))
        {
            // 可以动态创建，或从Resources加载预定义的通道
            var channel = ScriptableObject.CreateInstance<EnemySpottedChannel>();
            teamChannels[fation] = channel;
        }
        return teamChannels[fation];
    }
}