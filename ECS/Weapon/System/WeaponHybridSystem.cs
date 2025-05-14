using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;
using Unity.Transforms;
using FPS.Attribute;
using FPS.MVP;
using Unity.Mathematics;
using UniRx;

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
public partial class WeaponHybridSystem : SystemBase
{
    [SingletonInject] public PlayerModel playerModel;
    private WeaponData weaponData;

    protected override void OnCreate()
    {
        ModelContainer.InjectDependencies(this);
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(World.Unmanaged);

        foreach (var (baseWeapon, entity) in SystemAPI.Query<BaseWeapon>()
            .WithNone<WeaponHybridLink>()
            .WithEntityAccess())
        {
            Entity playerEntity = SystemAPI.GetSingletonEntity<Player>();
            if (EntityManager.HasComponent<PlayerHybridLink>(playerEntity))
            {
                var playerHybridLink = EntityManager.GetComponentObject<PlayerHybridLink>(playerEntity);
                var weapons = AddressableManager.Instance.InitGameObjects("Weapon", playerHybridLink.playerView.WeaponPart, false);

                playerModel.EquipType.Subscribe(_ =>
                {
                    if (_ == EquipType.None)
                        return;

                    EntityCommandBuffer ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(World.Unmanaged);
                    var equipWeaponName = playerModel.EquipType.Value == EquipType.MainWeapon ? playerModel.MainWeapon.Value.ToString() : playerModel.SecondaryWeapon.Value.ToString();

                    weaponData = DataContainer.WeaponData[equipWeaponName];

                    Entity weaponEntity = SystemAPI.GetSingletonEntity<PrefabWeapon>();

                    var prefabWeapon = SystemAPI.GetComponent<PrefabWeapon>(weaponEntity);


                    prefabWeapon.Damage = weaponData.Damage;
                    ecb.SetComponent(weaponEntity, prefabWeapon);
                });

                playerModel.Weapons = weapons;
                playerModel.SetWeapon(WeaponType.MX16A4);
                playerModel.SetWeapon(WeaponType.M1911);
                playerModel.ChangeEquipType(EquipType.SecondaryWeapon);

                Dictionary<string, WeaponView> viewDict = new Dictionary<string, WeaponView>();
                Dictionary<string, FPSWeaponSettings> settingDict = new Dictionary<string, FPSWeaponSettings>();

                foreach (var weapon in weapons)
                {
                    viewDict[weapon.gameObject.name] = weapon.GetComponent<WeaponView>();
                    settingDict[weapon.gameObject.name] = AddressableManager.Instance.Get<FPSWeaponSettings>("WeaponData", weapon.gameObject.name);
                }

                WeaponHybridLink hybridLink = new WeaponHybridLink
                {
                    WeaponViews = viewDict,
                    WeaponSettings = settingDict
                };

                ecb.AddComponent(entity, hybridLink);
            }
        }

        foreach (var (baseWeapon, entity) in SystemAPI.Query<RefRW<BaseWeapon>>().WithEntityAccess())
        {
            if (EntityManager.HasComponent<WeaponHybridLink>(entity))
            {
                var hybridLink = EntityManager.GetComponentObject<WeaponHybridLink>(entity);
                var equipWeaponName = playerModel.EquipType.Value == EquipType.MainWeapon ? playerModel.MainWeapon.Value.ToString() : playerModel.SecondaryWeapon.Value.ToString();

                if (weaponData != null)
                {
                    baseWeapon.ValueRW.FiringRate = weaponData.FiringRate;
                    baseWeapon.ValueRW.SpreadRadians = weaponData.SpreadRadians;
                    baseWeapon.ValueRW.Automatic = weaponData.Automatic;
                    baseWeapon.ValueRW.ProjectilesPerShot = weaponData.ProjectilesPerShot;
                }

                if (EntityManager.HasComponent<Parent>(baseWeapon.ValueRO.ShotOrigin))
                {
                    var parent = EntityManager.GetComponentData<Parent>(baseWeapon.ValueRO.ShotOrigin);
                    var parentLocalToWorld = EntityManager.GetComponentData<LocalToWorld>(parent.Value);

                    float4x4 parentMatrix = parentLocalToWorld.Value;
                    float4x4 parentInverse = math.inverse(parentMatrix);

                    float3 localPosition = math.transform(parentInverse, hybridLink.WeaponViews[equipWeaponName].ShootOrigin.position);

                    var correctedLocalTransform = LocalTransform.FromPosition(localPosition);

                    ecb.SetComponent(baseWeapon.ValueRO.ShotOrigin, correctedLocalTransform);
                }
                else
                {
                    var correctedLocalTransform = LocalTransform.FromPosition(hybridLink.WeaponViews[equipWeaponName].ShootOrigin.position);
                    ecb.SetComponent(baseWeapon.ValueRO.ShotOrigin, correctedLocalTransform);
                }
            }
        }

    }
}
