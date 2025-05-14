using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EnemyAuthroing : MonoBehaviour
{
    public class Baker : Baker<EnemyAuthroing>
    {
        public override void Bake(EnemyAuthroing authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new Enemy {});
            AddComponent(entity, new NavAgentComponent {});
        }
    }
}