using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[Serializable]
public struct IKTransforms
{
    public IKTransforms(Transform t, Transform m, Transform r)
    {
        Tip = t;
        Mid = m;
        Root = r;
    }

    public Transform Tip;
    public Transform Mid;
    public Transform Root;
}

[Serializable]
public struct KTransform
{
    public static KTransform Identity = new(Vector3.zero, Quaternion.identity, Vector3.one);

    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public KTransform(Vector3 newPos, Quaternion newRot, Vector3 newScale)
    {
        position = newPos;
        rotation = newRot;
        scale = newScale;
    }

    public KTransform(Vector3 newPos, Quaternion newRot)
    {
        position = newPos;
        rotation = newRot;
        scale = Vector3.one;
    }

    public KTransform(Transform t, bool worldSpace = true)
    {
        if (worldSpace)
        {
            position = t.position;
            rotation = t.rotation;
        }
        else
        {
            position = t.localPosition;
            rotation = t.localRotation;
        }

        scale = t.localScale;
    }

    // Linearly interpolates translation and scale. Spherically interpolates rotation.
    public static KTransform Lerp(KTransform a, KTransform b, float alpha)
    {
        Vector3 outPos = Vector3.Lerp(a.position, b.position, alpha);
        Quaternion outRot = Quaternion.Slerp(a.rotation, b.rotation, alpha);

        Vector3 outScale = Vector3.Lerp(a.scale, a.scale, alpha);

        return new KTransform(outPos, outRot, outScale);
    }

    public static KTransform EaseLerp(KTransform a, KTransform b, float alpha, EaseMode easeMode)
    {
        return Lerp(a, b, KCurves.Ease(0f, 1f, alpha, easeMode));
    }

    // Frame-rate independent interpolation.
    public static KTransform ExpDecay(KTransform a, KTransform b, float speed, float deltaTime)
    {
        return Lerp(a, b, KMath.ExpDecayAlpha(speed, deltaTime));
    }

    public bool Equals(KTransform other, bool useScale)
    {
        bool result = position.Equals(other.position) && rotation.Equals(other.rotation);

        if (useScale)
        {
            result = result && scale.Equals(other.scale);
        }

        return result;
    }

    // Returns a point relative to this transform.
    public Vector3 InverseTransformPoint(Vector3 worldPosition, bool useScale)
    {
        Vector3 result = Quaternion.Inverse(rotation) * (worldPosition - position);

        if (useScale)
        {
            result = Vector3.Scale(scale, result);
        }

        return result;
    }

    // Returns a vector relative to this transform.
    public Vector3 InverseTransformVector(Vector3 worldDirection, bool useScale)
    {
        Vector3 result = Quaternion.Inverse(rotation) * worldDirection;

        if (useScale)
        {
            result = Vector3.Scale(scale, result);
        }

        return result;
    }

    // Converts a local position from this transform to world.
    public Vector3 TransformPoint(Vector3 localPosition, bool useScale)
    {
        if (useScale)
        {
            localPosition = Vector3.Scale(scale, localPosition);
        }

        return position + rotation * localPosition;
    }

    // Converts a local vector from this transform to world.
    public Vector3 TransformVector(Vector3 localDirection, bool useScale)
    {
        if (useScale)
        {
            localDirection = Vector3.Scale(scale, localDirection);
        }

        return rotation * localDirection;
    }

    // Returns a transform relative to this transform.
    public KTransform GetRelativeTransform(KTransform worldTransform, bool useScale)
    {
        return new KTransform()
        {
            position = InverseTransformPoint(worldTransform.position, useScale),
            rotation = Quaternion.Inverse(rotation) * worldTransform.rotation,
            scale = Vector3.Scale(scale, worldTransform.scale)
        };
    }

    // Converts a local transform to world.
    public KTransform GetWorldTransform(KTransform localTransform, bool useScale)
    {
        return new KTransform()
        {
            position = TransformPoint(localTransform.position, useScale),
            rotation = rotation * localTransform.rotation,
            scale = Vector3.Scale(scale, localTransform.scale)
        };
    }
}

