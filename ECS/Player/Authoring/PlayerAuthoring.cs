using UnityEngine;
using Unity.Entities;

[DisallowMultipleComponent]
public class PlayerAuthoring : MonoBehaviour
{
    public GameObject ControlledCharacter;
    public GameObject ControlledCamera;

    public class Baker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new Player
            {
                ControlledCharacter = GetEntity(authoring.ControlledCharacter, TransformUsageFlags.Dynamic),
                ControlledCamera = GetEntity(authoring.ControlledCamera, TransformUsageFlags.Dynamic),
            });
            AddComponent(entity, new PlayerInputs());
        }
    }
}