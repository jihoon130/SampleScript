using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct AttackSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AttackTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>()
                           .ValueRW.CreateCommandBuffer(state.WorldUnmanaged);

        var hpLookup = state.GetComponentLookup<CharacterHP>(isReadOnly: false);

        foreach (var (evt, entity) in SystemAPI.Query<RefRO<AttackTag>>().WithEntityAccess())
        {
            if (hpLookup.HasComponent(entity))
            {
                var hp = hpLookup[entity];
                hp.HP -= evt.ValueRO.Damage;

                ecb.SetComponent(entity, hp);
                ecb.RemoveComponent<AttackTag>(entity);
            }
        }
    }
}
