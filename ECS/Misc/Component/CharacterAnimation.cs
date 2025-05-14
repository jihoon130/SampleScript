using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.CharacterController;
using FPS.MVP;

[Serializable]
public struct CharacterAnimation : IComponentData
{
    [HideInInspector] public int ClipIndexParameterHash;

    [HideInInspector] public int IdleClip;
    [HideInInspector] public int WalkClip;
    [HideInInspector] public int SprintClip;
    [HideInInspector] public int AttackClip;
    [HideInInspector] public int DieClip;

    [HideInInspector] public float SmoothGait;
}
public static class CharacterAnimationHandler
{

    public static void UpdateAnimation(
        Animator animator,
        ref CharacterAnimation characterAnimation,
        in KinematicCharacterBody characterBody,
        in CharacterComponent characterComponent,
        in CharacterStateMachine characterStateMachine,
        in CharacterControl characterControl,
        in LocalTransform localTransform)
    {
        switch (characterStateMachine.CurrentState)
        {
            case CharacterState.GroundMove:
                {
                    if (math.length(characterControl.MoveVector) < 0.01f)
                    {
                        animator.SetInteger("CiplIndex", characterAnimation.IdleClip);
                    }
                    else
                    {
                        if (characterControl.SprintHeId)
                        {
                            animator.SetInteger(characterAnimation.ClipIndexParameterHash, characterAnimation.SprintClip);
                        }
                        else
                        {
                            animator.SetInteger("CiplIndex", characterAnimation.WalkClip);
                        }
                    }
                }
                break;
            case CharacterState.Attack:
                {
                    animator.SetInteger("CiplIndex", characterAnimation.AttackClip);
                }
                break;
            case CharacterState.Die:
                {
                    animator.SetInteger("CiplIndex", characterAnimation.DieClip);
                }
                break;
        }
    }
}