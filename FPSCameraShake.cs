using UnityEngine;

[CreateAssetMenu(fileName = "NewCameraShake", menuName = "KINEMATION/FPS Animation Pack/Camera Shake")]
public class FPSCameraShake : ScriptableObject
{
    [Unfold] public VectorCurve shakeCurve = VectorCurve.Constant(0f, 1f, 0f);
    public Vector2 pitch;
    public Vector2 yaw;
    public Vector2 roll;
    [Min(0f)] public float smoothSpeed;
    [Min(0f)] public float playRate = 1f;

    public static float GetTarget(Vector2 value)
    {
        return Random.Range(value.x, value.y);
    }
}