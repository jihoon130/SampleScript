using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Collider = Unity.Physics.Collider;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[BurstCompile]
public partial struct DelayedDespawnSystem : ISystem
{
    [BurstCompile]
    private void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    private void OnUpdate(ref SystemState state)
    {
        DelayedDespawnJob job = new DelayedDespawnJob
        {
            ECB = SystemAPI.GetSingletonRW<BeginSimulationEntityCommandBufferSystem.Singleton>().ValueRW
                .CreateCommandBuffer(state.WorldUnmanaged),
            ChildBufferLookup = SystemAPI.GetBufferLookup<Child>(true),
            PhysicsColliderLookup = SystemAPI.GetComponentLookup<PhysicsCollider>(false),
        };
        state.Dependency = job.Schedule(state.Dependency);
    }

    [BurstCompile]
    public unsafe partial struct DelayedDespawnJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        [ReadOnly] public BufferLookup<Child> ChildBufferLookup;
        public ComponentLookup<PhysicsCollider> PhysicsColliderLookup;

        void Execute(Entity entity, ref DelayedDespawn delayedDespawn)
        {
            delayedDespawn.Ticks++;
            if (delayedDespawn.Ticks > 16)
            {
                ECB.DestroyEntity(entity);
            }

            if (delayedDespawn.HasHandledPreDespawn == 0)
            {
                // Disable collisions
                if (PhysicsColliderLookup.TryGetComponent(entity, out PhysicsCollider physicsCollider))
                {
                    ref Collider collider = ref *physicsCollider.ColliderPtr;
                    collider.SetCollisionResponse(CollisionResponsePolicy.None);
                }

                delayedDespawn.HasHandledPreDespawn = 1;
            }
        }
    }
}