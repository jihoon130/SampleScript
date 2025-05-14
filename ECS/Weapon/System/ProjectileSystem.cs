using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct ProjectileSimulationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        ProjectileSimulationsJob job = new ProjectileSimulationsJob
        {
            DeltaTime = SystemAPI.Time.DeltaTime,
            PhysicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld,
            DelayedDespawnLookup = SystemAPI.GetComponentLookup<DelayedDespawn>(false),
        };

        job.ScheduleParallel();
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct ProjectileSimulationsJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public float DeltaTime;
        [ReadOnly] public PhysicsWorld PhysicsWorld;

        [NativeDisableContainerSafetyRestriction]
        private NativeList<RaycastHit> Hits;
        [NativeDisableParallelForRestriction] public ComponentLookup<DelayedDespawn> DelayedDespawnLookup;

        void Execute(Entity entity, ref Projectile projectile, ref LocalTransform localTransform,
            in DynamicBuffer<WeaponShotIgnoredEntity> ignoredEntities)
        {
            if (projectile.HasHit == 0)
            {
                // Movement
                projectile.Velocity += (math.up() * projectile.Gravity) * DeltaTime;
                float3 displacement = projectile.Velocity * DeltaTime;

                // Hit detection
                Hits.Clear();
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = localTransform.Position,
                    End = localTransform.Position + displacement,
                    Filter = CollisionFilter.Default,
                };
                PhysicsWorld.CastRay(raycastInput, ref Hits);
                if (WeaponUtilities.GetClosestValidWeaponRaycastHit(in Hits, in ignoredEntities,
                        out RaycastHit closestValidHit))
                {
                    displacement *= closestValidHit.Fraction;
                    projectile.HitEntity = closestValidHit.Entity;
                    projectile.HasHit = 1;
                }

                // Advance position 
                localTransform.Position += displacement;
            }

            // Lifetime
            projectile.LifetimeCounter += DeltaTime;
            if (projectile.LifetimeCounter >= projectile.MaxLifeTime)
            {
                DelayedDespawnLookup.SetComponentEnabled(entity, true);
            }
        }

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask)
        {
            if (!Hits.IsCreated)
            {
                Hits = new NativeList<RaycastHit>(128, Allocator.Temp);
            }

            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask,
            bool chunkWasExecuted)
        {
        }
    }
}

[BurstCompile]
[UpdateInGroup(typeof(TransformSystemGroup))]
[UpdateBefore(typeof(LocalToWorldSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct PrefabProjectileVisualsSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PrefabProjectileVisualsJob job = new PrefabProjectileVisualsJob
        { };
        state.Dependency = job.Schedule(state.Dependency);
    }

    [BurstCompile]
    public partial struct PrefabProjectileVisualsJob : IJobEntity
    {
        void Execute(ref LocalToWorld ltw, in LocalTransform transform, in Projectile projectile)
        {
            float3 visualOffset = math.lerp(projectile.VisualOffset, float3.zero,
                math.saturate(projectile.LifetimeCounter / projectile.VisualOffsetCorrectionDuration));
            float4x4 visualOffsetTransform = float4x4.Translate(visualOffset);
            ltw.Value = math.mul(visualOffsetTransform,
                float4x4.TRS(transform.Position,
                    quaternion.LookRotationSafe(math.normalizesafe(projectile.Velocity), math.up()),
                    transform.Scale));
        }
    }
}