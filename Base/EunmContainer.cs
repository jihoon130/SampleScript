using System;
using UnityEngine;

public enum WeaponType
{
    None,
    M1911,
    Kar98k,
    Drake12,
    MX16A4,
}

public enum EquipType
{
    None,
    MainWeapon,
    SecondaryWeapon,
}

public enum ESpaceType
{
    BoneSpace,
    ParentBoneSpace,
    ComponentSpace,
    WorldSpace
}

public enum EModifyMode
{
    Add,
    Replace
}

public enum ShopCategory
{
    Weapon,
    Upgrades
}

public enum ReloadType
{
    Auto,
    Manual
}

public enum InfomationPopupIconType
{
    Shop
}