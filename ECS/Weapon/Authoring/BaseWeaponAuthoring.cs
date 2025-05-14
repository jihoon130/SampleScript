using Unity.Entities;
using UnityEngine;

public class BaseWeaponAuthoring : MonoBehaviour
{
    public GameObject ShotOrigin;
    public WeaponType WeaponType;

    class Baker : Baker<BaseWeaponAuthoring>
    {
        public override void Bake(BaseWeaponAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new BaseWeapon
            {
                ShotOrigin = GetEntity(authoring.ShotOrigin, TransformUsageFlags.Dynamic)
            });

            AddComponent(entity, new WeaponShotSimulationOriginOverride());
            AddBuffer<WeaponShotIgnoredEntity>(entity);
            AddBuffer<WeaponProjectileEvent>(entity);
        }
    }
}