[Serializable]
public struct VectorCurve
{
    public AnimationCurve x;
    public AnimationCurve y;
    public AnimationCurve z;

    public static VectorCurve Linear(float timeStart, float timeEnd, float valueStart, float valueEnd)
    {
        VectorCurve result = new VectorCurve()
        {
            x = AnimationCurve.Linear(timeStart, timeEnd, valueStart, valueEnd),
            y = AnimationCurve.Linear(timeStart, timeEnd, valueStart, valueEnd),
            z = AnimationCurve.Linear(timeStart, timeEnd, valueStart, valueEnd)
        };

        return result;
    }

    public static VectorCurve Constant(float timeStart, float timeEnd, float value)
    {
        VectorCurve result = new VectorCurve()
        {
            x = AnimationCurve.Constant(timeStart, timeEnd, value),
            y = AnimationCurve.Constant(timeStart, timeEnd, value),
            z = AnimationCurve.Constant(timeStart, timeEnd, value)
        };

        return result;
    }

    public float GetCurveLength()
    {
        float maxTime = -1f;

        float curveTime = KCurves.GetCurveLength(x);
        maxTime = curveTime > maxTime ? curveTime : maxTime;

        curveTime = KCurves.GetCurveLength(y);
        maxTime = curveTime > maxTime ? curveTime : maxTime;

        curveTime = KCurves.GetCurveLength(z);
        maxTime = curveTime > maxTime ? curveTime : maxTime;

        return maxTime;
    }

    public Vector3 GetValue(float time)
    {
        return new Vector3(x.Evaluate(time), y.Evaluate(time), z.Evaluate(time));
    }

    public bool IsValid()
    {
        return x != null && y != null && z != null;
    }

    public VectorCurve(Keyframe[] keyFrame)
    {
        x = new AnimationCurve(keyFrame);
        y = new AnimationCurve(keyFrame);
        z = new AnimationCurve(keyFrame);
    }
}

public static class KCurves
{
    public static float GetCurveLength(AnimationCurve curve)
    {
        float length = 0f;

        if (curve != null)
        {
            length = curve[curve.length - 1].time;
        }

        return length;
    }

    public static float EaseSine(float a, float b, float alpha)
    {
        return Mathf.Lerp(a, b, -(Mathf.Cos(Mathf.PI * alpha) - 1) / 2);
    }

    public static float EaseCubic(float a, float b, float alpha)
    {
        alpha = alpha < 0.5 ? 4 * alpha * alpha * alpha : 1 - Mathf.Pow(-2 * alpha + 2, 3) / 2;
        return Mathf.Lerp(a, b, alpha);
    }

    public static float EaseCurve(float a, float b, float alpha, AnimationCurve curve)
    {
        alpha = curve?.Evaluate(alpha) ?? alpha;
        return Mathf.Lerp(a, b, alpha);
    }

    public static float Ease(float a, float b, float alpha, EaseMode ease)
    {
        alpha = Mathf.Clamp01(alpha);

        if (ease.easeFunc == EEaseFunc.Sine)
        {
            return EaseSine(a, b, alpha);
        }

        if (ease.easeFunc == EEaseFunc.Cubic)
        {
            return EaseCubic(a, b, alpha);
        }

        if (ease.easeFunc == EEaseFunc.Custom)
        {
            return EaseCurve(a, b, alpha, ease.curve);
        }

        return Mathf.Lerp(a, b, alpha);
    }
}

public static class KMath
{
    public const float FloatMin = 1e-10f;
    public const float SqrEpsilon = 1e-8f;

    public static float Square(float value)
    {
        return value * value;
    }

    public static float SqrDistance(Vector3 a, Vector3 b)
    {
        return (b - a).sqrMagnitude;
    }

    public static float NormalizeEulerAngle(float angle)
    {
        while (angle < -180f) angle += 360f;
        while (angle >= 180f) angle -= 360f;
        return angle;
    }

    public static float TriangleAngle(float aLen, float aLen1, float aLen2)
    {
        float c = Mathf.Clamp((aLen1 * aLen1 + aLen2 * aLen2 - aLen * aLen) / (aLen1 * aLen2) / 2.0f, -1.0f, 1.0f);
        return Mathf.Acos(c);
    }

