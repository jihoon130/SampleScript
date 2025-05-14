using FPS.Attribute;
using FPS.MVP;
using UniRx;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class PlatformerPlayerInputsSystem : SystemBase
{
    private PlayerCharacterActions.GamePlayerMapActions _defaultActionsMap;
    [SingletonInject] public PlayerModel playerModel;

    protected override void OnCreate()
    {
        ModelContainer.InjectDependencies(this);
        PlayerCharacterActions inputActions = new PlayerCharacterActions();
        inputActions.Enable();
        inputActions.GamePlayerMap.Enable();
        _defaultActionsMap = inputActions.GamePlayerMap;

        MessageBroker.Default.Receive<InputEnableEvent>().Subscribe(_ =>
        {
            inputActions.Enable();
            inputActions.GamePlayerMap.Enable();
        });

        MessageBroker.Default.Receive<InputDisableEvent>().Subscribe(_ =>
        {
                inputActions.Disable();
                inputActions.GamePlayerMap.Disable();
        });

        RequireForUpdate<FixedTickSystem.Singleton>();
        RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Player, PlayerInputs>().Build());
    }

    protected override void OnUpdate()
    {
        uint fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

        foreach (var (playerInputs, player) in SystemAPI.Query<RefRW<PlayerInputs>, Player>())
        {
            playerInputs.ValueRW.Move = Vector2.ClampMagnitude(_defaultActionsMap.Move.ReadValue<Vector2>(), 1f);

            {
                if (math.lengthsq(_defaultActionsMap.LookConst.ReadValue<Vector2>()) >
                    math.lengthsq(_defaultActionsMap.LookDelta.ReadValue<Vector2>()))
                {

                    CharacterUtilities.AddInputDelta(ref playerInputs.ValueRW.LookYawPitchDegrees,
                            (float2)_defaultActionsMap.LookConst.ReadValue<Vector2>() * SystemAPI.Time.DeltaTime *
                            2f);
                }
                else
                {
                    CharacterUtilities.AddInputDelta(ref playerInputs.ValueRW.LookYawPitchDegrees,
                            (float2)_defaultActionsMap.LookDelta.ReadValue<Vector2>() *
                            2f);
                }
            }
            playerInputs.ValueRW.JumpHeld = _defaultActionsMap.Jump.IsPressed();
            playerInputs.ValueRW.SprintHeId = _defaultActionsMap.Sprint.IsPressed();
            playerInputs.ValueRW.ReloadHeId = playerModel.IsReload() && _defaultActionsMap.Reload.IsPressed();

            if (_defaultActionsMap.Jump.IsPressed())
            {
                playerInputs.ValueRW.JumpPressed.Set(fixedTick);
            }

            if (_defaultActionsMap.Shoot.IsPressed()
                && playerModel.CurrentProjectileStream.Value.Value > 0)
            {
                playerInputs.ValueRW.ShootPressed.Set(fixedTick);
            }

            if (_defaultActionsMap.MainWeapon.IsPressed())
            {
                playerInputs.ValueRW.MainWeaponPressed.Set(fixedTick);
            }

            if (_defaultActionsMap.SecondaryWeapon.IsPressed())
            {
                playerInputs.ValueRW.SecondaryWeaponPressed.Set(fixedTick);
            }
        }
    }

    /// <summary>
    /// Apply inputs that need to be read at a variable rate
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct PlayerVariableStepControlSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Player, PlayerInputs>().Build());
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            uint fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

            foreach (var (playerInputs, player) in SystemAPI.Query<RefRW<PlayerInputs>, Player>())
            {
                CharacterControl characterControl = SystemAPI.GetComponent<CharacterControl>(player.ControlledCharacter);

                float2 lookYawPitchDegreesDelta = CharacterUtilities.GetInputDelta(
                        playerInputs.ValueRW.LookYawPitchDegrees,
                        playerInputs.ValueRW.LastProcessedLook);
                playerInputs.ValueRW.LastProcessedLook = playerInputs.ValueRW.LookYawPitchDegrees;

                characterControl.LookYawPitchDegreesDelta = lookYawPitchDegreesDelta;
                SystemAPI.SetComponent(player.ControlledCharacter, characterControl);
            }
        }
    }

    /// <summary>
    /// Apply inputs that need to be read at a fixed rate.
    /// It is necessary to handle this as part of the fixed step group, in case your framerate is lower than the fixed step rate.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
    [BurstCompile]
    public partial struct PlatformerPlayerFixedStepControlSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FixedTickSystem.Singleton>();
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Player, PlayerInputs>().Build());
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            uint fixedTick = SystemAPI.GetSingleton<FixedTickSystem.Singleton>().Tick;

            foreach (var (playerInputs, player) in SystemAPI.Query<RefRW<PlayerInputs>, RefRW<Player>>()
                         .WithAll<Simulate>())
            {
                if (SystemAPI.HasComponent<CharacterControl>(player.ValueRO.ControlledCharacter) && SystemAPI.HasComponent<CharacterStateMachine>(player.ValueRO.ControlledCharacter))
                {
                    CharacterControl characterControl = SystemAPI.GetComponent<CharacterControl>(player.ValueRO.ControlledCharacter);
                    CharacterStateMachine stateMachine = SystemAPI.GetComponent<CharacterStateMachine>(player.ValueRO.ControlledCharacter);

                    quaternion cameraRotation = quaternion.identity;
                    if (SystemAPI.HasComponent<LocalTransform>(player.ValueRO.ControlledCamera))
                    {
                        cameraRotation = SystemAPI.GetComponent<LocalTransform>(player.ValueRO.ControlledCharacter).Rotation;
                    }

                    stateMachine.GetMoveVectorFromPlayerInput(stateMachine.CurrentState, in playerInputs.ValueRO, cameraRotation, out characterControl.MoveVector);

                    characterControl.JumpHeld = playerInputs.ValueRW.JumpHeld;
                    characterControl.SprintHeId = playerInputs.ValueRW.SprintHeId;

                    characterControl.JumpPressed = playerInputs.ValueRW.JumpPressed.IsSet(fixedTick);
                    characterControl.ShootPressed = playerInputs.ValueRW.ShootPressed.IsSet(fixedTick);
                    characterControl.ShootReleased = playerInputs.ValueRW.ShootReleased.IsSet(fixedTick);

                    if (playerInputs.ValueRW.MainWeaponPressed.IsSet(fixedTick))
                        player.ValueRW.EquipType = EquipType.MainWeapon;

                    if (playerInputs.ValueRW.SecondaryWeaponPressed.IsSet(fixedTick))
                        player.ValueRW.EquipType = EquipType.SecondaryWeapon;

                    SystemAPI.SetComponent(player.ValueRO.ControlledCharacter, characterControl);
                }
            }
        }
    }
}