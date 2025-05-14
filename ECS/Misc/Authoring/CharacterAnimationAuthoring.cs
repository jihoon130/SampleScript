using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterAnimationAuthoring : MonoBehaviour
{
    public class Baker : Baker<CharacterAnimationAuthoring>
    {
        public override void Bake(CharacterAnimationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            CharacterAnimation characterAnimation = new CharacterAnimation();

            characterAnimation.IdleClip = 0;
            characterAnimation.WalkClip = 1;
            characterAnimation.SprintClip = 2;
            characterAnimation.AttackClip = 3;
            characterAnimation.DieClip = 4;

            AddComponent(entity, characterAnimation);
        }
    }
}