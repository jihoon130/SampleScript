using Unity.CharacterController;
using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PhysicsShapeAuthoring))]
public class CharacterAuthoring : MonoBehaviour
{
    public AuthoringKinematicCharacterProperties CharacterProperties = AuthoringKinematicCharacterProperties.GetDefault();
    public CharacterComponent Character = default;

    public GameObject MeshPrefab;
    public GameObject MeshRoot;
    public GameObject RollballMesh;
    public GameObject DefaultCameraTarget;
    public GameObject CameraBone;

    public class Baker : Baker<CharacterAuthoring>
    {
        public override void Bake(CharacterAuthoring authoring)
        {
            KinematicCharacterUtilities.BakeCharacter(this, authoring, authoring.CharacterProperties);

            authoring.Character.DefaultCameraTargetEntity = GetEntity(authoring.DefaultCameraTarget, TransformUsageFlags.Dynamic);
            authoring.Character.MeshRootEntity = GetEntity(authoring.MeshRoot, TransformUsageFlags.Dynamic);
            authoring.Character.RollBallMeshEntity = GetEntity(authoring.RollballMesh, TransformUsageFlags.Dynamic);
            authoring.Character.CameraBoneEntity = GetEntity(authoring.CameraBone, TransformUsageFlags.Dynamic);

            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, authoring.Character);
            AddComponent(entity, new CharacterControl());
            AddComponent(entity, new CharacterStateMachine());
            AddComponentObject(entity, new CharacterHybridData { MeshObject = authoring.MeshPrefab });
        }
    }
}
