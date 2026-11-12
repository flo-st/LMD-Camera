using LmdCamera.Config;
using LmdCamera.Game;
using UnityEngine;

namespace LmdCamera.Core;

internal sealed class FirstPersonFollower : ICameraFollower
{
    private const float MaxNearClipBoost = 0.2f;
    private const float HiddenRagdollNearClip = 0.1f;
    private const float FlipStartTilt = 60f;

    private readonly CameraViewSettings _view;
    private readonly FirstPersonMotion _motion;
    private readonly bool _hideRider;
    private readonly bool _immersiveMode;

    private Vector3 _headOffset;
    private Ragdoll? _ragdoll;
    private Quaternion _cameraRotationInHead;

    public FirstPersonFollower(CameraSettings settings)
    {
        _view = settings.FirstPerson;
        _motion = settings.FirstPersonMotion;
        _hideRider = settings.HideRider;
        _immersiveMode = settings.ImmersiveMode;
    }

    public void Reset(RiderRig rig)
    {
        var bike = rig.BikeTransform;
        _headOffset = HeadOffset(rig, bike.position, bike.rotation);
    }

    public void Follow(RiderRig rig)
    {
        if (_immersiveMode && rig.IsRagdolling && TryFollowRagdoll(rig)) return;

        if (_ragdoll is not null)
        {
            _ragdoll.SetVisible(true);
            _ragdoll = null;
            Reset(rig);
        }

        var camera = rig.CameraTransform;
        var bike = rig.BikeTransform;
        var bikePosition = bike.position;
        var bikeRotation = bike.rotation;

        var headOffset = HeadOffset(rig, bikePosition, bikeRotation);
        _headOffset = Vector3.Lerp(_headOffset, headOffset, Damping.Factor(_motion.HeadSmoothing));
        var cameraOffset = _headOffset + new Vector3(0f, _view.Height, _view.Forward);

        var bikeUp = bikeRotation * Vector3.up;
        var flip = Mathf.InverseLerp(Mathf.Cos(FlipStartTilt * Mathf.Deg2Rad), -1f, bikeUp.y);
        var roll = Mathf.Lerp(_motion.Roll, 1f, flip);
        var up = Vector3.Slerp(Vector3.up, bikeUp, roll);
        var targetRotation = Quaternion.LookRotation(bikeRotation * Vector3.forward, up)
            * Quaternion.Euler(_view.Pitch, 0f, 0f);

        camera.position = bikePosition + bikeRotation * cameraOffset;
        camera.rotation = Quaternion.Slerp(camera.rotation, targetRotation, Damping.Factor(_motion.RotationSmoothing));

        // Push near clip by distance that smoothed camera trails the head
        var lag = _headOffset - headOffset;
        var boost = Mathf.Min(new Vector2(lag.y, lag.z).magnitude, MaxNearClipBoost);
        rig.Camera.nearClipPlane = _view.Lens.NearClipPlane + boost;
    }

    private bool TryFollowRagdoll(RiderRig rig)
    {
        var camera = rig.CameraTransform;

        if (_ragdoll is null)
        {
            _ragdoll = RiderRig.FindRagdoll();
            if (_ragdoll is null) return false;

            _cameraRotationInHead = Quaternion.Inverse(rig.Head.rotation) * camera.rotation;
        }

        if (!_ragdoll.IsAlive) return false;

        if (_hideRider) _ragdoll.SetVisible(false);

        var head = _ragdoll.Head;
        camera.rotation = Quaternion.Slerp(camera.rotation, head.rotation * _cameraRotationInHead,
            Damping.Factor(_motion.RotationSmoothing));
        camera.position = head.position + camera.rotation * new Vector3(0f, _view.Height, _view.Forward);
        rig.Camera.nearClipPlane = _hideRider ? HiddenRagdollNearClip : _view.Lens.NearClipPlane;
        return true;
    }

    private static Vector3 HeadOffset(RiderRig rig, Vector3 bikePosition, Quaternion bikeRotation) =>
        Quaternion.Inverse(bikeRotation) * (rig.Head.position - bikePosition);
}
