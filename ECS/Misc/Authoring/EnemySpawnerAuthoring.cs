using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class EnemySpawnerAuthoring : MonoBehaviour 
{
    public GameObject[] SpawnPoint;
    public GameObject Enemy;

    class Baker : Baker<EnemySpawnerAuthoring>
    {
        public override void Bake(EnemySpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new EnemySpanwer { Enemy = GetEntity(authoring.Enemy, TransformUsageFlags.Dynamic) });
        }
    }
}