    public static Quaternion FromToRotation(Vector3 from, Vector3 to)
    {
        float theta = Vector3.Dot(from.normalized, to.normalized);
        if (theta >= 1f) return Quaternion.identity;

        if (theta <= -1f)
        {
            Vector3 axis = Vector3.Cross(from, Vector3.right);
            if (axis.sqrMagnitude == 0f) axis = Vector3.Cross(from, Vector3.up);

            return Quaternion.AngleAxis(180f, axis);
        }

        return Quaternion.AngleAxis(Mathf.Acos(theta) * Mathf.Rad2Deg, Vector3.Cross(from, to).normalized);
    }

    public static Quaternion NormalizeSafe(Quaternion q)
    {
        float dot = Quaternion.Dot(q, q);
        if (dot > FloatMin)
        {
            float rsqrt = 1.0f / Mathf.Sqrt(dot);
            return new Quaternion(q.x * rsqrt, q.y * rsqrt, q.z * rsqrt, q.w * rsqrt);
        }

        return Quaternion.identity;
    }

    public static float InvLerp(float value, float a, float b)
    {
        float alpha = 0f;

        if (!Mathf.Approximately(a, b))
        {
            alpha = (value - a) / (b - a);
        }

        return Mathf.Clamp01(alpha);
    }

    public static float ExpDecayAlpha(float speed, float deltaTime)
    {
        return 1 - Mathf.Exp(-speed * deltaTime);
    }

    public static Vector2 ComputeLookAtInput(Transform root, Transform from, Transform to)
    {
        Vector2 result = Vector2.zero;

        Quaternion rot = Quaternion.LookRotation(to.position - from.position);
        rot = Quaternion.Inverse(root.rotation) * rot;

        Vector3 euler = rot.eulerAngles;
        result.x = NormalizeEulerAngle(euler.x);
        result.y = NormalizeEulerAngle(euler.y);

        return result;
    }
}

public static class KAnimationMath
{
    public static Quaternion RotateInSpace(Quaternion space, Quaternion target, Quaternion rotation, float alpha)
    {
        return Quaternion.Slerp(target, space * rotation * (Quaternion.Inverse(space) * target), alpha);
    }

    public static Quaternion RotateInSpace(KTransform space, KTransform target, Quaternion offset, float alpha)
    {
        return RotateInSpace(space.rotation, target.rotation, offset, alpha);
    }

    public static void RotateInSpace(Transform space, Transform target, Quaternion offset, float alpha)
    {
        target.rotation = RotateInSpace(space.rotation, target.rotation, offset, alpha);
    }

    public static Vector3 MoveInSpace(KTransform space, KTransform target, Vector3 offset, float alpha)
    {
        return target.position + (space.TransformPoint(offset, false) - space.position) * alpha;
    }

    public static void MoveInSpace(Transform space, Transform target, Vector3 offset, float alpha)
    {
        target.position += (space.TransformPoint(offset) - space.position) * alpha;
    }

    public static bool IsWeightFull(float weight)
    {
        return Mathf.Approximately(weight, 1f);
    }

    public static bool IsWeightRelevant(float weight)
    {
        return !Mathf.Approximately(weight, 0f);
    }

    public static void ModifyTransform(Transform component, Transform target, in KPose pose, float alpha = 1f)
    {
        if (pose.modifyMode == EModifyMode.Add)
        {
            AddTransform(component, target, in pose, alpha);
            return;
        }

        ReplaceTransform(component, target, in pose, alpha);
    }

    private static void AddTransform(Transform component, Transform target, in KPose pose, float alpha = 1f)
    {
        if (pose.space == ESpaceType.BoneSpace)
        {
            MoveInSpace(target, target, pose.pose.position, alpha);
            RotateInSpace(target, target, pose.pose.rotation, alpha);
            return;
        }

        if (pose.space == ESpaceType.ParentBoneSpace)
        {
            Transform parent = target.parent;

            MoveInSpace(parent, target, pose.pose.position, alpha);
            RotateInSpace(parent, target, pose.pose.rotation, alpha);
            return;
        }

        if (pose.space == ESpaceType.ComponentSpace)
        {
            MoveInSpace(component, target, pose.pose.position, alpha);
            RotateInSpace(component, target, pose.pose.rotation, alpha);
            return;
        }

        Vector3 position = target.position;
        Quaternion rotation = target.rotation;

        target.position = Vector3.Lerp(position, position + pose.pose.position, alpha);
        target.rotation = Quaternion.Slerp(rotation, rotation * pose.pose.rotation, alpha);
    }

