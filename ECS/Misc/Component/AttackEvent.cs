using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct AttackTag : IComponentData
{
    [HideInInspector] public int Damage; 
}