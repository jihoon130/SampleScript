using FPS.Attribute;
using FPS.MVP;
using Unity.Entities;
using UnityEngine;
using UniRx;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class EnemyHPSystem : SystemBase
{
    [SingletonInject] private PlayerModel playerModel;
    protected override void OnCreate()
    {
        ModelContainer.InjectDependencies(this);
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(World.Unmanaged);
        foreach (var (hp, enemy, entity) in 
            SystemAPI.Query<CharacterHP, Enemy>()
            .WithEntityAccess())
        {
            if (hp.HP <= 0)
            {
                playerModel.AddMoney(enemy.Gold);
                CharacterControl characterControl = SystemAPI.GetComponent<CharacterControl>(entity);
                characterControl.DieHeId = true;

                MessageBroker.Default.Publish(new EnemyDieEvent { });

                ecb.SetComponent(entity, characterControl);
                ecb.RemoveComponent<CharacterHP>(entity);
            }
        }
    }
}
