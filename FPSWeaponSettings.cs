// Designed by KINEMATION, 2025.

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponSettings", menuName = "KINEMATION/FPS Animation Pack/Weapon Settings")]
public class FPSWeaponSettings : ScriptableObject
{
    [Header("General")]
    public RuntimeAnimatorController characterController;
    public RecoilAnimData recoilAnimData;
    public ReloadType reloadType;
    public FPSCameraShake cameraShake;

    [Header("IK")]
    public Vector3 ikOffset;
    public Vector3 leftClavicleOffset;
    public Vector3 rightClavicleOffset;
    public Vector3 aimPointOffset;
    public Quaternion rightHandSprintOffset = Quaternion.identity;
    [Range(0f, 1f)] public float adsBlend = 0f;

    [Header("SFX")]
    public AudioClip[] fireSounds;
    public Vector2 firePitchRange = Vector2.one;
    public Vector2 fireVolumeRange = Vector2.one;

    [Header("Auto")]
    public AudioClip ReloadTacAudioClip;
    public AudioClip ReloadEmptyAudioClip;

    [Header("Manual")]
    public AudioClip ReloadStartAudioClip;
    public AudioClip ReloadLoopAudioClip;
    public AudioClip ReloadEndAudioClip;
}