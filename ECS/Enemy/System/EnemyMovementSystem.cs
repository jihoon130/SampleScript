using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EnemyMovementSystem : ISystem
{
    private float3 previousPlayerPosition;
    private float pathRequestThreshold;

    public void OnCreate(ref SystemState state)
    {
        pathRequestThreshold = 2.0f;
    }

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

        bool playerMovedSignificantly = math.distance(previousPlayerPosition.xz, playerPos.xz) >= pathRequestThreshold;
        previousPlayerPosition = playerPos;

        foreach (var (navAgent, pathBuffer, request, entity) in SystemAPI.Query<RefRW<NavAgentComponent>, DynamicBuffer<PathBufferElement>, RefRW<PathRequest>>().WithEntityAccess())
        {
            float3 currentPos = SystemAPI.GetComponent<LocalTransform>(entity).Position;
            CharacterControl control = SystemAPI.GetComponent<CharacterControl>(entity);

            float3 toPlayer = playerPos - currentPos;
            toPlayer.y = 0;
            float distanceToPlayer = math.length(toPlayer);

            float attackRange = 1.5f;
            if (distanceToPlayer <= attackRange)
            {
                control.AttackHeId = true;
                control.MoveVector = float3.zero;
            }
            else if (pathBuffer.Length > 0)
            {
                int2 nextCell = pathBuffer[0].GridPosition;
                float3 nextWorldPos = new float3(
                 grid.Origin.x + nextCell.x * grid.CellSize + grid.CellSize * 0.5f,
                 grid.Origin.y,
                 grid.Origin.z + nextCell.y * grid.CellSize + grid.CellSize * 0.5f
                );

                float3 toNext = nextWorldPos - currentPos;
                toNext.y = 0;
                float distanceToNext = math.length(toNext);

                if (distanceToNext < 0.5f)
                {
                    pathBuffer.RemoveAt(0);
                    request.ValueRW.RequestPath = true; // 다음 경유지로 이동했으므로 필요하다면 새 경로 요청
                }
                else
                {
                    float3 desiredDir = toNext / (distanceToNext + 1e-5f);

                    // Local avoidance (basic): push away from nearby enemies
                    float3 avoidance = float3.zero;
                    foreach (var (otherNav, otherEntity) in SystemAPI.Query<RefRW<NavAgentComponent>>().WithEntityAccess())
                    {
                        if (otherEntity == entity)
                            continue;

                        float3 otherPos = SystemAPI.GetComponent<LocalTransform>(otherEntity).Position;
                        float3 offset = currentPos - otherPos;
                        offset.y = 0;
                        float dist = math.length(offset);

                        if (dist > 0f && dist < 1.5f)
                            avoidance += offset / (dist * dist);
                    }

                    float3 finalDir = math.normalize(desiredDir + avoidance);
                    float3 smoothedDir = math.lerp(control.MoveVector, finalDir, 0.2f);

                    control.MoveVector = smoothedDir;
                    control.AttackHeId = false;

                    if (distanceToNext < 0.2f)
                        request.ValueRW.RequestPath = true;
                }
            }
            else
            {
                control.MoveVector = float3.zero;
                control.AttackHeId = false;
                if (playerMovedSignificantly)
                    request.ValueRW.RequestPath = true;
            }

            SystemAPI.SetComponent(entity, control);

            if (playerMovedSignificantly)
                request.ValueRW.RequestPath = true;
        }
    }
}