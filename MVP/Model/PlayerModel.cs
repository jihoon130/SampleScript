
using UnityEngine;
using UniRx;
using System;
using System.Collections.Generic;

namespace FPS.MVP
{
    public class PlayerModel : ModelBase
    {
        private ReactiveProperty<int> rx_Hp = new();
        private ReactiveProperty<int> rx_MaxHp = new();
        private ReactiveProperty<int> rx_Money = new();
        private ReactiveProperty<EquipType> rx_EquipType = new();
        private ReactiveProperty<WeaponType> rx_MainWeapon = new();
        private ReactiveProperty<WeaponType> rx_SecondaryWeapon = new();

        private ReactiveProperty<int> rx_MainCurrentProjectile = new();
        private ReactiveProperty<int> rx_MainMaxProjectile = new();
        private ReactiveProperty<int> rx_MainHaveProjectile = new();

        private ReactiveProperty<int> rx_SecondaryCurrentProjectile = new();
        private ReactiveProperty<int> rx_SecondaryMaxProjectile = new();
        private ReactiveProperty<int> rx_SecondaryHaveProjectile = new();



        public IReadOnlyReactiveProperty<int> Hp => rx_Hp;
        public IReadOnlyReactiveProperty<int> MaxHp => rx_MaxHp;
        public IReadOnlyReactiveProperty<int> Money => rx_Money;
        public IReadOnlyReactiveProperty<EquipType> EquipType => rx_EquipType;
        public IReadOnlyReactiveProperty<WeaponType> MainWeapon => rx_MainWeapon;
        public IReadOnlyReactiveProperty<WeaponType> SecondaryWeapon => rx_SecondaryWeapon;
        public IReadOnlyReactiveProperty<IReadOnlyReactiveProperty<int>> CurrentProjectileStream =>
                rx_EquipType.Select(type => type == global::EquipType.MainWeapon ? (IReadOnlyReactiveProperty<int>)rx_MainCurrentProjectile : rx_SecondaryCurrentProjectile)
                            .ToReactiveProperty();

        public IReadOnlyReactiveProperty<IReadOnlyReactiveProperty<int>> HaveProjectileStream =>
                rx_EquipType.Select(type => type == global::EquipType.MainWeapon ? (IReadOnlyReactiveProperty<int>)rx_MainHaveProjectile : rx_SecondaryHaveProjectile)
                            .ToReactiveProperty();
        public IReadOnlyReactiveProperty<int> MaxProjectile => rx_EquipType.Value == global::EquipType.MainWeapon ? rx_MainMaxProjectile : rx_SecondaryMaxProjectile;


        public GameObject PrevMainWeapon;
        public GameObject PrevSecondaryWeapon;
        public GameObject[] Weapons; 
        public EquipType PrevEquipType;

        public void TakeDamage(int amount)
        {
            rx_Hp.Value -= amount;
            if (rx_Money.Value > 0)
            {
                rx_Hp.Value = 0;
            }
        }

        public void Fire()
        {
            if (rx_EquipType.Value == global::EquipType.MainWeapon)
            {
                rx_MainCurrentProjectile.Value--;
            }
            else
            {
                rx_SecondaryCurrentProjectile.Value--;
            }
        }

        public void Reload()
        {
            var reloadValue = HaveProjectileStream.Value.Value >= MaxProjectile.Value - CurrentProjectileStream.Value.Value 
                ? MaxProjectile.Value - CurrentProjectileStream.Value.Value : HaveProjectileStream.Value.Value;

            if (rx_EquipType.Value == global::EquipType.MainWeapon)
            {
                rx_MainHaveProjectile.Value -= reloadValue;
                rx_MainCurrentProjectile.Value += reloadValue;
            }
            else
            {
                rx_SecondaryHaveProjectile.Value -= reloadValue;
                rx_SecondaryCurrentProjectile.Value += reloadValue;
            }
        }

        public bool IsReload()
        {
            return CurrentProjectileStream.Value.Value < MaxProjectile.Value
                && (MaxProjectile.Value - CurrentProjectileStream.Value.Value) <= HaveProjectileStream.Value.Value;
        }

        public void SetWeapon(WeaponType type)
        {
            var weaponData = DataContainer.WeaponData[type.ToString()];

            if (weaponData.EquipType == global::EquipType.MainWeapon)
            {
                rx_MainWeapon.Value = (WeaponType)Enum.Parse(typeof(WeaponType), type.ToString());
                rx_MainCurrentProjectile.Value = weaponData.MaxProjectile;
                rx_MainMaxProjectile.Value = weaponData.MaxProjectile;
                rx_MainHaveProjectile.Value = weaponData.DefaultProjectile;
            }
            else
            {
                rx_SecondaryWeapon.Value = (WeaponType)Enum.Parse(typeof (WeaponType), type.ToString());
                rx_SecondaryCurrentProjectile.Value = weaponData.MaxProjectile;
                rx_SecondaryMaxProjectile.Value = weaponData.MaxProjectile;
                rx_SecondaryHaveProjectile.Value = weaponData.DefaultProjectile;
            }
        }

        public void ChangeEquipType(EquipType type)
        {
            if (rx_EquipType.Value == type)
                return;

            if (type == global::EquipType.MainWeapon && MainWeapon.Value == WeaponType.None)
                return;

            if (type == global::EquipType.SecondaryWeapon && SecondaryWeapon.Value == WeaponType.None)
                return;

            rx_EquipType.Value = type;
        }

        public void SetHP(int value)
        {
            rx_Hp.Value = value;
        }

        public void SetMaxHP(int value)
        {
            rx_MaxHp.Value = value;
        }
        public void AddMoney(int money)
        {
            rx_Money.Value += money;
        }

        public bool ConsumeMoney(int money)
        {
            if (money > rx_Money.Value)
                return false;

            rx_Money.Value -= money;

            return true;
        }
    }
}