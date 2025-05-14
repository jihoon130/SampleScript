using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct Projectile : IComponentData
{
    public float Speed;
    public float Gravity;
    public float MaxLifeTime;
    public float VisualOffsetCorrectionDuration;

    public Entity HitEntity;
    public byte HasHit;
    public float3 Velocity;
    public float LifetimeCounter;
    public float3 VisualOffset;
}
