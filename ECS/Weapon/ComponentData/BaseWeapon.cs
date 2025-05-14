using Unity.Entities;
using UnityEngine;

public struct BaseWeapon : IComponentData
{
    public Entity ShotOrigin;
    public bool Automatic;
    public float FiringRate;
    public float SpreadRadians;
    public int ProjectilesPerShot;

    [HideInInspector] public bool IsFiring;
    [HideInInspector] public float ShotTimer;
    [HideInInspector] public uint TotalShotsCount;
    [HideInInspector] public uint TotalProjectilesCount;
}

public struct PrefabWeapon : IComponentData
{
    public Entity ProjectilePrefab;
    public int Damage;
}