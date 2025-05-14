using Cysharp.Threading.Tasks;
using FPS.Attribute;
using FPS.MVP;
using System.Linq;
using UniRx;
using UnityEngine;

public class PlayerPresenter : MonoBehaviour
{
    [SingletonInject] private PlayerModel model;
    [SerializeField] private PlayerView view;

    private void Start()
    {
        ModelContainer.InjectDependencies(this);
        Bind();
    }
    private void Bind()
    {
        model.EquipType
            .Subscribe(_ =>
            {
                UniTask.Void(async () =>
                {

                    if (_ == EquipType.None)
                        return;

                    if (model.PrevEquipType == _)
                        return;

                    view.Animator.Play("UnEquip");

                    await UniTask.Delay((int)(GetAnimationClipLength(view.Animator, "UnEquip") * 1000));

                    foreach (var weapon in model.Weapons)
                    {
                        weapon.SetActive(false);
                    }


                    string targetWeaponName = _ == EquipType.MainWeapon ? model.MainWeapon.ToString() : model.SecondaryWeapon.ToString();

                    var target = model.Weapons.FirstOrDefault(w => w != null && w.name == targetWeaponName);
                    if (target != null)
                        target.SetActive(true);

                    model.PrevEquipType = _;

                    var data = AddressableManager.Instance.Get<FPSWeaponSettings>("WeaponData", targetWeaponName);
                    view.Animator.runtimeAnimatorController = data.characterController;
                    view.Animator.Play("Equip");
                });
            })
            .AddTo(gameObject);


        MessageBroker.Default
        .Receive<FireEvent>()
        .Subscribe(evt =>
        {
            model.Fire();
            var equipWeaponName = model.EquipType.Value == EquipType.MainWeapon ? model.MainWeapon.Value.ToString() : model.SecondaryWeapon.Value.ToString();
            var data = AddressableManager.Instance.Get<FPSWeaponSettings>("WeaponData", equipWeaponName);
            int random = Random.Range(0, data.fireSounds.Length);
            view.AudioSource.PlayOneShot(data.fireSounds[random]);
        })
        .AddTo(gameObject);
    }



    private float GetAnimationClipLength(Animator animator, string clipName)
    {
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
                return clip.length;
        }
        return 0f;
    }
}
