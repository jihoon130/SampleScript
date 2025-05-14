using Unity.Entities;
using Unity.CharacterController;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public struct GroundMoveState : ICharacterState
{
    public void OnStateEnter(CharacterState previousState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        ref CharacterComponent character = ref aspect.Character.ValueRW;
        ref CharacterControl characterControl = ref aspect.CharacterControl.ValueRW;
        aspect.SetCapsuleGeometry(character.StandingGeometry.ToCapsuleGeometry());
    }

    public void OnStateExit(CharacterState nextState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        ref CharacterControl character = ref aspect.CharacterControl.ValueRW;

        character.SprintHeId = false;
    }

    public void OnStatePhysicsUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        float deltaTime = baseContext.Time.DeltaTime;
        float elapsedTime = (float)baseContext.Time.ElapsedTime;
        ref KinematicCharacterBody characterBody = ref aspect.Aspect.CharacterBody.ValueRW;
        ref CharacterComponent character = ref aspect.Character.ValueRW;
        ref CharacterControl characterControl = ref aspect.CharacterControl.ValueRW;

        aspect.HandlePhysicsUpdatePhase1(ref context, ref baseContext, true, true);

        if (characterBody.ParentEntity != Entity.Null)
        {
            characterControl.MoveVector = math.rotate(characterBody.RotationFromParent, characterControl.MoveVector);
            characterBody.RelativeVelocity = math.rotate(characterBody.RotationFromParent, characterBody.RelativeVelocity);
        }

        if (characterBody.IsGrounded)
        {
            {
                float chosenMaxSpeed = characterControl.SprintHeId ? character.GroundSprintMaxSpeed : character.GroundRunMaxSpeed;

                float chosenSharpness = character.GroundedMovementSharpness;
                if (context.CharacterFrictionModifierLookup.TryGetComponent(characterBody.GroundHit.Entity, out CharacterFrictionModifier frictionModifier))
                {
                    chosenSharpness *= frictionModifier.Friction;
                }

                float3 moveVectorOnPlane = math.normalizesafe(MathUtilities.ProjectOnPlane(characterControl.MoveVector, characterBody.GroundingUp)) * math.length(characterControl.MoveVector);
                float3 targetVelocity = moveVectorOnPlane * chosenMaxSpeed;
                CharacterControlUtilities.StandardGroundMove_Interpolated(ref characterBody.RelativeVelocity, targetVelocity, chosenSharpness, deltaTime, characterBody.GroundingUp, characterBody.GroundHit.Normal);
            }

            if (characterControl.JumpPressed ||
                (character.JumpPressedBeforeBecameGrounded && elapsedTime < character.LastTimeJumpPressed + character.JumpBeforeGroundedGraceTime))
            {
                CharacterControlUtilities.StandardJump(ref characterBody, characterBody.GroundingUp * character.GroundJumpSpeed, true, characterBody.GroundingUp);
                character.AllowJumpAfterBecameUngrounded = false;
            }
        }

        aspect.HandlePhysicsUpdatePhase2(ref context, ref baseContext, true, true, true, true, true);

        DetectTransitions(ref context, ref baseContext, in aspect);
    }

    public void OnStateVariableUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        float deltaTime = baseContext.Time.DeltaTime;
        ref KinematicCharacterBody characterBody = ref aspect.Aspect.CharacterBody.ValueRW;
        ref CharacterComponent character = ref aspect.Character.ValueRW;
        ref CharacterControl characterControl = ref aspect.CharacterControl.ValueRW;
        ref quaternion characterRotation = ref aspect.Aspect.LocalTransform.ValueRW.Rotation;
        CustomGravity customGravity = aspect.CustomGravity.ValueRO;

        if (character.MinViewAngle != 0)
        {
            CharacterUtilities.ComputeFinalRotationsFromRotationDelta(
                        ref character.ViewPitchDegrees,
                        ref character.CharacterYDegrees,
                        math.up(),
                        characterControl.LookYawPitchDegreesDelta,
                        0,
                        character.MinViewAngle,
                        character.MaxViewAngle,
                        out characterRotation,
                        out float canceledPitchDegrees,
                        out character.ViewLocalRotation);

        }
        else
        {
            if (math.lengthsq(characterControl.MoveVector) > 0f)
            {
                CharacterControlUtilities.SlerpRotationTowardsDirectionAroundUp(ref characterRotation, deltaTime, math.normalizesafe(characterControl.MoveVector), MathUtilities.GetUpFromRotation(characterRotation), character.GroundedRotationSharpness);
            }

            CharacterControlUtilities.SlerpCharacterUpTowardsDirection(ref characterRotation, deltaTime, math.normalizesafe(-customGravity.Gravity), character.UpOrientationAdaptationSharpness);
        }
    }

    public void GetCameraParameters(in CharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity)
    {
        cameraTarget = character.DefaultCameraTargetEntity;
        calculateUpFromGravity = !character.IsOnStickySurface;
    }

    public void GetMoveVectorFromPlayerInput(in PlayerInputs inputs, quaternion cameraRotation, out float3 moveVector)
    {
        CharacterAspect.GetCommonMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
    }

    public bool DetectTransitions(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        ref KinematicCharacterBody characterBody = ref aspect.Aspect.CharacterBody.ValueRW;
        ref CharacterControl characterControl = ref aspect.CharacterControl.ValueRW;
        ref CharacterStateMachine stateMachine = ref aspect.StateMachine.ValueRW;

        if (characterControl.DieHeId)
        {
            stateMachine.TransitionToState(CharacterState.Die, ref context, ref baseContext, in aspect);
            return true;
        }

        if (characterControl.DashPressed)
        {
            stateMachine.TransitionToState(CharacterState.Dashing, ref context, ref baseContext, in aspect);
            return true;
        }

        if (characterControl.AttackHeId)
        {
            stateMachine.TransitionToState(CharacterState.Attack, ref context, ref baseContext, in aspect);
            return true;
        }

        if (!characterBody.IsGrounded)
        {
            stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in aspect);
            return true;
        }

        return aspect.DetectGlobalTransitions(ref context, ref baseContext);
    }
}