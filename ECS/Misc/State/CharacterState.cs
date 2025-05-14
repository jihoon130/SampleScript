using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.CharacterController;
using Unity.Mathematics;

public enum CharacterState
{
    Uninitialized,

    GroundMove,
    Dashing,
    AirMove,
    Attack,
    Die
}

public interface ICharacterState
{
    void OnStateEnter(CharacterState previousState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect);
    void OnStateExit(CharacterState nextState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect);
    void OnStatePhysicsUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect);
    void OnStateVariableUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect);
    void GetCameraParameters(in CharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity);
    void GetMoveVectorFromPlayerInput(in PlayerInputs inputs, quaternion cameraRotation, out float3 moveVector);
}