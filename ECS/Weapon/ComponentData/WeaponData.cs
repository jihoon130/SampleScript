using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct WeaponProjectileEvent : IBufferElementData
{
    public uint ID;
    public float3 SimulationPosition;
    public float3 VisualPosition;
    public float3 SimulationDirection;
}

public struct WeaponShotSimulationOriginOverride : IComponentData
{
    public Entity Entity;
}

public struct WeaponShotIgnoredEntity : IBufferElementData
{
    public Entity Entity;
}

public struct ActiveWeapon : IComponentData
{
    public Entity Entity;
    public Entity PreviousEntity;
}