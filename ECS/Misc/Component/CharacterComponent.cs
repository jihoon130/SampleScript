using System;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

[Serializable]
public struct CharacterComponent : IComponentData
{
    public Entity MeshRootEntity;
    public Entity RollBallMeshEntity;
    public Entity DefaultCameraTargetEntity;
    public Entity CameraBoneEntity;

    public float GroundRunMaxSpeed;
    public float GroundSprintMaxSpeed;
    public float GroundedMovementSharpness;
    public float GroundedRotationSharpness;

    public float AirAcceleration;
    public float AirMaxSpeed;
    public float AirDrag;
    public float AirRotationSharpness;

    public float GroundJumpSpeed;
    public float AirJumpSpeed;
    public float WallRunJumpSpeed;
    public float JumpHeldAcceleration;
    public float MaxHeldJumpTime;
    public byte MaxUngroundedJumps;
    public float JumpAfterUngroundedGraceTime;
    public float JumpBeforeGroundedGraceTime;

    public int Damage;

    public float MinViewAngle;
    public float MaxViewAngle;

    public float AttackFireTime;

    public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling;

    public float UpOrientationAdaptationSharpness;
    public CapsuleGeometryDefinition StandingGeometry;
    public CapsuleGeometryDefinition DieGeometry;

    [HideInInspector]
    public bool JumpPressedBeforeBecameGrounded;
    [HideInInspector]
    public bool AllowJumpAfterBecameUngrounded;
    [HideInInspector]
    public float LastTimeJumpPressed;
    [HideInInspector]
    public bool HasDetectedMoveAgainstWall;
    [HideInInspector]
    public float3 LastKnownWallNormal;
    [HideInInspector]
    public float LastTimeWasGrounded; 
    [HideInInspector]
    public float HeldJumpTimeCounter; 
    [HideInInspector]
    public byte CurrentUngroundedJumps;
    [HideInInspector]
    public bool AllowHeldJumpInAir;
    [HideInInspector]
    public bool IsOnStickySurface;
    [HideInInspector]
    public bool IsLoading;
    [HideInInspector]
    public float AttackDeltaTime;

    [HideInInspector]
    public float ViewPitchDegrees;

    [HideInInspector]
    public float CharacterYDegrees;

    [HideInInspector]
    public float WalkDelayTime;

    [HideInInspector] 
    public quaternion ViewLocalRotation;
}

[Serializable]
public struct CharacterControl : IComponentData
{
    public float3 MoveVector;
    public float2 LookYawPitchDegreesDelta;

    public bool AttackHeId;
    public bool DieHeId;
    public bool AttackSuccessHeId;

    public bool JumpHeld;
    public bool SprintHeId;
    public bool JumpPressed;
    public bool ShootPressed;
    public bool ShootReleased;
    public bool DashPressed;
    public bool SprintPressed;
}

public struct CharacterInitialized : IComponentData
{

}

[Serializable]
public struct CapsuleGeometryDefinition
{
    public float Radius;
    public float Height;
    public float3 Center;


    public CapsuleGeometry ToCapsuleGeometry()
    {
        Height = math.max(Height, (Radius + math.EPSILON) * 2f);
        float halfHeight = Height * 0.5f;

        return new CapsuleGeometry
        {
            Radius = Radius,
            Vertex0 = Center + (-math.up() * (halfHeight - Radius)),
            Vertex1 = Center + (math.up() * (halfHeight - Radius)),
        };
    }
}