using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Player : IComponentData
{
    public Entity ControlledCharacter;
    public Entity ControlledCamera;

    public EquipType EquipType;
}

[Serializable]
public struct PlayerInputs : IComponentData
{
    public float2 Move;
    public float2 LookYawPitchDegrees;
    public float2 LastProcessedLook;
    public float CameraZoom;

    public bool RollHeld;
    public bool JumpHeld;
    public bool SprintHeId;
    public bool ReloadHeId;

    public FixedInputEvent JumpPressed;
    public FixedInputEvent ShootPressed;
    public FixedInputEvent ShootReleased;
    public FixedInputEvent MainWeaponPressed;
    public FixedInputEvent SecondaryWeaponPressed;
    public FixedInputEvent ReloadPressed;
}
