using System;
using System.Collections.Generic;

[Serializable]
public class WeaponData : IGameData
{
	public string Name;
	public EquipType EquipType;
	public bool Automatic;
	public float FiringRate;
	public float SpreadRadians;
	public int ProjectilesPerShot;
	public int MaxProjectile;
	public int DefaultProjectile;
	public int Damage;
}
