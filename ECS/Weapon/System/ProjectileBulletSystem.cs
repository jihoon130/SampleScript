using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ProjectileBulletSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ProjectileBulletSimulationJob simulationJob = new ProjectileBulletSimulationJob
        {
            ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                        .CreateCommandBuffer(state.WorldUnmanaged),
            DelayedDespawnLookup = SystemAPI.GetComponentLookup<DelayedDespawn>(false)
        };
        state.Dependency = simulationJob.Schedule(state.Dependency);
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    [WithDisabled(typeof(DelayedDespawn))]
    public partial struct ProjectileBulletSimulationJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        public ComponentLookup<DelayedDespawn> DelayedDespawnLookup;

        void Execute(Entity entity, ref ProjectileBullet bullet, in Projectile projectile)
        {
            if (projectile.HasHit == 1)
            {
                if (projectile.HitEntity != Entity.Null)
                {
                    var evt = new AttackTag { Damage = bullet.Damage };
                    ECB.AddComponent(projectile.HitEntity, evt);
                    ECB.DestroyEntity(entity);
                }
            }
        }
    }
}
