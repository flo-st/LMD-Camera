using LmdCamera.Config;
using LmdCamera.Game;
using UnityEngine;

namespace LmdCamera.Core;

internal sealed class ThirdPersonFollower : CameraFollower
{
    private const float MinLengthSquared = 1e-6f;
    private const float LookHeightMeters = 1f;
    private const float SoftVerticalLagMeters = 0.5f;
    private const float UprightMinUp = 0.9f;
    private const float SpinCenterSmoothing = 1f;
    private const float RagdollSwitchSmoothing = 4f;

    private readonly CameraSettings _settings;

    private Vector3 _heading = Vector3.forward;
    private Vector3 _spinCenter;
    private float _focusHeight;
    private float _ragdollBlend;

    public ThirdPersonFollower(CameraSettings settings) : base(settings.ThirdPerson)
    {
        _settings = settings;
    }

    public override void Reset(RiderRig rig)
    {
        Transform bike = rig.BikeTransform;
        if (TryGetHeading(bike, out Vector3 heading)) _heading = heading;

        _spinCenter = bike.InverseTransformPoint(rig.RiderCenter);
        _focusHeight = bike.position.y;
        _ragdollBlend = rig.IsRagdolling ? 1f : 0f;
    }

    public override void Follow(RiderRig rig, float deltaTime)
    {
        Transform camera = rig.CameraTransform;
        Transform bike = rig.BikeTransform;
        bool isRagdolling = rig.IsRagdolling;

        if (!isRagdolling && TryGetHeading(bike, out Vector3 heading))
        {
            float headingFactor = GetSmoothingFactor(_settings.ThirdPersonHeadingSmoothing, deltaTime);
            _heading = Vector3.Slerp(_heading, heading, headingFactor);
        }

        if (!isRagdolling && bike.up.y > UprightMinUp)
        {
            Vector3 riderCenter = bike.InverseTransformPoint(rig.RiderCenter);
            _spinCenter = Vector3.Lerp(_spinCenter, riderCenter, GetSmoothingFactor(SpinCenterSmoothing, deltaTime));
        }

        Vector3 spinCenter = bike.TransformPoint(_spinCenter) - Vector3.up * _spinCenter.y;
        float ragdollFactor = GetSmoothingFactor(RagdollSwitchSmoothing, deltaTime);
        _ragdollBlend = Mathf.Lerp(_ragdollBlend, isRagdolling ? 1f : 0f, ragdollFactor);
        var target = Vector3.Lerp(spinCenter, bike.position, _ragdollBlend);

        float lag = (target.y - _focusHeight) / SoftVerticalLagMeters;
        float verticalSmoothing = _settings.ThirdPersonVerticalSmoothing * (1f + lag * lag);
        _focusHeight = Mathf.Lerp(_focusHeight, target.y, GetSmoothingFactor(verticalSmoothing, deltaTime));

        Vector3 offset = Vector3.up * View.Height + _heading * View.Forward;
        Vector3 lookDirection = Vector3.up * LookHeightMeters - offset;
        camera.position = new Vector3(target.x, _focusHeight, target.z) + offset;

        if (lookDirection.sqrMagnitude < MinLengthSquared) return;

        camera.rotation = Quaternion.LookRotation(lookDirection, Vector3.up) * Quaternion.Euler(View.Pitch, 0f, 0f);
    }

    private static bool TryGetHeading(Transform bike, out Vector3 heading)
    {
        heading = Vector3.Cross(bike.right, Vector3.up);
        if (heading.sqrMagnitude < MinLengthSquared) return false;

        heading.Normalize();
        return true;
    }
}
