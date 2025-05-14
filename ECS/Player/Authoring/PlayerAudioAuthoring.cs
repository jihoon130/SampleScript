using Unity.Entities;
using UnityEngine;

public class PlayerAudioAuthoring : MonoBehaviour
{
    public class Baker : Baker<PlayerAudioAuthoring>
    {
        public override void Bake(PlayerAudioAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            PlayerAudio playerAudio = new PlayerAudio();
            playerAudio.WalkDelayTime = 0.4f;
            playerAudio.ReloadDelayTime = 0f;

            AddComponent(entity, playerAudio);
        }
    }
}
