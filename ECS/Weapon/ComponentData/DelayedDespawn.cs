using Unity.Entities;
using UnityEngine;

public struct DelayedDespawn : IComponentData, IEnableableComponent
{
    public uint Ticks;
    public byte HasHandledPreDespawn;
}