    private static void ReplaceTransform(Transform component, Transform target, in KPose pose, float alpha = 1f)
    {
        if (pose.space == ESpaceType.BoneSpace || pose.space == ESpaceType.ParentBoneSpace)
        {
            target.localPosition = Vector3.Lerp(target.localPosition, pose.pose.position, alpha);
            target.localRotation = Quaternion.Slerp(target.localRotation, pose.pose.rotation, alpha);
            return;
        }

        if (pose.space == ESpaceType.ComponentSpace)
        {
            target.position = Vector3.Lerp(target.position, component.TransformPoint(pose.pose.position), alpha);
            target.rotation = Quaternion.Slerp(target.rotation, component.rotation * pose.pose.rotation, alpha);
            return;
        }

        target.position = Vector3.Lerp(target.position, pose.pose.position, alpha);
        target.rotation = Quaternion.Slerp(target.rotation, pose.pose.rotation, alpha);
    }
}


[Serializable]
public struct KRigElement
{
    public string name;
    [HideInInspector] public int index;
    public bool isVirtual;

    public KRigElement(int index = -1, string name = "None", bool isVirtual = false)
    {
        this.index = index;
        this.name = name;
        this.isVirtual = isVirtual;
    }
}

[Serializable]
public struct KPose
{
    public KRigElement element;
    public KTransform pose;
    public ESpaceType space;
    public EModifyMode modifyMode;
}

[Serializable]
public enum EEaseFunc
{
    Linear,
    Sine,
    Cubic,
    Custom
}

[Serializable]
public struct EaseMode
{
    public EEaseFunc easeFunc;
    public AnimationCurve curve;

    public EaseMode(EEaseFunc func)
    {
        easeFunc = func;
        curve = AnimationCurve.Linear(0f, 0f, 1f, 0f);
    }
}

public struct KTwoBoneIkData
{
    public KTransform root;
    public KTransform mid;
    public KTransform tip;
    public KTransform target;
    public KTransform hint;

    public float posWeight;
    public float rotWeight;
    public float hintWeight;

    public bool hasValidHint;
}

