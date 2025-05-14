using Cysharp.Threading.Tasks;
using FPS.Attribute;
using FPS.MVP;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UniRx;

[AlwaysSynchronizeSystem]
[UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
[UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
public partial class CharacterHybridSystem : SystemBase
{
    protected override void OnCreate()
    {
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(World.Unmanaged);

        foreach (var (characterAnimation, hybridData, entity) in SystemAPI.Query<RefRW<CharacterAnimation>, CharacterHybridData>()
                     .WithNone<CharacterHybridLink>()
                     .WithEntityAccess())
        {
            GameObject instance = CharacterFactory.Create(hybridData.MeshObject);
            if (instance == null) continue;

            Animator animator = instance.GetComponentInChildren<Animator>();
            AudioSource audioSource = instance.GetComponentInChildren<AudioSource>();
            CharacterPart part = instance.GetComponent<CharacterPart>();

            var hybridLink = new CharacterHybridLink
            {
                Object = instance,
                Animator = animator,
                AudioSource = audioSource,
            };

            ecb.AddComponent(entity, hybridLink);

            for (int i = 0; i < animator.parameters.Length; i++)
            {
                if (animator.parameters[i].name == "ClipIndex")
                {
                    characterAnimation.ValueRW.ClipIndexParameterHash = animator.parameters[i].nameHash;
                    break;
                }
            }
        }


        // Update
        foreach (var (characterAnimation, characterBody, characterTransform, characterComponent, characterStateMachine, characterControl, hybridLink, entity) in SystemAPI.Query<
            RefRW<CharacterAnimation>,
            KinematicCharacterBody,
            LocalTransform,
            CharacterComponent,
            CharacterStateMachine,
            CharacterControl,
            CharacterHybridLink>()
            .WithEntityAccess())
        {
            if (hybridLink.Object)
            {
                // Transform
                LocalToWorld meshRootLTW = SystemAPI.GetComponent<LocalToWorld>(characterComponent.MeshRootEntity);
                hybridLink.Object.transform.position = meshRootLTW.Position;
                hybridLink.Object.transform.rotation = meshRootLTW.Rotation;

                if (characterControl.DieHeId)
                    hybridLink.Object.SetActive(false);

                // Animation
                if (hybridLink.Animator)
                {
                    CharacterAnimationHandler.UpdateAnimation(
                        hybridLink.Animator,
                        ref characterAnimation.ValueRW,
                        in characterBody,
                        in characterComponent,
                        in characterStateMachine,
                        in characterControl,
                        in characterTransform);
                }
            }
        }

        // Destroy
        foreach (var (hybridLink, entity) in SystemAPI.Query<CharacterHybridLink>()
                     .WithNone<CharacterHybridData>()
                     .WithEntityAccess())
        {
            GameObject.Destroy(hybridLink.Object);
            ecb.RemoveComponent<CharacterHybridLink>(entity);
        }
    }
}