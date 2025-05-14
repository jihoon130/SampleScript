using Unity.Entities;
using UnityEngine;

public class EnemySpawnPointAuthoring : MonoBehaviour
{

    class Baker : Baker<EnemySpawnPointAuthoring>
    {
        public override void Bake(EnemySpawnPointAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemySpawnPoint { });
        }
    }


}
