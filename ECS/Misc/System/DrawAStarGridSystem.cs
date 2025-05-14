using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class AStarGridGizmoDrawer : MonoBehaviour
{
    void OnDrawGizmos()
    {
        if (World.DefaultGameObjectInjectionWorld == null)
            return;

        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<GridBlobHolder>());

        if (query.CalculateEntityCount() == 0)
            return;

        var holders = query.ToComponentDataArray<GridBlobHolder>(Unity.Collections.Allocator.Temp);
        foreach (var holder in holders)
        {
            ref var grid = ref holder.GridBlob.Value;

            for (int x = 0; x < grid.GridSize.x; x++)
            {
                for (int z = 0; z < grid.GridSize.y; z++)
                {
                    var cell = grid.Cells[x + z * grid.GridSize.x];
                    Color color = (cell.State == (byte)0) ? Color.green : Color.red;

                    float3 pos = new float3(
                        grid.Origin.x + x * grid.CellSize + grid.CellSize * 0.5f,
                        grid.Origin.y,
                        grid.Origin.z + z * grid.CellSize + grid.CellSize * 0.5f
                    );

                    float heightOffset = 0.05f; 
                    pos.y += heightOffset;

                    Gizmos.color = color;
                    Gizmos.DrawWireCube((Vector3)pos, new Vector3(1, 0.1f, 1));
                }
            }
        }

        holders.Dispose();
    }
}
