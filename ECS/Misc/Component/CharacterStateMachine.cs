using System;
using Unity.Collections;
using Unity.Entities;
using Unity.CharacterController;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct CharacterStateMachine : IComponentData
{
    public CharacterState CurrentState;
    public CharacterState PreviousState;

    public GroundMoveState GroundMoveState;
    public AirMoveState AirMoveState;
    public AttackState AttackState;
    public DieState DieState;

    public void TransitionToState(CharacterState newState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        PreviousState = CurrentState;
        CurrentState = newState;

        OnStateExit(PreviousState, CurrentState, ref context, ref baseContext, in aspect);
        OnStateEnter(CurrentState, PreviousState, ref context, ref baseContext, in aspect);
    }

    public void OnStateEnter(CharacterState state, CharacterState previousState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.AirMove:
                AirMoveState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Attack:
                AttackState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Die:
                DieState.OnStateEnter(previousState, ref context, ref baseContext, in aspect);
                break;
        }
    }

    public void OnStateExit(CharacterState state, CharacterState newState, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.AirMove:
                AirMoveState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Attack:
                AttackState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Die:
                DieState.OnStateExit(newState, ref context, ref baseContext, in aspect);
                break;
        }
    }

    public void OnStatePhysicsUpdate(CharacterState state, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.AirMove:
                AirMoveState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Attack:
                AttackState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Die:
                DieState.OnStatePhysicsUpdate(ref context, ref baseContext, in aspect);
                break;
        }
    }

    public void OnStateVariableUpdate(CharacterState state, ref CharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext, in CharacterAspect aspect)
    {
        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.AirMove:
                AirMoveState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Attack:
                AttackState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
            case CharacterState.Die:
                DieState.OnStateVariableUpdate(ref context, ref baseContext, in aspect);
                break;
        }
    }

    public void GetCameraParameters(CharacterState state, in CharacterComponent character, out Entity cameraTarget, out bool calculateUpFromGravity)
    {
        cameraTarget = default;
        calculateUpFromGravity = default;

        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.AirMove:
                AirMoveState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.Attack:
                AttackState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
            case CharacterState.Die:
                DieState.GetCameraParameters(in character, out cameraTarget, out calculateUpFromGravity);
                break;
        }
    }

    public void GetMoveVectorFromPlayerInput(CharacterState state, in PlayerInputs inputs, quaternion cameraRotation, out float3 moveVector)
    {
        moveVector = default;

        switch (state)
        {
            case CharacterState.GroundMove:
                GroundMoveState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.AirMove:
                AirMoveState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.Attack:
                AttackState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
            case CharacterState.Die:
                DieState.GetMoveVectorFromPlayerInput(in inputs, cameraRotation, out moveVector);
                break;
        }
    }
}