using Cysharp.Threading.Tasks;
using System;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterAudioHandler
{
    public static void PlayAudio(
    AudioSource audioSource,
    FPSPlayerSettings playerSettings,
    ref PlayerAudio playerAudio,
    in CharacterStateMachine characterStateMachine,
    in CharacterControl characterControl)
    {
        playerAudio.SprintDelayTime -= Time.deltaTime;
        playerAudio.WalkDelayTime -= Time.deltaTime;
        switch (characterStateMachine.CurrentState)
        {
            case CharacterState.GroundMove:
                {
                    if (math.length(characterControl.MoveVector) < 0.01f)
                    {
                    }
                    else
                    {
                        if (characterControl.SprintHeId)
                        {
                            if (playerAudio.SprintDelayTime <= 0f)
                            {
                                PlayRandomOneShot(audioSource, playerSettings.Sprints);
                                playerAudio.SprintDelayTime = 0.4f;

                            }
                        }
                        else
                        {
                            if (playerAudio.WalkDelayTime <= 0f)
                            {
                                PlayRandomOneShot(audioSource, playerSettings.MoveMents);
                                playerAudio.WalkDelayTime = 0.4f;
                            }
                        }
                    }
                }
                break;
        }
    }

public static async UniTask PlayOneShotAsync(AudioSource audioSource, AudioClip clip, Action onComplete = null)
{
    audioSource.PlayOneShot(clip);
    await UniTask.Delay(TimeSpan.FromSeconds(clip.length));
    onComplete?.Invoke();
}

public static void PlayRandomOneShot(AudioSource audioSource, AudioClip[] clips)
{
    int random = Random.Range(0, clips.Length);

    audioSource.PlayOneShot(clips[random]);
}
}
