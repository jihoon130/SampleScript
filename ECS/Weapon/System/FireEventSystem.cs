using UniRx;
using Unity.Entities;
using UnityEngine;

public struct FireEventTag : IComponentData { }

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class FireEventSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(World.Unmanaged);

        foreach (var (_, entity) in SystemAPI.Query<RefRO<FireEventTag>>().WithEntityAccess())
        {
            MessageBroker.Default.Publish(new FireEvent { });
            ecb.RemoveComponent<FireEventTag>(entity);
        }
    }
}
