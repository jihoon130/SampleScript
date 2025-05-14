using Cysharp.Threading.Tasks;
using FPS.Attribute;
using FPS.MVP;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
public partial class PlayerHybridSystem : SystemBase
{
    [SingletonInject] public PlayerModel playerModel;
    private FPSPlayerSettings playerSettings;

    protected override void OnCreate()
    {
        ModelContainer.InjectDependencies(this);
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(World.Unmanaged);
        uint fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

        foreach (var (player, inputs, entity) in
            SystemAPI.Query<RefRO<Player>, RefRW<PlayerInputs>>()
            .WithNone<PlayerHybridLink>()
            .WithEntityAccess())
        {
            PlayerView playerView = GameObject.FindAnyObjectByType<PlayerView>();

            playerSettings = AddressableManager.Instance.Get<FPSPlayerSettings>("PlayerData", "PlayerData");

            var hybridLink = new PlayerHybridLink
            {
                playerView = playerView,
            };

            var playerData = DataContainer.PlayerData["DefaultHP"];

            CharacterHP hp = new CharacterHP { HP = playerData.Value };
            CharacterComponent characterComponent = SystemAPI.GetComponent<CharacterComponent>(player.ValueRO.ControlledCharacter);


            ecb.AddComponent(player.ValueRO.ControlledCharacter, hp);
            ecb.AddComponent(player.ValueRO.ControlledCharacter, new PlayerTag { });
            ecb.AddComponent(player.ValueRO.ControlledCamera, new CameraShake { RootEntity = player.ValueRO.ControlledCharacter, CameraBone = characterComponent.CameraBoneEntity });
            playerModel.SetMaxHP(playerData.Value);
            ecb.AddComponent(entity, hybridLink);
        }

        foreach (var (player, audio, hybrid) in
            SystemAPI.Query<RefRO<Player>, RefRW<PlayerAudio>, PlayerHybridLink>())
        {
            if (EntityManager.HasComponent<CharacterHybridLink>(player.ValueRO.ControlledCharacter))
            {
                var characterControl = SystemAPI.GetComponent<CharacterControl>(player.ValueRO.ControlledCharacter);
                var characterStateMachine = SystemAPI.GetComponent<CharacterStateMachine>(player.ValueRO.ControlledCharacter);
                var hp = SystemAPI.GetComponent<CharacterHP>(player.ValueRO.ControlledCharacter);

                //LocalToWorld cameraLTW = SystemAPI.GetComponent<LocalToWorld>(player.ValueRO.ControlledCamera);
                //hybrid.playerView.CameraTransform.position = cameraLTW.Position;
                //hybrid.playerView.CameraTransform.rotation = cameraLTW.Rotation;

                playerModel.ChangeEquipType(player.ValueRO.EquipType);
                playerModel.SetHP(hp.HP);
                CharacterAudioHandler.PlayAudio(hybrid.playerView.AudioSource, playerSettings, ref audio.ValueRW, characterStateMachine, characterControl);
            }
        }

    }
}