public class KTwoBoneIK
{
    public static void Solve(ref KTwoBoneIkData ikData)
    {
        Vector3 aPosition = ikData.root.position;
        Vector3 bPosition = ikData.mid.position;
        Vector3 cPosition = ikData.tip.position;

        Vector3 tPosition = Vector3.Lerp(cPosition, ikData.target.position, ikData.posWeight);
        Quaternion tRotation = Quaternion.Lerp(ikData.tip.rotation, ikData.target.rotation, ikData.rotWeight);
        bool hasHint = ikData.hasValidHint && ikData.hintWeight > 0f;

        Vector3 ab = bPosition - aPosition;
        Vector3 bc = cPosition - bPosition;
        Vector3 ac = cPosition - aPosition;
        Vector3 at = tPosition - aPosition;

        float abLen = ab.magnitude;
        float bcLen = bc.magnitude;
        float acLen = ac.magnitude;
        float atLen = at.magnitude;

        float oldAbcAngle = KMath.TriangleAngle(acLen, abLen, bcLen);
        float newAbcAngle = KMath.TriangleAngle(atLen, abLen, bcLen);

        // Bend normal strategy is to take whatever has been provided in the animation
        // stream to minimize configuration changes, however if this is collinear
        // try computing a bend normal given the desired target position.
        // If this also fails, try resolving axis using hint if provided.
        Vector3 axis = Vector3.Cross(ab, bc);
        if (axis.sqrMagnitude < KMath.SqrEpsilon)
        {
            axis = hasHint ? Vector3.Cross(ikData.hint.position - aPosition, bc) : Vector3.zero;

            if (axis.sqrMagnitude < KMath.SqrEpsilon)
                axis = Vector3.Cross(at, bc);

            if (axis.sqrMagnitude < KMath.SqrEpsilon)
                axis = Vector3.up;
        }

        axis = Vector3.Normalize(axis);

        float a = 0.5f * (oldAbcAngle - newAbcAngle);
        float sin = Mathf.Sin(a);
        float cos = Mathf.Cos(a);
        Quaternion deltaR = new Quaternion(axis.x * sin, axis.y * sin, axis.z * sin, cos);

        KTransform localTip = ikData.mid.GetRelativeTransform(ikData.tip, false);
        ikData.mid.rotation = deltaR * ikData.mid.rotation;

        // Update child transform.
        ikData.tip = ikData.mid.GetWorldTransform(localTip, false);

        cPosition = ikData.tip.position;
        ac = cPosition - aPosition;

        KTransform localMid = ikData.root.GetRelativeTransform(ikData.mid, false);
        localTip = ikData.mid.GetRelativeTransform(ikData.tip, false);
        ikData.root.rotation = KMath.FromToRotation(ac, at) * ikData.root.rotation;

        // Update child transforms.
        ikData.mid = ikData.root.GetWorldTransform(localMid, false);
        ikData.tip = ikData.mid.GetWorldTransform(localTip, false);

        if (hasHint)
        {
            float acSqrMag = ac.sqrMagnitude;
            if (acSqrMag > 0f)
            {
                bPosition = ikData.mid.position;
                cPosition = ikData.tip.position;
                ab = bPosition - aPosition;
                ac = cPosition - aPosition;

                Vector3 acNorm = ac / Mathf.Sqrt(acSqrMag);
                Vector3 ah = ikData.hint.position - aPosition;
                Vector3 abProj = ab - acNorm * Vector3.Dot(ab, acNorm);
                Vector3 ahProj = ah - acNorm * Vector3.Dot(ah, acNorm);

                float maxReach = abLen + bcLen;
                if (abProj.sqrMagnitude > (maxReach * maxReach * 0.001f) && ahProj.sqrMagnitude > 0f)
                {
                    Quaternion hintR = KMath.FromToRotation(abProj, ahProj);
                    hintR.x *= ikData.hintWeight;
                    hintR.y *= ikData.hintWeight;
                    hintR.z *= ikData.hintWeight;
                    hintR = KMath.NormalizeSafe(hintR);
                    ikData.root.rotation = hintR * ikData.root.rotation;

                    ikData.mid = ikData.root.GetWorldTransform(localMid, false);
                    ikData.tip = ikData.mid.GetWorldTransform(localTip, false);
                }
            }
        }

        ikData.tip.rotation = tRotation;
    }
}

public struct KTwoBoneIKJob : IJobParallelFor
{
    public NativeArray<KTwoBoneIkData> twoBoneIkJobData;

    public void Execute(int index)
    {
        var twoBoneIkData = twoBoneIkJobData[index];
        KTwoBoneIK.Solve(ref twoBoneIkData);
        twoBoneIkJobData[index] = twoBoneIkData;
    }
}


public class IKUtilites
{
    public static KTransform GetWeaponPose(Transform rightHandTip, KTransform rightHandPose, Animator animator, Transform weaponBone)
    {
        KTransform defaultWorldPose =
            new KTransform(rightHandTip).GetWorldTransform(rightHandPose, false);

        float weight = animator.GetFloat(Animator.StringToHash("RightHandWeight"));

        return KTransform.Lerp(new KTransform(weaponBone), defaultWorldPose, weight);
    }

    public static void ApplyIkData(in KTwoBoneIkData ikData, in IKTransforms transforms)
    {
        transforms.Root.rotation = ikData.root.rotation;
        transforms.Mid.rotation = ikData.mid.rotation;
        transforms.Tip.rotation = ikData.tip.rotation;
    }

