using Unity.Mathematics;
using Unity.Physics;
using Unity.Entities;
using Unity.CharacterController;

public struct AirMoveState : ICharacterState
{
    public void OnStateEnter(CharacterState previousState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        ref CharacterComponent character = ref aspect.Character.ValueRW;

        aspect.SetCapsuleGeometry(character.StandingGeometry.ToCapsuleGeometry());
    }

    public void OnStateExit(CharacterState nextState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    { }

    public void OnStatePhysicsUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        float deltaTime = baseContext.Time.DeltaTime;
        float elapsedTime = (float)baseContext.Time.ElapsedTime;
        ref KinematicCharacterBody characterBody = ref aspect.Aspect.CharacterBody.ValueRW;
        ref CharacterComponent character = ref aspect.Character.ValueRW;
        ref CharacterControl characterControl = ref aspect.CharacterControl.ValueRW;
        CustomGravity customGravity = aspect.CustomGravity.ValueRO;

        aspect.HandlePhysicsUpdatePhase1(ref context, ref baseContext, true, true);

        float3 airAcceleration = characterControl.MoveVector * character.AirAcceleration;
        if (math.lengthsq(airAcceleration) > 0f)
        {
            float3 tmpVelocity = characterBody.RelativeVelocity;
            CharacterControlUtilities.StandardAirMove(ref characterBody.RelativeVelocity, airAcceleration, character.AirMaxSpeed, characterBody.GroundingUp, deltaTime, false);

            if (aspect.Aspect.MovementWouldHitNonGroundedObstruction(in aspect, ref context, ref baseContext, characterBody.RelativeVelocity * deltaTime, out ColliderCastHit hit))
            {
                characterBody.RelativeVelocity = tmpVelocity;

                character.HasDetectedMoveAgainstWall = true;
                character.LastKnownWallNormal = hit.SurfaceNormal;
            }
        }

        {
            if (characterControl.JumpPressed)
            {
                if (character.AllowJumpAfterBecameUngrounded && elapsedTime < character.LastTimeWasGrounded + character.JumpAfterUngroundedGraceTime)
                {
                    CharacterControlUtilities.StandardJump(ref characterBody, characterBody.GroundingUp * character.GroundJumpSpeed, true, characterBody.GroundingUp);
                    character.HeldJumpTimeCounter = 0f;
                }
                else if (character.CurrentUngroundedJumps < character.MaxUngroundedJumps)
                {
                    CharacterControlUtilities.StandardJump(ref characterBody, characterBody.GroundingUp * character.AirJumpSpeed, true, characterBody.GroundingUp);
                    character.CurrentUngroundedJumps++;
                }
                else
                {
                    character.JumpPressedBeforeBecameGrounded = true;
                }

                character.AllowJumpAfterBecameUngrounded = false;
            }

            if (character.AllowHeldJumpInAir && characterControl.JumpHeld && character.HeldJumpTimeCounter < character.MaxHeldJumpTime)
            {
                characterBody.RelativeVelocity += characterBody.GroundingUp * character.JumpHeldAcceleration * deltaTime;
            }
        }

        CharacterControlUtilities.AccelerateVelocity(ref characterBody.RelativeVelocity, customGravity.Gravity, deltaTime);

        CharacterControlUtilities.ApplyDragToVelocity(ref characterBody.RelativeVelocity, deltaTime, character.AirDrag);

        aspect.HandlePhysicsUpdatePhase2(ref context, ref baseContext, true, true, true, true, true);

        DetectTransitions(ref context, ref baseContext, in aspect);
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
            CharacterControlUtilities.SlerpRotationTowardsDirectionAroundUp(ref characterRotation, deltaTime, math.normalizesafe(characterControl.MoveVector), MathUtilities.GetUpFromRotation(characterRotation), character.AirRotationSharpness);
        }
        CharacterControlUtilities.SlerpCharacterUpTowardsDirection(ref characterRotation, deltaTime, math.normalizesafe(-customGravity.Gravity), character.UpOrientationAdaptationSharpness);
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

        if (characterBody.IsGrounded)
        {
            stateMachine.TransitionToState(CharacterState.GroundMove, ref context, ref baseContext, in aspect);
            return true;
        }

        return aspect.DetectGlobalTransitions(ref context, ref baseContext);
    }
}