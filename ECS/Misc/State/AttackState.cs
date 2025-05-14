using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.TextCore.Text;

public struct AttackState : ICharacterState
{
    public void OnStateEnter(CharacterState previousState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        ref CharacterComponent character = ref aspect.Character.ValueRW;

        aspect.SetCapsuleGeometry(character.StandingGeometry.ToCapsuleGeometry());
    }
    public void OnStateExit(CharacterState nextState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        ref CharacterComponent character = ref aspect.Character.ValueRW;
        character.AttackDeltaTime = 0f;
    }

    public void GetCameraParameters(in CharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity)
    {
        cameraTarget = character.DefaultCameraTargetEntity;
        calculateUpFromGravity = true;
    }

    public void GetMoveVectorFromPlayerInput(in PlayerInputs inputs, quaternion cameraRotation, out float3 moveVector)
    {
        CharacterAspect.GetCommonMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
    }

    public void OnStatePhysicsUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        ref CharacterComponent character = ref aspect.Character.ValueRW;
        ref CharacterControl characterControl = ref aspect.CharacterControl.ValueRW;
        ref float3 characterPosition = ref aspect.Aspect.LocalTransform.ValueRW.Position;
        ref quaternion characterRotation = ref aspect.Aspect.LocalTransform.ValueRW.Rotation;
        float deltaTime = baseContext.Time.DeltaTime;

        character.AttackDeltaTime += deltaTime;

        if (character.AttackFireTime <= character.AttackDeltaTime)
        {
            characterControl.AttackSuccessHeId = true;
            character.AttackDeltaTime = 0;

            float3 origin = characterPosition + new float3(0f, 1.0f, 0f);
            float3 forward = math.mul(characterRotation, new float3(0, 0, 1)); ;

            float distance = 4.0f;

            RaycastInput input = new RaycastInput
            {
                Start = origin,
                End = origin + forward * distance,
                Filter = new CollisionFilter
                { 
                    BelongsTo = ~0u,
                    CollidesWith = 1,
                    GroupIndex = 0
                }

            };

            if (baseContext.PhysicsWorld.CastRay(input, out Unity.Physics.RaycastHit hit))
            {
                context.EndFrameECB.AddComponent(context.ChunkIndex, hit.Entity, new AttackTag { Damage = character.Damage });
            }

            DetectTransitions(ref context, ref baseContext, in aspect);
            characterControl.AttackSuccessHeId = false;
        }
    }

    public void OnStateVariableUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        float deltaTime = baseContext.Time.DeltaTime;
        ref CharacterComponent character = ref aspect.Character.ValueRW;
        ref CharacterControl characterControl = ref aspect.CharacterControl.ValueRW;
        ref quaternion characterRotation = ref aspect.Aspect.LocalTransform.ValueRW.Rotation;
        CustomGravity customGravity = aspect.CustomGravity.ValueRO;

        if (math.lengthsq(characterControl.MoveVector) > 0f)
        {
            CharacterControlUtilities.SlerpRotationTowardsDirectionAroundUp(ref characterRotation, deltaTime, math.normalizesafe(characterControl.MoveVector), MathUtilities.GetUpFromRotation(characterRotation), character.GroundedRotationSharpness);
        }

        CharacterControlUtilities.SlerpCharacterUpTowardsDirection(ref characterRotation, deltaTime, math.normalizesafe(-customGravity.Gravity), character.UpOrientationAdaptationSharpness);
    }

    public bool DetectTransitions(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        ref KinematicCharacterBody characterBody = ref aspect.Aspect.CharacterBody.ValueRW;
        ref CharacterComponent character = ref aspect.Character.ValueRW;
        ref CharacterControl characterControl = ref aspect.CharacterControl.ValueRW;
        ref CharacterStateMachine stateMachine = ref aspect.StateMachine.ValueRW;

        if (characterControl.DieHeId)
        {
            stateMachine.TransitionToState(CharacterState.Die, ref context, ref baseContext, in aspect);
            return true;
        }

        if (characterBody.IsGrounded && characterControl.AttackSuccessHeId && !characterControl.AttackHeId)
        {
            stateMachine.TransitionToState(CharacterState.GroundMove, ref context, ref baseContext, in aspect);
            return true;
        }

        return aspect.DetectGlobalTransitions(ref context, ref baseContext);
    }
}
