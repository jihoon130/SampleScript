using System;
using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class WeaponHybridLink : ICleanupComponentData
{
    public Dictionary<string, WeaponView> WeaponViews;
    public Dictionary<string, FPSWeaponSettings> WeaponSettings;
}
