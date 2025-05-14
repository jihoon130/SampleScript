using FPS.Attribute;
using FPS.MVP;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class PlayerHybridIKSystem : SystemBase
{
    private static Quaternion ANIMATED_OFFSET = Quaternion.Euler(90f, 0f, 0f);
    [SingletonInject] private PlayerModel playerModel;
    private FPSWeaponSettings weaponData;
    private float adsWeight;

    protected override void OnCreate()
    {
        ModelContainer.InjectDependencies(this);
    }

    protected override void OnUpdate()
    {
        if (!SystemAPI.HasSingleton<Player>())
            return;

        Entity entity = SystemAPI.GetSingletonEntity<Player>();
        Player player = SystemAPI.GetComponent<Player>(entity);
        adsWeight = Mathf.Clamp01(adsWeight + 2.5f * SystemAPI.Time.DeltaTime * -1f);
        if (entity == null || !EntityManager.HasComponent<PlayerHybridLink>(entity))
            return;

        PlayerHybridLink playerHybridLink = EntityManager.GetComponentObject<PlayerHybridLink>(entity);
        CharacterHybridLink characterHybridLink = EntityManager.GetComponentObject<CharacterHybridLink>(player.ControlledCharacter);
        CharacterAnimation characterAnimation = SystemAPI.GetComponent<CharacterAnimation>(player.ControlledCharacter);
        CharacterControl characterControl = SystemAPI.GetComponent<CharacterControl>(player.ControlledCharacter);

        characterAnimation.SmoothGait = Mathf.Lerp(characterAnimation.SmoothGait,
            math.length(characterControl.MoveVector),
            CharacterUtilities.ExpDecayAlpha(8f, SystemAPI.Time.DeltaTime));

        characterHybridLink.Animator.SetFloat("Gait", characterAnimation.SmoothGait);

        weaponData = AddressableManager.Instance.Get<FPSWeaponSettings>("WeaponData", playerModel.EquipType.Value ==
            EquipType.MainWeapon ? playerModel.MainWeapon.Value.ToString() : playerModel.SecondaryWeapon.Value.ToString());
        var playerData = AddressableManager.Instance.Get<FPSPlayerSettings>("PlayerData", "PlayerData");

        KAnimationMath.RotateInSpace(playerHybridLink.playerView.Root, playerHybridLink.playerView.RightHand.Tip,
                weaponData.rightHandSprintOffset, characterHybridLink.Animator.GetFloat(Animator.StringToHash("TacSprintWeight")));

        KTransform weaponTransform = IKUtilites.GetWeaponPose(playerHybridLink.playerView.RightHand.Tip, playerHybridLink.RightHandPose, characterHybridLink.Animator, playerHybridLink.playerView.WeaponPart);


        weaponTransform.rotation = KAnimationMath.RotateInSpace(weaponTransform, weaponTransform,
                    ANIMATED_OFFSET, 1f);

        KTransform rightHandTarget = weaponTransform.GetRelativeTransform(new KTransform(playerHybridLink.playerView.RightHand.Tip), false);
        KTransform leftHandTarget = weaponTransform.GetRelativeTransform(new KTransform(playerHybridLink.playerView.LeftHand.Tip), false);

        IKUtilites.ProcessOffsets(ref weaponTransform, playerHybridLink.playerView.Root, weaponData, characterHybridLink.Animator, playerHybridLink.playerView.RightHand, playerHybridLink.playerView.LeftHand);
        IKUtilites.ProcessAdditives(ref weaponTransform, playerHybridLink.playerView.Root, playerHybridLink.playerView.WeaponBoneAdditive, characterHybridLink.Animator, adsWeight);

        playerHybridLink.playerView.WeaponPart.position = weaponTransform.position;
        playerHybridLink.playerView.WeaponPart.rotation = weaponTransform.rotation;

        rightHandTarget = weaponTransform.GetWorldTransform(rightHandTarget, false);
        leftHandTarget = weaponTransform.GetWorldTransform(leftHandTarget, false);

        KTwoBoneIkData _rightHandIk = new();
        KTwoBoneIkData _leftHandIk = new();

        IKUtilites.SetupIkData(ref _rightHandIk, rightHandTarget, playerHybridLink.playerView.RightHand, 1.0f);
        IKUtilites.SetupIkData(ref _leftHandIk, leftHandTarget, playerHybridLink.playerView.LeftHand, 1.0f);

        KTwoBoneIK.Solve(ref _rightHandIk);
        KTwoBoneIK.Solve(ref _leftHandIk);

        IKUtilites.ApplyIkData(_rightHandIk, playerHybridLink.playerView.RightHand);
        IKUtilites.ApplyIkData(_leftHandIk, playerHybridLink.playerView.LeftHand);

        SystemAPI.SetComponent(player.ControlledCharacter, characterAnimation);
    }
}