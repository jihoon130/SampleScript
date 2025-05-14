using Codice.Client.Common;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using System.Linq;
using Unity.Transforms;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct AStarSolverSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<GridBlobHolder>(out var gridHolder))
            return;

        ref var grid = ref gridHolder.GridBlob.Value;
        Entity playerEntity = Entity.Null;
        float3 playerPos = float3.zero;

        foreach (var (player, entity) in SystemAPI.Query<RefRO<Player>>().WithEntityAccess())
        {
            playerEntity = player.ValueRO.ControlledCharacter;
            playerPos = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;
            break;
        }

        if (playerEntity == Entity.Null)
            return;

        foreach (var (enemy, pathBuffer, request, entity) in SystemAPI.Query<RefRW<Enemy>, DynamicBuffer<PathBufferElement>, RefRW<PathRequest>>().WithEntityAccess())
        {
            if (!request.ValueRW.RequestPath)
                continue;

            float3 enemyPos = SystemAPI.GetComponent<LocalTransform>(entity).Position;
            int2 start = WorldToGrid(enemyPos, grid.Origin, grid.CellSize);
            int2 end = WorldToGrid(playerPos, grid.Origin, grid.CellSize);

            if (!IsWalkable(start, ref grid) || !IsWalkable(end, ref grid))
            {
                pathBuffer.Clear();
                request.ValueRW.RequestPath = false;
                continue;
            }

            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            AStarPathfinding(ref grid, start, end, ref path);

            pathBuffer.Clear();
            for (int i = 0; i < path.Length; i++)
                pathBuffer.Add(new PathBufferElement { GridPosition = path[i] });

            path.Dispose();
            request.ValueRW.RequestPath = false;
        }
    }

    private static int2 WorldToGrid(float3 position, float3 origin, float cellSize)
    {
        return new int2(
         (int)math.floor((position.x - origin.x) / cellSize),
         (int)math.floor((position.z - origin.z) / cellSize)
        );
    }

    private static void AStarPathfinding(ref GridBlob grid, int2 start, int2 end, ref NativeList<int2> path)
    {
         var closedSet = new NativeHashSet<int2>(128, Allocator.Temp);
         var openList = new NativeList<Node>(Allocator.Temp);
         var cameFrom = new NativeParallelHashMap<int2, int2>(128, Allocator.Temp);
         var gScore = new NativeParallelHashMap<int2, float>(128, Allocator.Temp);
         var fScore = new NativeParallelHashMap<int2, float>(128, Allocator.Temp);

        openList.Add(new Node { Position = start, FScore = Heuristic(start, end) });
        gScore[start] = 0;
        fScore[start] = Heuristic(start, end);

        while (openList.Length > 0)
        {
            Node current = openList[0];
            openList.RemoveAtSwapBack(0);

            if (current.Position.Equals(end))
            {
                ReconstructPath(cameFrom, current.Position, ref path);
                return;
            }

            closedSet.Add(current.Position);

            foreach (var neighbor in GetNeighbors(current.Position, ref grid))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                if (!IsWalkable(neighbor, ref grid))
                    continue;

                float tentativeG = gScore[current.Position] + Distance(current.Position, neighbor);

                if (!gScore.TryGetValue(neighbor, out float gOld) || tentativeG < gOld)
                {
                    cameFrom[neighbor] = current.Position;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, end);

                    bool inOpenList = false;
                    for (int i = 0; i < openList.Length; i++)
                    {
                        if (openList[i].Position.Equals(neighbor))
                        {
                            openList[i] = new Node { Position = neighbor, FScore = fScore[neighbor] };
                            inOpenList = true;
                            break;
                        }
                    }
                    if (!inOpenList)
                    {
                        openList.Add(new Node { Position = neighbor, FScore = fScore[neighbor] });
                    }

                    openList.Sort(new NodeComparer());
                }
            }
        }

        path.Clear();
    }

    private struct NodeComparer : IComparer<Node>
    {
        public int Compare(Node a, Node b)
        {
            return a.FScore.CompareTo(b.FScore);
        }
    }

    private struct Node
    {
        public int2 Position;
        public float FScore;
    }

    private static float Heuristic(int2 a, int2 b)
    {
        return math.abs(a.x - b.x) + math.abs(a.y - b.y); 
    }

    private static float Distance(int2 a, int2 b)
    {
        return math.distance(new float2(a.x, a.y), new float2(b.x, b.y)); 
    }

    private static bool IsWalkable(int2 pos, ref GridBlob grid)
    {
        int2 size = grid.GridSize;
        if (pos.x < 0 || pos.x >= size.x || pos.y < 0 || pos.y >= size.y)
            return false;

        int index = pos.x + pos.y * size.x;
        return grid.Cells[index].State == 0;
    }

    private static NativeList<int2> GetNeighbors(int2 cell, ref GridBlob grid)
    {
        NativeList<int2> neighbors = new NativeList<int2>(8, Allocator.Temp);
        int2[] directions = new int2[]
        {
    new int2(-1, 0), new int2(1, 0), new int2(0, -1), new int2(0, 1),
    new int2(-1, -1), new int2(-1, 1), new int2(1, -1), new int2(1, 1)
        };

        foreach (var dir in directions)
        {
            int2 neighbor = cell + dir;
            if (IsWalkable(neighbor, ref grid))
                neighbors.Add(neighbor);
        }
        return neighbors;
    }

    private static void ReconstructPath(NativeParallelHashMap<int2, int2> cameFrom, int2 current, ref NativeList<int2> path)
    {
        using var totalPath = new NativeList<int2>(Allocator.Temp);
        totalPath.Add(current);

        while (cameFrom.TryGetValue(current, out int2 prev))
        {
            current = prev;
            totalPath.Add(current);
        }

        for (int i = totalPath.Length - 1; i >= 0; i--)
            path.Add(totalPath[i]);
    }
}