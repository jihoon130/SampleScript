using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct GroundTag : IComponentData { }

public struct GridCell
{
    public byte State;   // 0 = Walkable, 1 = Blocked
}

public struct GridBlob
{
    public int2 GridSize;
    public float CellSize;
    public float3 Origin;
    public BlobArray<GridCell> Cells;
}

public struct PathBufferElement : IBufferElementData
{
    public int2 GridPosition;
}

public struct PathRequest : IComponentData
{
    public bool RequestPath;
}