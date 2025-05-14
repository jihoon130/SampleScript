using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct EnemySpanwer : IComponentData
{
    public Entity Enemy;
    public float3 LastSpawnPoint;
}
