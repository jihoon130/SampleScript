using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using Unity.Physics.GraphicsIntegration;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[BurstCompile]
public partial struct SceneInitializationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    { }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    { }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<SceneInitialization>())
        {
            ref SceneInitialization sceneInitializer = ref SystemAPI.GetSingletonRW<SceneInitialization>().ValueRW;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Spawn player
            Entity playerEntity = state.EntityManager.Instantiate(sceneInitializer.PlayerPrefabEntity);

            Entity characterEntity = state.EntityManager.Instantiate(sceneInitializer.CharacterPrefabEntity);
            LocalTransform spawnTransform = SystemAPI.GetComponent<LocalTransform>(sceneInitializer.CharacterSpawnPointEntity);
            SystemAPI.SetComponent(characterEntity, LocalTransform.FromPositionRotation(spawnTransform.Position, spawnTransform.Rotation));

            Entity cameraEntity = state.EntityManager.Instantiate(sceneInitializer.CameraPrefabEntity);
            state.EntityManager.AddComponentData(cameraEntity, new MainEntityCamera());

            Player player = SystemAPI.GetComponent<Player>(playerEntity);
            player.ControlledCharacter = characterEntity;
            player.ControlledCamera = SystemAPI.GetComponent<CharacterComponent>(characterEntity).DefaultCameraTargetEntity;
            SystemAPI.SetComponent(playerEntity, player);

            state.EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<SceneInitialization>());
        }
    }
}