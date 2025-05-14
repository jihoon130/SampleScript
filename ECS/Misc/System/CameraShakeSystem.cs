using FPS.Attribute;
using FPS.MVP;
using UniRx;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class CameraShakeSystem : SystemBase
{
    [SingletonInject] public PlayerModel playerModel;

    private FPSCameraShake activeShake; 
    private Vector3 cameraShake;
    private Vector3 cameraShakeTarget;
    private float cameraShakePlayback;

    protected override void OnCreate()
    {
        ModelContainer.InjectDependencies(this);

        MessageBroker.Default.Receive<FireEvent>().Subscribe(_ =>
        {
            activeShake = AddressableManager.Instance.Get<FPSWeaponSettings>("WeaponData", playerModel.EquipType.Value == EquipType.MainWeapon
                ? playerModel.MainWeapon.Value.ToString() : playerModel.SecondaryWeapon.Value.ToString()).cameraShake;
            cameraShakePlayback = 0f;

            cameraShakeTarget.x = FPSCameraShake.GetTarget(activeShake.pitch);
            cameraShakeTarget.y = FPSCameraShake.GetTarget(activeShake.yaw);
            cameraShakeTarget.z = FPSCameraShake.GetTarget(activeShake.roll);
        });
    }

    protected override void OnUpdate()
    {
        var ecb = SystemAPI.GetSingletonRW<EndSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(World.Unmanaged);

        foreach (var (shake, entity) in SystemAPI.Query<RefRO<CameraShake>>().WithEntityAccess())
        {
            var transform = SystemAPI.GetComponent<LocalTransform>(entity);
            var rootTransform = SystemAPI.GetComponent<LocalTransform>(shake.ValueRO.RootEntity);
            var cameraBoneLocal = SystemAPI.GetComponent<LocalTransform>(shake.ValueRO.CameraBone);

            transform.Rotation = math.mul(rootTransform.Rotation, cameraBoneLocal.Rotation);

            if (activeShake == null)
                continue;

            float deltaTime = SystemAPI.Time.DeltaTime;
            float length = activeShake.shakeCurve.GetCurveLength();

            cameraShakePlayback += deltaTime * activeShake.playRate;
            cameraShakePlayback = math.clamp(cameraShakePlayback, 0f, length);

            float alpha = KMath.ExpDecayAlpha(activeShake.smoothSpeed, deltaTime);
            if (!KAnimationMath.IsWeightRelevant(activeShake.smoothSpeed))
                alpha = 1f;

            float3 target = activeShake.shakeCurve.GetValue(cameraShakePlayback);
            target *= cameraShakeTarget;

            cameraShake = math.lerp(cameraShake, target, alpha);

            transform.Rotation = math.mul(transform.Rotation, quaternion.Euler(cameraShake));

            ecb.SetComponent(entity, transform);
        }
    }

}
