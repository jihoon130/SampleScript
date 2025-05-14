using Cysharp.Threading.Tasks;
using FPS.Attribute;
using FPS.MVP;
using System.Linq;
using Unity.Entities;
using UnityEngine;


[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class WeaponReloadSystem : SystemBase
{
    [SingletonInject] public PlayerModel playerModel;
    private bool IsReload;
    protected override void OnCreate()
    {
        ModelContainer.InjectDependencies(this);
    }

    protected override void OnUpdate()
    {
        foreach (var (inputs, audio, entity) in SystemAPI.Query<RefRW<PlayerInputs>, RefRW<PlayerAudio>>().WithEntityAccess())
        {
            var weaponEntity = SystemAPI.GetSingletonEntity<BaseWeapon>();

            if (weaponEntity != null)
            {
                if (EntityManager.HasComponent<WeaponHybridLink>(weaponEntity) && EntityManager.HasComponent<PlayerHybridLink>(entity))
                {
                    WeaponHybridLink weaponHybridLink = EntityManager.GetComponentObject<WeaponHybridLink>(weaponEntity);
                    PlayerHybridLink playerHybridLink = EntityManager.GetComponentObject<PlayerHybridLink>(entity);

                    if (audio.ValueRW.ReloadDelayTime > 0f)
                    {
                        audio.ValueRW.ReloadDelayTime -= SystemAPI.Time.DeltaTime;

                        if (audio.ValueRW.ReloadDelayTime <= 0f)
                        {
                            playerModel.Reload();
                            IsReload = false;
                        }
                    }

                    Reload(playerHybridLink, weaponHybridLink, inputs.ValueRO, ref audio.ValueRW);
                }
            }
        }
    }

    private void Reload(PlayerHybridLink playerHybridLink, WeaponHybridLink weaponHybridLink, PlayerInputs inputs,ref PlayerAudio audio)
    {
        if (inputs.ReloadHeId == true && !IsReload && playerModel.HaveProjectileStream.Value.Value > 0)
        {
            IsReload = true;
            var equipWeaponName = playerModel.EquipType.Value == EquipType.MainWeapon ? playerModel.MainWeapon.Value.ToString() : playerModel.SecondaryWeapon.Value.ToString();
            UpdateAnimation(playerHybridLink, weaponHybridLink.WeaponSettings[equipWeaponName]
                                , playerHybridLink.playerView.Animator, weaponHybridLink.WeaponViews[equipWeaponName].Animator, ref audio);
        }
    }

    private void UpdateAnimation(PlayerHybridLink playerHybridLink, FPSWeaponSettings weaponSetting,
        Animator playerAnimator, Animator weaponAnimator, ref PlayerAudio audio)
    {
        if (weaponSetting.reloadType == ReloadType.Auto)
            PlayAuto(playerHybridLink, weaponSetting, playerAnimator, weaponAnimator, ref audio);
        else
            PlayManualAsync(playerHybridLink, weaponSetting, playerAnimator, weaponAnimator).Forget();
    }

    private void PlayAuto(PlayerHybridLink playerHybridLink, FPSWeaponSettings weaponSetting, Animator playerAnimator, Animator weaponAnimator, ref PlayerAudio audio)
    {
        if (playerModel.CurrentProjectileStream.Value.Value > 0 && (playerModel.EquipType.Value == EquipType.MainWeapon && playerModel.MainWeapon.Value == WeaponType.Drake12))
        {
            PlayDrakeTac(playerHybridLink, weaponSetting, playerAnimator, weaponAnimator).Forget();
            return;
        }

        if (playerModel.CurrentProjectileStream.Value.Value == 0 && weaponSetting.ReloadEmptyAudioClip != null)
        {
            playerAnimator.Play("Reload_Empty");
            weaponAnimator.Play("Reload_Empty");
            playerHybridLink.playerView.AudioSource.PlayOneShot(weaponSetting.ReloadEmptyAudioClip);
            audio.ReloadDelayTime = weaponSetting.ReloadEmptyAudioClip.length;
        }
        else if (playerModel.CurrentProjectileStream.Value.Value > 0 && weaponSetting.ReloadTacAudioClip != null)
        {
            playerAnimator.Play("Reload_Tac");
            weaponAnimator.Play("Reload_Tac");
            playerHybridLink.playerView.AudioSource.PlayOneShot(weaponSetting.ReloadTacAudioClip);
            audio.ReloadDelayTime = weaponSetting.ReloadTacAudioClip.length;
        }
        else
            PlayManualAsync(playerHybridLink, weaponSetting, playerAnimator, weaponAnimator).Forget();
    }

    private async UniTaskVoid PlayDrakeTac(PlayerHybridLink playerHybridLink, FPSWeaponSettings weaponSetting, Animator playerAnimator, Animator weaponAnimator)
    {
        playerAnimator.Play("Reload_Tac");
        weaponAnimator.Play("Reload_Tac");

        playerHybridLink.playerView.AudioSource.PlayOneShot(weaponSetting.ReloadStartAudioClip);

        await UniTask.WaitForSeconds(weaponSetting.ReloadStartAudioClip.length);

        playerHybridLink.playerView.AudioSource.PlayOneShot(weaponSetting.ReloadLoopAudioClip);

        await UniTask.WaitForSeconds(weaponSetting.ReloadLoopAudioClip.length);

        playerHybridLink.playerView.AudioSource.PlayOneShot(weaponSetting.ReloadEndAudioClip);
        playerModel.Reload();
        IsReload = false;
    }

    private async UniTaskVoid PlayManualAsync(PlayerHybridLink playerHybridLink, FPSWeaponSettings weaponSetting, Animator playerAnimator, Animator weaponAnimator)
    {
        playerAnimator.Play("Reload_Start");
        weaponAnimator.Play("Reload_Start");
        playerHybridLink.playerView.AudioSource.PlayOneShot(weaponSetting.ReloadStartAudioClip);

        await UniTask.WaitForSeconds(weaponSetting.ReloadStartAudioClip.length);

        var loopCount = playerModel.MaxProjectile.Value - playerModel.CurrentProjectileStream.Value.Value;
        PlayManualLoopAsync(loopCount, playerHybridLink, weaponSetting, playerAnimator, weaponAnimator).Forget();
    }

    private async UniTaskVoid PlayManualLoopAsync(int loopCount, PlayerHybridLink playerHybridLink, FPSWeaponSettings weaponSetting, Animator playerAnimator, Animator weaponAnimator)
    {
        while (loopCount > 0)
        {
            loopCount--;
            playerAnimator.CrossFade("Reload_Loop", 0.1f, -1, 0f, 0f);
            weaponAnimator.Play("Reload_Loop", -1, 0f);
            playerHybridLink.playerView.AudioSource.PlayOneShot(weaponSetting.ReloadLoopAudioClip);

            await UniTask.WaitForSeconds(weaponSetting.ReloadLoopAudioClip.length);
        }

        PlayManualEnd(playerHybridLink, weaponSetting, playerAnimator, weaponAnimator);
    }

    private void PlayManualEnd(PlayerHybridLink playerHybridLink, FPSWeaponSettings weaponSetting, Animator playerAnimator, Animator weaponAnimator)
    {
        playerAnimator.Play("Reload_End");
        weaponAnimator.Play("Reload_End");
        playerHybridLink.playerView.AudioSource.PlayOneShot(weaponSetting.ReloadEndAudioClip);
        playerModel.Reload();
        IsReload = false;
    }
}
