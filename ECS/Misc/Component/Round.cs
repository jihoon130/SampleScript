using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct Round : IComponentData
{
    public int CurrentRound;
    public int Count;
    public float Time;
    public float SpawnTimer;
    public bool StartHeId;
}
