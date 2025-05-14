using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.CharacterController;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using CapsuleCollider = Unity.Physics.CapsuleCollider;
using Material = Unity.Physics.Material;
using UnityEditorInternal;
using UnityEngine.TextCore.Text;

public struct CharacterUpdateContext
{
    public int ChunkIndex;
    public EntityCommandBuffer.ParallelWriter EndFrameECB;
    [ReadOnly]
    public ComponentLookup<CharacterFrictionModifier> CharacterFrictionModifierLookup;
    [ReadOnly]
    public BufferLookup<LinkedEntityGroup> LinkedEntityGroupLookup;

    public void SetChunkIndex(int chunkIndex)
    {
        ChunkIndex = chunkIndex;
    }

    public void OnSystemCreate(ref SystemState state)
    {
        CharacterFrictionModifierLookup = state.GetComponentLookup<CharacterFrictionModifier>(true);
        LinkedEntityGroupLookup = state.GetBufferLookup<LinkedEntityGroup>(true);
        
    }

    public void OnSystemUpdate(ref SystemState state, EntityCommandBuffer endFrameECB)
    {
        EndFrameECB = endFrameECB.AsParallelWriter();
        CharacterFrictionModifierLookup.Update(ref state);
        LinkedEntityGroupLookup.Update(ref state);
    }
}

