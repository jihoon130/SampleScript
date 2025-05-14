using log4net.Filter;
using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;


[Serializable]
public class CharacterHybridData : IComponentData
{
    public GameObject MeshObject;
}


[Serializable]
public class CharacterHybridLink : ICleanupComponentData
{
    public GameObject Object;
    public Animator Animator;
    public AudioSource AudioSource;
}

[Serializable]
public class PlayerHybridLink : ICleanupComponentData
{
    public PlayerView playerView;
    public KTransform RightHandPose;
}

[Serializable]
public class EnemyHybridLink : ICleanupComponentData
{
    public NavMeshAgent NavMeshAgent;
}