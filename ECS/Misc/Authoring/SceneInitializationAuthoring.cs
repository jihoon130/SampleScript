using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class SceneInitializationAuthoring : MonoBehaviour
{
    public GameObject CharacterSpawnPointEntity;
    public GameObject CharacterPrefabEntity;
    public GameObject PlayerPrefabEntity;
    public GameObject CameraPrefabEntity;
    public GameObject Enemy;
    public class Baker : Baker<SceneInitializationAuthoring>
    {
        public override void Bake(SceneInitializationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new SceneInitialization
            {
                CharacterSpawnPointEntity = GetEntity(authoring.CharacterSpawnPointEntity, TransformUsageFlags.Dynamic),
                CharacterPrefabEntity = GetEntity(authoring.CharacterPrefabEntity, TransformUsageFlags.Dynamic),
                PlayerPrefabEntity = GetEntity(authoring.PlayerPrefabEntity, TransformUsageFlags.Dynamic),
                CameraPrefabEntity = GetEntity(authoring.CameraPrefabEntity, TransformUsageFlags.Dynamic),
                EnemyPrefabEntity = GetEntity(authoring.Enemy, TransformUsageFlags.Dynamic),
            });
        }
    }
}