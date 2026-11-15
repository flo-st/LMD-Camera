using LmdCamera.Config;
using LmdCamera.Game;
using UnityEngine;

namespace LmdCamera.Core;

internal sealed class ThirdPersonFollower : ICameraFollower
{
    private const float MinLengthSquared = 1e-6f;
    private const float LookHeightMeters = 1f;
    private const float SoftVerticalLagMeters = 0.5f;
    private const float UprightMinUp = 0.9f;
    private const float SpinCenterSmoothing = 1f;
    private const float RagdollSwitchSmoothing = 4f;

    private readonly CameraViewSettings _view;
    private readonly ThirdPersonMotion _motion;

    private Vector3 _heading = Vector3.forward;
    private float _focusHeight;
    private Vector3 _spinCenter;
    private float _ragdollBlend;

    public ThirdPersonFollower(CameraSettings settings)
    {
        _view = settings.ThirdPerson;
        _motion = settings.ThirdPersonMotion;
    }

    public void Reset(RiderRig rig)
    {
        Transform bike = rig.BikeTransform;
        if (TryGetHeading(bike, out Vector3 heading)) _heading = heading;
        _focusHeight = bike.position.y;
        _spinCenter = bike.InverseTransformPoint(rig.RiderCenter);
        _ragdollBlend = rig.IsRagdolling ? 1f : 0f;
    }

    public void Follow(RiderRig rig)
    {
        Transform camera = rig.CameraTransform;
        Transform bike = rig.BikeTransform;
        bool isRagdolling = rig.IsRagdolling;

        if (!isRagdolling && TryGetHeading(bike, out Vector3 heading))
        {
            _heading = Vector3.Slerp(_heading, heading, Smoothing.GetFactor(_motion.HeadingSmoothing));
        }

        float ragdoll = isRagdolling ? 1f : 0f;
        _ragdollBlend = Mathf.Lerp(_ragdollBlend, ragdoll, Smoothing.GetFactor(RagdollSwitchSmoothing));
        var target = Vector3.Lerp(GetSpinCenter(rig, bike, isRagdolling), bike.position, _ragdollBlend);

        UpdateFocusHeight(target.y);
        var focus = new Vector3(target.x, _focusHeight, target.z);

        Vector3 position = focus + Vector3.up * _view.Height + _heading * _view.Forward;
        Vector3 lookDirection = focus + Vector3.up * LookHeightMeters - position;
        camera.position = position;

        if (lookDirection.sqrMagnitude < MinLengthSquared) return;

        camera.rotation = Quaternion.LookRotation(lookDirection, Vector3.up) * Quaternion.Euler(_view.Pitch, 0f, 0f);
    }

    private void UpdateFocusHeight(float targetHeight)
    {
        float lag = (targetHeight - _focusHeight) / SoftVerticalLagMeters;
        float verticalSmoothing = _motion.VerticalSmoothing * (1f + lag * lag);
        _focusHeight = Mathf.Lerp(_focusHeight, targetHeight, Smoothing.GetFactor(verticalSmoothing));
    }

    private Vector3 GetSpinCenter(RiderRig rig, Transform bike, bool isRagdolling)
    {
        if (!isRagdolling && bike.up.y > UprightMinUp)
        {
            Vector3 riderCenter = bike.InverseTransformPoint(rig.RiderCenter);
            _spinCenter = Vector3.Lerp(_spinCenter, riderCenter, Smoothing.GetFactor(SpinCenterSmoothing));
        }

        return bike.TransformPoint(_spinCenter) - Vector3.up * _spinCenter.y;
    }

    private static bool TryGetHeading(Transform bike, out Vector3 heading)
    {
        heading = Vector3.Cross(bike.right, Vector3.up);

        if (heading.sqrMagnitude < MinLengthSquared) return false;

        heading.Normalize();
        return true;
    }
}
