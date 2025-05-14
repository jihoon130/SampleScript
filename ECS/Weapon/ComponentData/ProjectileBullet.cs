using Unity.Entities;
using Unity.Mathematics;

public struct ProjectileBullet : IComponentData, IEnableableComponent
{
    public int Damage;

    //public float HitSparksSize;
    //public float HitSparksLifetime;
    //public float HitSparksSpeed;
    //public float3 HitSparksColor;
}