using Unity.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public partial struct WeaponsSimulationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerInputs, player) in SystemAPI.Query<RefRW<PlayerInputs>, Player>()
             .WithAll<Simulate>())
        {
            if (SystemAPI.HasComponent<CharacterControl>(player.ControlledCharacter) && SystemAPI.HasComponent<CharacterStateMachine>(player.ControlledCharacter))
            {
                CharacterControl characterControl = SystemAPI.GetComponent<CharacterControl>(player.ControlledCharacter);
                BaseWeaponSimulationJob job = new BaseWeaponSimulationJob
                {
                    DeltaTime = SystemAPI.Time.DeltaTime,
                    LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
                    ParentLookup = SystemAPI.GetComponentLookup<Parent>(true),
                    PostTransformMatrixLookup = SystemAPI.GetComponentLookup<PostTransformMatrix>(true),
                    CharacterControl = characterControl,
                };
                job.ScheduleParallel();
                var debugPositions = new NativeList<float3>(Allocator.TempJob);
                var prefabWeaponSimulationJob = new PrefabWeaponSimulationJob
                {
                    ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                        .CreateCommandBuffer(state.WorldUnmanaged),
                    ProjectileLookup = SystemAPI.GetComponentLookup<Projectile>(true),
                };
                prefabWeaponSimulationJob.Schedule();
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct BaseWeaponSimulationJob : IJobEntity
    {
        public float DeltaTime;
        [ReadOnly] public ComponentLookup<LocalTransform> LocalTransformLookup;
        [ReadOnly] public ComponentLookup<Parent> ParentLookup;
        [ReadOnly] public ComponentLookup<PostTransformMatrix> PostTransformMatrixLookup;
        public CharacterControl CharacterControl;

        void Execute(
            ref BaseWeapon baseWeapon,
            ref DynamicBuffer<WeaponProjectileEvent> projectileEvents,
            in WeaponShotSimulationOriginOverride shotSimulationOriginOverride)
        {
            projectileEvents.Clear();

            uint prevTotalShotsCount = baseWeapon.TotalShotsCount;

            if (CharacterControl.ShootPressed)
            {
                baseWeapon.IsFiring = true;
            }

            if (baseWeapon.FiringRate > 0f)
            {
                float delayBetweenShots = 1f / baseWeapon.FiringRate;

                float maxUsefulShotTimer = delayBetweenShots + DeltaTime;
                if (baseWeapon.ShotTimer < maxUsefulShotTimer)
                {
                    baseWeapon.ShotTimer += DeltaTime;
                }

                while (baseWeapon.IsFiring && baseWeapon.ShotTimer > delayBetweenShots)
                {
                    baseWeapon.TotalShotsCount++;

                    baseWeapon.ShotTimer -= delayBetweenShots;

                    if (!baseWeapon.Automatic)
                    {
                        baseWeapon.IsFiring = false;
                    }
                }
            }

            if (!baseWeapon.Automatic || CharacterControl.ShootReleased)
            {
                baseWeapon.IsFiring = false;
            }

            uint shotsToFire = baseWeapon.TotalShotsCount - prevTotalShotsCount;
            if (shotsToFire > 0)
            {
                RigidTransform shotSimulationOrigin = WeaponUtilities.GetShotSimulationOrigin(
                        baseWeapon.ShotOrigin,
                        in shotSimulationOriginOverride,
                        ref LocalTransformLookup,
                        ref ParentLookup,
                        ref PostTransformMatrixLookup);
                TransformHelpers.ComputeWorldTransformMatrix(baseWeapon.ShotOrigin, out float4x4 shotVisualsOrigin,
                    ref LocalTransformLookup, ref ParentLookup, ref PostTransformMatrixLookup);

                for (int i = 0; i < shotsToFire; i++)
                {
                    for (int j = 0; j < baseWeapon.ProjectilesPerShot; j++)
                    {
                        baseWeapon.TotalProjectilesCount++;

                        Random deterministicRandom = Random.CreateFromIndex(baseWeapon.TotalProjectilesCount);
                        quaternion shotRotationWithSpread =
                            WeaponUtilities.CalculateSpreadRotation(shotSimulationOrigin.rot,
                                baseWeapon.SpreadRadians,
                                ref deterministicRandom);

                        projectileEvents.Add(new WeaponProjectileEvent
                        {
                            ID = baseWeapon.TotalProjectilesCount,
                            SimulationPosition = shotSimulationOrigin.pos,
                            SimulationDirection = math.mul(shotRotationWithSpread, math.forward()),
                            VisualPosition = shotVisualsOrigin.Translation(),
                        });
                    }
                }
            }
        }
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct PrefabWeaponSimulationJob : IJobEntity
    {
        public EntityCommandBuffer ECB;
        [ReadOnly] public ComponentLookup<Projectile> ProjectileLookup;

        void Execute(
            Entity entity,
            in PrefabWeapon prefabWeapon,
            in DynamicBuffer<WeaponProjectileEvent> projectileEvents,
            in LocalTransform localTransform,
            in DynamicBuffer<WeaponShotIgnoredEntity> ignoredEntities)
        {
            if (ProjectileLookup.TryGetComponent(prefabWeapon.ProjectilePrefab,
                    out Projectile prefabProjectile))
            {
                for (int i = 0; i < projectileEvents.Length; i++)
                {
                    WeaponProjectileEvent projectileEvent = projectileEvents[i];
                    Entity spawnedProjectile = ECB.Instantiate(prefabWeapon.ProjectilePrefab);
                    ECB.SetComponent(spawnedProjectile, LocalTransform.FromPositionRotation(
                        projectileEvent.SimulationPosition,
                        quaternion.LookRotationSafe(projectileEvent.SimulationDirection,
                            math.mul(localTransform.Rotation, math.up()))));

                    ECB.AddComponent<FireEventTag>(entity);
                    ECB.AddComponent(spawnedProjectile, new ProjectileBullet { Damage = prefabWeapon.Damage });

                    for (int k = 0; k < ignoredEntities.Length; k++)
                    {
                        ECB.AppendToBuffer(spawnedProjectile, ignoredEntities[k]);
                    }

                    {
                        prefabProjectile.Velocity = prefabProjectile.Speed * projectileEvent.SimulationDirection;
                        prefabProjectile.VisualOffset =
                            projectileEvent.VisualPosition - projectileEvent.SimulationPosition;
                        ECB.SetComponent(spawnedProjectile, prefabProjectile);
                    }
                }
            }
        }
    }
}
