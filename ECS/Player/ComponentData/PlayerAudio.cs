using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct PlayerAudio : IComponentData
{
    public float WalkDelayTime;
    public float SprintDelayTime;
    public float ReloadDelayTime;
}
