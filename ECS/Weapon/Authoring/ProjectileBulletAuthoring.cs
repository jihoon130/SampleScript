using FPS.Attribute;
using FPS.MVP;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(ProjectileAuthoring))]
public class ProjectileBulletAuthoring : MonoBehaviour
{

    class Baker : Baker<ProjectileBulletAuthoring>
    {
        public override void Bake(ProjectileBulletAuthoring authoring)
        {
        }
    }
}