    public static void ProcessOffsets(ref KTransform weaponT, Transform root, FPSWeaponSettings weaponSettings, Animator animator, IKTransforms rightHand, IKTransforms leftHand)
    {
        KTransform rootT = new KTransform(root);
        var weaponOffset = weaponSettings.ikOffset;

        float mask = 1f - animator.GetFloat(Animator.StringToHash("TacSprintWeight"));
        weaponT.position = KAnimationMath.MoveInSpace(rootT, weaponT, weaponOffset, mask);

        KAnimationMath.MoveInSpace(root, rightHand.Root, weaponSettings.rightClavicleOffset, mask);
        KAnimationMath.MoveInSpace(root, leftHand.Root, weaponSettings.leftClavicleOffset, mask);
    }

    public static void ProcessAdditives(ref KTransform weaponT, Transform skeletonRoot, Transform weaponBoneAdditive, Animator animator, float adsWeight)
    {
        KTransform rootT = new KTransform(skeletonRoot);
        KTransform additive = rootT.GetRelativeTransform(new KTransform(weaponBoneAdditive), false);

        float weight = Mathf.Lerp(1f, 0.3f, adsWeight) * (1f - animator.GetFloat("GrenadeWeight"));

        weaponT.position = KAnimationMath.MoveInSpace(rootT, weaponT, additive.position, weight);
        weaponT.rotation = KAnimationMath.RotateInSpace(rootT, weaponT, additive.rotation, weight);
    }

    //private void ProcessIkMotion(ref KTransform weaponT, Transform rootTransform, IKMotion activeMotion)
    //{
    //    if (activeMotion == null) return;

    //    _ikMotionPlayBack = Mathf.Clamp(_ikMotionPlayBack + activeMotion.playRate * Time.deltaTime, 0f,
    //        activeMotion.GetLength());

    //    Vector3 positionTarget = activeMotion.translationCurves.GetValue(_ikMotionPlayBack);
    //    positionTarget.x *= activeMotion.translationScale.x;
    //    positionTarget.y *= activeMotion.translationScale.y;
    //    positionTarget.z *= activeMotion.translationScale.z;

    //    Vector3 rotationTarget = activeMotion.rotationCurves.GetValue(_ikMotionPlayBack);
    //    rotationTarget.x *= activeMotion.rotationScale.x;
    //    rotationTarget.y *= activeMotion.rotationScale.y;
    //    rotationTarget.z *= activeMotion.rotationScale.z;

    //    _ikMotion.position = positionTarget;
    //    _ikMotion.rotation = Quaternion.Euler(rotationTarget);

    //    if (!Mathf.Approximately(activeMotion.blendTime, 0f))
    //    {
    //        _ikMotion = KTransform.Lerp(_cachedIkMotion, _ikMotion,
    //            _ikMotionPlayBack / activeMotion.blendTime);
    //    }

    //    var root = new KTransform(rootTransform);
    //    weaponT.position = KAnimationMath.MoveInSpace(root, weaponT, _ikMotion.position, 1f);
    //    weaponT.rotation = KAnimationMath.RotateInSpace(root, weaponT, _ikMotion.rotation, 1f);
    //}

    //private void ProcessRecoil(ref KTransform weaponT, Transform rootTransform)
    //{
    //    KTransform recoil = new KTransform()
    //    {
    //        rotation = _recoilAnimation.OutRot,
    //        position = _recoilAnimation.OutLoc,
    //    };

    //    KTransform root = new KTransform(rootTransform);
    //    weaponT.position = KAnimationMath.MoveInSpace(root, weaponT, recoil.position, 1f);
    //    weaponT.rotation = KAnimationMath.RotateInSpace(root, weaponT, recoil.rotation, 1f);
    //}

    public static void SetupIkData(ref KTwoBoneIkData ikData, in KTransform target, in IKTransforms transforms,
    float weight = 1f)
    {
        ikData.target = target;

        ikData.tip = new KTransform(transforms.Tip);
        ikData.mid = ikData.hint = new KTransform(transforms.Mid);
        ikData.root = new KTransform(transforms.Root);

        ikData.hintWeight = weight;
        ikData.posWeight = weight;
        ikData.rotWeight = weight;
    }
}