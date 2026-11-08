using LmdCamera.Config;
using LmdCamera.Game;
using UnityEngine;

namespace LmdCamera.Core;

internal sealed class FirstPersonFollower : ICameraFollower
{
    private const float MaxNearClipBoost = 0.2f;

    private readonly CameraViewSettings _view;
    private readonly FirstPersonMotion _motion;

    private Vector3 _headOffset;

    public FirstPersonFollower(CameraViewSettings view, FirstPersonMotion motion)
    {
        _view = view;
        _motion = motion;
    }

    public void Reset(RiderRig rig)
    {
        var bike = rig.BikeTransform;
        _headOffset = HeadOffset(rig, bike.position, bike.rotation);
    }

    public void Follow(RiderRig rig)
    {
        var camera = rig.CameraTransform;
        var bike = rig.BikeTransform;
        var bikePosition = bike.position;
        var bikeRotation = bike.rotation;

        var headOffset = HeadOffset(rig, bikePosition, bikeRotation);
        _headOffset = Vector3.Lerp(_headOffset, headOffset, Damping.Factor(_motion.HeadSmoothing));
        var cameraOffset = _headOffset + new Vector3(0f, _view.Height, _view.Forward);

        var up = Vector3.Slerp(Vector3.up, bikeRotation * Vector3.up, _motion.Roll);
        var targetRotation = Quaternion.LookRotation(bikeRotation * Vector3.forward, up)
            * Quaternion.Euler(_view.Pitch, 0f, 0f);

        camera.position = bikePosition + bikeRotation * cameraOffset;
        camera.rotation = Quaternion.Slerp(camera.rotation, targetRotation, Damping.Factor(_motion.RotationSmoothing));

        // Push near clip by distance that the smoothed camera trails the head
        var lag = _headOffset - headOffset;
        var boost = Mathf.Min(new Vector2(lag.y, lag.z).magnitude, MaxNearClipBoost);
        rig.Camera.nearClipPlane = _view.Lens.NearClipPlane + boost;
    }

    private static Vector3 HeadOffset(RiderRig rig, Vector3 bikePosition, Quaternion bikeRotation) =>
        Quaternion.Inverse(bikeRotation) * (rig.Head.position - bikePosition);
}
