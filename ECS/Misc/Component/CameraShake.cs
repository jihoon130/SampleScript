using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct CameraShake : IComponentData
{
    public Entity RootEntity;
    public Entity CameraBone;
}
