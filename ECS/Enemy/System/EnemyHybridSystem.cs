using FPS.Attribute;
using log4net.Util;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
public partial class EnemyHybridSystem : SystemBase
{
    NavMeshPath path = new NavMeshPath();

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(World.Unmanaged);

        Entity playerEntity = Entity.Null;
        foreach (var (player, entity) in SystemAPI.Query<RefRO<Player>>().WithEntityAccess())
        {
            playerEntity = player.ValueRO.ControlledCharacter;
            break;
        }

        if (playerEntity == Entity.Null)
            return;

        foreach (var (enemy, entity) in SystemAPI.Query<RefRW<Enemy>>()
            .WithNone<CharacterHP>()
            .WithEntityAccess())
        {
            CharacterControl characterControl = SystemAPI.GetComponent<CharacterControl>(entity);

            if (characterControl.DieHeId)
                break;

            var enemyData = DataContainer.EnemyData[1];

            CharacterHP hp = new CharacterHP { HP = enemyData.Hp };

            enemy.ValueRW.Gold = enemy.ValueRW.Gold = enemyData.Gold;

            ecb.AddComponent(entity, hp);
        }

        foreach (var (navAgent, entity) in 
            SystemAPI.Query<RefRW<NavAgentComponent>>()
            .WithEntityAccess())
        {
            navAgent.ValueRW.TargetEntity = playerEntity;

            var targetTransform = SystemAPI.GetComponent<LocalTransform>(navAgent.ValueRW.TargetEntity);
            var transform = SystemAPI.GetComponent<LocalTransform>(entity);
            CharacterControl characterControl = SystemAPI.GetComponent<CharacterControl>(entity);

            Vector3 start = transform.Position;
            Vector3 target = targetTransform.Position;

            if (NavMesh.CalculatePath(start, target, NavMesh.AllAreas, path))
            {
                if (path.corners.Length >= 2)
                {
                    Vector3 current = path.corners[0];
                    Vector3 next = path.corners[1];

                    float stopDistance = 1.5f;
                    float distanceToTarget = Vector3.Distance(start, target);
                    Vector3 moveVector = Vector3.zero;

                    if (distanceToTarget <= stopDistance)
                    {
                        characterControl.AttackHeId = true;
                    }
                    else
                    {
                        moveVector = (next - current).normalized;
                        characterControl.AttackHeId = false;
                    }

                    characterControl.MoveVector = moveVector;

                    ecb.SetComponent(entity, characterControl);
                }
            }
        }
    }
}
