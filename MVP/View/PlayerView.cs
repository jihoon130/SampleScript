using UnityEngine;

public class PlayerView : ViewBase
{
    public IKTransforms RightHand;
    public IKTransforms LeftHand;

    public Transform WeaponPart;
    public Transform Root;
    public Transform CameraTransform;
    public Transform SkeletonRoot;
    public Transform WeaponBoneAdditive;

    public Animator Animator;
    public AudioSource AudioSource;
}