public readonly partial struct CharacterAspect : IAspect, IKinematicCharacterProcessor<CharacterUpdateContext>
{
    public readonly KinematicCharacterAspect Aspect;
    public readonly RefRW<CharacterComponent> Character;
    public readonly RefRW<CharacterStateMachine> StateMachine;
    public readonly RefRW<CharacterControl> CharacterControl;
    public readonly RefRW<CustomGravity> CustomGravity;

    public void PhysicsUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        ref KinematicCharacterBody characterBody = ref Aspect.CharacterBody.ValueRW;
        ref CharacterComponent character = ref Character.ValueRW;
        ref CharacterControl characterControl = ref CharacterControl.ValueRW;
        ref CharacterStateMachine stateMachine = ref StateMachine.ValueRW;

        if (stateMachine.CurrentState == CharacterState.Uninitialized)
        {
            stateMachine.TransitionToState(CharacterState.AirMove, ref context, ref baseContext, in this);
        }

        stateMachine.OnStatePhysicsUpdate(stateMachine.CurrentState, ref context, ref baseContext, in this);
    }

    public void VariableUpdate(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        ref KinematicCharacterBody characterBody = ref Aspect.CharacterBody.ValueRW;
        ref CharacterStateMachine stateMachine = ref StateMachine.ValueRW;
        ref quaternion characterRotation = ref Aspect.LocalTransform.ValueRW.Rotation;
        ref CharacterComponent character = ref Character.ValueRW;
        ref CharacterControl characterControl = ref CharacterControl.ValueRW;

        stateMachine.OnStateVariableUpdate(stateMachine.CurrentState, ref context, ref baseContext, in this);
    }

    public bool DetectGlobalTransitions(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        ref CharacterStateMachine stateMachine = ref StateMachine.ValueRW;
        ref CharacterControl characterControl = ref CharacterControl.ValueRW;

        return false;
    }

    public void HandlePhysicsUpdatePhase1(
        ref CharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        bool allowParentHandling,
        bool allowGroundingDetection)
    {
        ref KinematicCharacterBody characterBody = ref Aspect.CharacterBody.ValueRW;
        ref float3 characterPosition = ref Aspect.LocalTransform.ValueRW.Position;

        Aspect.Update_Initialize(in this, ref context, ref baseContext, ref characterBody, baseContext.Time.DeltaTime);
        if (allowParentHandling)
        {
            Aspect.Update_ParentMovement(in this, ref context, ref baseContext, ref characterBody, ref characterPosition, characterBody.WasGroundedBeforeCharacterUpdate);
        }
        if (allowGroundingDetection)
        {
            Aspect.Update_Grounding(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);
        }
    }

    public void HandlePhysicsUpdatePhase2(
        ref CharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        bool allowPreventGroundingFromFutureSlopeChange,
        bool allowGroundingPushing,
        bool allowMovementAndDecollisions,
        bool allowMovingPlatformDetection,
        bool allowParentHandling)
    {
        ref CharacterComponent character = ref Character.ValueRW;
        ref KinematicCharacterBody characterBody = ref Aspect.CharacterBody.ValueRW;
        ref float3 characterPosition = ref Aspect.LocalTransform.ValueRW.Position;
        CustomGravity customGravity = CustomGravity.ValueRO;

        if (allowPreventGroundingFromFutureSlopeChange)
        {
            Aspect.Update_PreventGroundingFromFutureSlopeChange(in this, ref context, ref baseContext, ref characterBody, in character.StepAndSlopeHandling);
        }
        if (allowGroundingPushing)
        {
            Aspect.Update_GroundPushing(in this, ref context, ref baseContext, customGravity.Gravity);
        }
        if (allowMovementAndDecollisions)
        {
            Aspect.Update_MovementAndDecollisions(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);
        }
        if (allowMovingPlatformDetection)
        {
            Aspect.Update_MovingPlatformDetection(ref baseContext, ref characterBody);
        }
        if (allowParentHandling)
        {
            Aspect.Update_ParentMomentum(ref baseContext, ref characterBody);
        }
        Aspect.Update_ProcessStatefulCharacterHits();
    }

    public unsafe void SetCapsuleGeometry(CapsuleGeometry capsuleGeometry)
    {
        ref PhysicsCollider physicsCollider = ref Aspect.PhysicsCollider.ValueRW;

        CapsuleCollider* capsuleCollider = (CapsuleCollider*)physicsCollider.ColliderPtr;
        capsuleCollider->Geometry = capsuleGeometry;
    }

    public float3 GetGeometryCenter(CapsuleGeometryDefinition geometry)
    {
        float3 characterPosition = Aspect.LocalTransform.ValueRW.Position;
        quaternion characterRotation = Aspect.LocalTransform.ValueRW.Rotation;

        RigidTransform characterTransform = new RigidTransform(characterRotation, characterPosition);
        float3 geometryCenter = math.transform(characterTransform, geometry.Center);
        return geometryCenter;
    }

    public unsafe bool CanStandUp(ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
    {
        ref PhysicsCollider physicsCollider = ref Aspect.PhysicsCollider.ValueRW;
        ref CharacterComponent character = ref Character.ValueRW;
        ref float3 characterPosition = ref Aspect.LocalTransform.ValueRW.Position;
        ref quaternion characterRotation = ref Aspect.LocalTransform.ValueRW.Rotation;
        float characterScale = Aspect.LocalTransform.ValueRO.Scale;
        ref KinematicCharacterProperties characterProperties = ref Aspect.CharacterProperties.ValueRW;

        // Overlap test with standing geometry to see if we have space to stand
        CapsuleCollider* capsuleCollider = ((CapsuleCollider*)physicsCollider.ColliderPtr);

        CapsuleGeometry initialGeometry = capsuleCollider->Geometry;
        capsuleCollider->Geometry = character.StandingGeometry.ToCapsuleGeometry();

        bool isObstructed = false;
        if (Aspect.CalculateDistanceClosestCollisions(
                in this,
                ref context,
                ref baseContext,
                characterPosition,
                characterRotation,
                characterScale,
                0f,
                characterProperties.ShouldIgnoreDynamicBodies(),
                out DistanceHit hit))
        {
            isObstructed = true;
        }

        capsuleCollider->Geometry = initialGeometry;

        return !isObstructed;
    }

    public static CapsuleGeometry CreateCharacterCapsuleGeometry(float radius, float height, bool centered)
    {
        height = math.max(height, radius * 2f);
        float halfHeight = height * 0.5f;

        return new CapsuleGeometry
        {
            Radius = radius,
            Vertex0 = centered ? (-math.up() * (halfHeight - radius)) : (math.up() * radius),
            Vertex1 = centered ? (math.up() * (halfHeight - radius)) : (math.up() * (height - radius)),
        };
    }

    public static void GetCommonMoveVectorFromPlayerInput(in PlayerInputs inputs, quaternion cameraRotation, out float3 moveVector)
    {
        moveVector = (math.mul(cameraRotation, math.right()) * inputs.Move.x) + (math.mul(cameraRotation, math.forward()) * inputs.Move.y);
    }

    #region Character Processor Callbacks
    public void UpdateGroundingUp(
        ref CharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext)
    {
        ref KinematicCharacterBody characterBody = ref Aspect.CharacterBody.ValueRW;

        Aspect.Default_UpdateGroundingUp(ref characterBody);
    }

    public bool CanCollideWithHit(
        ref CharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        in BasicHit hit)
    {
        return PhysicsUtilities.IsCollidable(hit.Material);
    }

    public bool IsGroundedOnHit(
        ref CharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        in BasicHit hit,
        int groundingEvaluationType)
    {
        CharacterComponent characterComponent = Character.ValueRO;

        return Aspect.Default_IsGroundedOnHit(
            in this,
            ref context,
            ref baseContext,
            in hit,
            in characterComponent.StepAndSlopeHandling,
            groundingEvaluationType);
    }

    public void OnMovementHit(
            ref CharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            ref KinematicCharacterHit hit,
            ref float3 remainingMovementDirection,
            ref float remainingMovementLength,
            float3 originalVelocityDirection,
            float hitDistance)
    {
        ref KinematicCharacterBody characterBody = ref Aspect.CharacterBody.ValueRW;
        ref float3 characterPosition = ref Aspect.LocalTransform.ValueRW.Position;
        CharacterComponent characterComponent = Character.ValueRO;

        Aspect.Default_OnMovementHit(
            in this,
            ref context,
            ref baseContext,
            ref characterBody,
            ref characterPosition,
            ref hit,
            ref remainingMovementDirection,
            ref remainingMovementLength,
            originalVelocityDirection,
            hitDistance,
            characterComponent.StepAndSlopeHandling.StepHandling,
            characterComponent.StepAndSlopeHandling.MaxStepHeight,
            characterComponent.StepAndSlopeHandling.CharacterWidthForStepGroundingCheck);
    }

    public void OverrideDynamicHitMasses(
        ref CharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref PhysicsMass characterMass,
        ref PhysicsMass otherMass,
        BasicHit hit)
    {
    }

    public void ProjectVelocityOnHits(
        ref CharacterUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref float3 velocity,
        ref bool characterIsGrounded,
        ref BasicHit characterGroundHit,
        in DynamicBuffer<KinematicVelocityProjectionHit> velocityProjectionHits,
        float3 originalVelocityDirection)
    {
        CharacterComponent characterComponent = Character.ValueRO;

        Aspect.Default_ProjectVelocityOnHits(
            ref velocity,
            ref characterIsGrounded,
            ref characterGroundHit,
            in velocityProjectionHits,
            originalVelocityDirection,
            characterComponent.StepAndSlopeHandling.ConstrainVelocityToGroundPlane);
    }
    #endregion
}
