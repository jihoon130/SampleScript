using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct NavAgentComponent : IComponentData
{
    public Entity TargetEntity;
    public float3 LastPosition;
    public int StuckCounter;
}