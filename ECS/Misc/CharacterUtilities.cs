using System;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static class CharacterUtilities
{
    public const float InputWrapAroundValue = 2000f;

    public static void SetEntityHierarchyEnabled(bool enabled, Entity parent, EntityCommandBuffer commandBuffer, BufferLookup<LinkedEntityGroup> linkedEntityGroupFromEntity)
    {
        if (enabled)
        {
            commandBuffer.RemoveComponent<Disabled>(parent);
        }
        else
        {
            commandBuffer.AddComponent<Disabled>(parent);
        }

        if (linkedEntityGroupFromEntity.HasBuffer(parent))
        {
            DynamicBuffer<LinkedEntityGroup> parentLinkedEntities = linkedEntityGroupFromEntity[parent];
            for (int i = 0; i < parentLinkedEntities.Length; i++)
            {
                if (enabled)
                {
                    commandBuffer.RemoveComponent<Disabled>(parentLinkedEntities[i].Value);
                }
                else
                {
                    commandBuffer.AddComponent<Disabled>(parentLinkedEntities[i].Value);
                }
            }
        }
    }

    public static float ExpDecayAlpha(float speed, float deltaTime)
    {
        return 1 - Mathf.Exp(-speed * deltaTime);
    }

    public static void SetEntityHierarchyEnabledParallel(bool enabled, Entity parent, EntityCommandBuffer.ParallelWriter ecb, int chunkIndex, BufferLookup<LinkedEntityGroup> linkedEntityGroupFromEntity)
    {
        if (enabled)
        {
            ecb.RemoveComponent<Disabled>(chunkIndex, parent);
        }
        else
        {
            ecb.AddComponent<Disabled>(chunkIndex, parent);
        }

        if (linkedEntityGroupFromEntity.HasBuffer(parent))
        {
            DynamicBuffer<LinkedEntityGroup> parentLinkedEntities = linkedEntityGroupFromEntity[parent];
            for (int i = 0; i < parentLinkedEntities.Length; i++)
            {
                if (enabled)
                {
                    ecb.RemoveComponent<Disabled>(chunkIndex, parentLinkedEntities[i].Value);
                }
                else
                {
                    ecb.AddComponent<Disabled>(chunkIndex, parentLinkedEntities[i].Value);
                }
            }
        }
    }
    public static void AddInputDelta(ref float2 input, float2 addedDelta)
    {
        input = math.fmod(input + addedDelta, InputWrapAroundValue);
    }
    public static float2 GetInputDelta(float2 currentValue, float2 previousValue)
    {
        float2 delta = currentValue - previousValue;

        // When delta is very large, consider that the input has wrapped around
        if (math.abs(delta.x) > (InputWrapAroundValue * 0.5f))
        {
            delta.x += (math.sign(previousValue.x - currentValue.x) * InputWrapAroundValue);
        }

        if (math.abs(delta.y) > (InputWrapAroundValue * 0.5f))
        {
            delta.y += (math.sign(previousValue.y - currentValue.y) * InputWrapAroundValue);
        }

        return delta;
    }

    public static void ComputeFinalRotationsFromRotationDelta(
    ref float viewPitchDegrees,
    ref float characterRotationYDegrees,
    float3 characterTransformUp,
    float2 yawPitchDeltaDegrees,
    float viewRollDegrees,
    float minPitchDegrees,
    float maxPitchDegrees,
    out quaternion characterRotation,
    out float canceledPitchDegrees,
    out quaternion viewLocalRotation)
    {
        // Yaw
        characterRotationYDegrees += yawPitchDeltaDegrees.x;
        ComputeRotationFromYAngleAndUp(characterRotationYDegrees, characterTransformUp, out characterRotation);

        // Pitch
        viewPitchDegrees += yawPitchDeltaDegrees.y;
        float viewPitchAngleDegreesBeforeClamp = viewPitchDegrees;
        viewPitchDegrees = math.clamp(viewPitchDegrees, minPitchDegrees, maxPitchDegrees);
        canceledPitchDegrees = yawPitchDeltaDegrees.y - (viewPitchAngleDegreesBeforeClamp - viewPitchDegrees);

        viewLocalRotation = CalculateLocalViewRotation(viewPitchDegrees, viewRollDegrees);
    }

    public static void ComputeRotationFromYAngleAndUp(
    float characterRotationYDegrees,
    float3 characterTransformUp,
    out quaternion characterRotation)
    {
        characterRotation =
            math.mul(MathUtilities.CreateRotationWithUpPriority(characterTransformUp, math.forward()),
                quaternion.Euler(0f, math.radians(characterRotationYDegrees), 0f));
    }

    public static quaternion CalculateLocalViewRotation(float viewPitchDegrees, float viewRollDegrees)
    {
        // Pitch
        quaternion viewLocalRotation = quaternion.AxisAngle(-math.right(), math.radians(viewPitchDegrees));

        // Roll
        viewLocalRotation = math.mul(viewLocalRotation,
            quaternion.AxisAngle(math.forward(), math.radians(viewRollDegrees)));

        return viewLocalRotation;
    }
}



