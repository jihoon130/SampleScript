using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct Enemy : IComponentData
{
    public int Code;
    public int Gold;
}
