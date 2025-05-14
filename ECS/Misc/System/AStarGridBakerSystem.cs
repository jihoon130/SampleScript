using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct AStarGridBakerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GroundTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        EntityQuery groundQuery = state.GetEntityQuery(ComponentType.ReadOnly<GroundTag>(), ComponentType.ReadOnly<PhysicsCollider>());
        if (groundQuery.CalculateEntityCount() == 0)
            return;

        Entity groundEntity = groundQuery.GetSingletonEntity();
        var collider = state.EntityManager.GetComponentData<PhysicsCollider>(groundEntity);

        Aabb aabb = collider.Value.Value.CalculateAabb();
        float3 min = aabb.Min;
        float3 max = aabb.Max;
        float3 size = max - min;

        float cellSize = 1.0f;
        float minSizeX = math.max(size.x, 0.001f);
        float minSizeZ = math.max(size.z, 0.001f);

        int2 gridSize = new int2(
         math.max(1, (int)math.ceil(minSizeX / cellSize)),
         math.max(1, (int)math.ceil(minSizeZ / cellSize))
        );

        float3 origin = new float3(min.x, 17f, min.z);
        NativeArray<GridCell> cells = new NativeArray<GridCell>(gridSize.x * gridSize.y, Allocator.Temp);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int z = 0; z < gridSize.y; z++)
            {
                float3 cellCenter = new float3(
                 origin.x + x * cellSize + cellSize * 0.5f,
                 origin.y,
                 origin.z + z * cellSize + cellSize * 0.5f
                );

                var aabbCell = new Aabb
                {
                    Min = cellCenter - new float3(cellSize * 0.5f, 0.1f, cellSize * 0.5f),
                    Max = cellCenter + new float3(cellSize * 0.5f, 0.1f, cellSize * 0.5f)
                };

                OverlapAabbInput input = new OverlapAabbInput
                {
                    Aabb = aabbCell,
                    Filter = CollisionFilter.Default
                };

                NativeList<int> hits = new NativeList<int>(Allocator.Temp);
                bool blocked = physicsWorld.CollisionWorld.OverlapAabb(input, ref hits);
                hits.Dispose();

                cells[x + z * gridSize.x] = new GridCell { State = blocked ? (byte)1 : (byte)0 };
            }
        }

        var builder = new BlobBuilder(Allocator.Temp);
        ref var root = ref builder.ConstructRoot<GridBlob>();
        root.GridSize = gridSize;
        root.CellSize = cellSize;
        root.Origin = origin;

        var array = builder.Allocate(ref root.Cells, cells.Length);
        for (int i = 0; i < cells.Length; i++)
        {
            array[i] = cells[i];
        }

        BlobAssetReference<GridBlob> blob = builder.CreateBlobAssetReference<GridBlob>(Allocator.Persistent);
        builder.Dispose();
        cells.Dispose();

        Entity gridEntity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentData(gridEntity, new GridBlobHolder { GridBlob = blob });

        state.Enabled = false;
    }
}

public struct GridBlobHolder : IComponentData
{
    public BlobAssetReference<GridBlob> GridBlob;
}