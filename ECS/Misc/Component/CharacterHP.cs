using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct CharacterHP : IComponentData
{
    public int HP;
}
