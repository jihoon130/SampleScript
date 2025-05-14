using FPS.Attribute;
using FPS.MVP;
using UniRx;
using Unity.Entities;
using UnityEngine;

public class ProjectileAuthoring : MonoBehaviour
{
    public float Speed = 10f;
    public float Gravity = -10f;
    public float MaxLifeTime = 5f;
    public float VisualOffsetCorrectionDuration = 0.3f;

    class Baker : Baker<ProjectileAuthoring>
    {
        public override void Bake(ProjectileAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Projectile
            {
                Speed = authoring.Speed,
                Gravity = authoring.Gravity,
                MaxLifeTime = authoring.MaxLifeTime,
                VisualOffsetCorrectionDuration = authoring.VisualOffsetCorrectionDuration,
                LifetimeCounter = 0f,
            });
            AddComponent<WeaponShotIgnoredEntity>(entity);
            AddComponent(entity, new DelayedDespawn());
            SetComponentEnabled<DelayedDespawn>(entity, false);
        }
    }
}
