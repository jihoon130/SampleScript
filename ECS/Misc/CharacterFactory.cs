using FPS.MVP;
using UnityEngine;
using UniRx;
using System.Linq;

public static class CharacterFactory
{
    public static ObjectPool pool = new();

    public static GameObject Create(GameObject prefab)
    {
        GameObject instance =  pool.Get(prefab.name);

        return instance;
    }

    public static void SetupWeapons(PlayerModel model, CharacterPart part)
    {
        model.Weapons = AddressableManager.Instance.InitGameObjects("Weapon", part.WeaponPart, false);
        model.SetWeapon(WeaponType.MX16A4);
    }

    public static void SetupEquipTypeObserver(PlayerModel model, CharacterPart part, GameObject owner)
    {
        model.EquipType
            .Subscribe(_ =>
            {
                if (_ == EquipType.None)
                {
                    _ = model.PrevEquipType;
                    return;
                }

                if (model.PrevEquipType == _)
                    return;

                foreach (var weapon in model.Weapons)
                {
                    weapon.SetActive(false);
                }

                string targetWeaponName = _ == EquipType.MainWeapon ? model.MainWeapon.ToString() : model.SecondaryWeapon.ToString();

                var target = model.Weapons.FirstOrDefault(w => w != null && w.name.Replace("(Clone)", "") == targetWeaponName);
                if (target != null)
                    target.SetActive(true);

                model.PrevEquipType = _;
            })
            .AddTo(owner);
    }
}
