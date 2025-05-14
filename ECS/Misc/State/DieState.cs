using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct DieState : ICharacterState
{
    public void GetCameraParameters(in CharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity)
    {
        cameraTarget = character.DefaultCameraTargetEntity;
        calculateUpFromGravity = true;
    }

    public void GetMoveVectorFromPlayerInput(in PlayerInputs inputs, quaternion cameraRotation, out float3 moveVector)
    {
        moveVector = float3.zero;
    }

    public void OnStateEnter(CharacterState previousState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        ref CharacterComponent character = ref aspect.Character.ValueRW;

        aspect.SetCapsuleGeometry(character.DieGeometry.ToCapsuleGeometry());
    }

    public void OnStateExit(CharacterState nextState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {

    }

    public void OnStatePhysicsUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        float deltaTime = baseContext.Time.DeltaTime;
        ref KinematicCharacterBody body = ref aspect.Aspect.CharacterBody.ValueRW;
        ref CharacterComponent character = ref aspect.Character.ValueRW;
        CustomGravity customGravity = aspect.CustomGravity.ValueRO;
        ref LocalTransform characterRotation = ref aspect.Aspect.LocalTransform.ValueRW;
    }

    public void OnStateVariableUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {

    }
}
