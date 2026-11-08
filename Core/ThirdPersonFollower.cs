using LmdCamera.Config;
using LmdCamera.Game;
using UnityEngine;

namespace LmdCamera.Core;

internal sealed class ThirdPersonFollower : ICameraFollower
{
    private const float MinLengthSquared = 1e-6f;

    private readonly CameraViewSettings _view;
    private readonly ThirdPersonMotion _motion;

    private Vector3 _heading = Vector3.forward;
    private float _focusHeight;

    public ThirdPersonFollower(CameraSettings settings)
    {
        _view = settings.ThirdPerson;
        _motion = settings.ThirdPersonMotion;
    }

    public void Reset(RiderRig rig)
    {
        var bike = rig.BikeTransform;
        if (TryGetHeading(bike, out var heading)) _heading = heading;
        _focusHeight = bike.position.y;
    }

    public void Follow(RiderRig rig)
    {
        var camera = rig.CameraTransform;
        var bike = rig.BikeTransform;
        var bikePosition = bike.position;

        if (!rig.IsRagdolling && TryGetHeading(bike, out var heading))
            _heading = Vector3.Slerp(_heading, heading, Damping.Factor(_motion.HeadingSmoothing));

        _focusHeight = Mathf.Lerp(_focusHeight, bikePosition.y, Damping.Factor(_motion.VerticalSmoothing));
        var focus = new Vector3(bikePosition.x, _focusHeight, bikePosition.z);

        var position = focus + Vector3.up * _view.Height + _heading * _view.Forward;
        var lookDirection = focus + Vector3.up * _motion.LookHeight - position;
        camera.position = position;

        if (lookDirection.sqrMagnitude < MinLengthSquared) return;

        camera.rotation = Quaternion.LookRotation(lookDirection, Vector3.up) * Quaternion.Euler(_view.Pitch, 0f, 0f);
    }

    private static bool TryGetHeading(Transform bike, out Vector3 heading)
    {
        heading = bike.forward;
        heading.y = 0f;

        if (heading.sqrMagnitude < MinLengthSquared) return false;

        heading.Normalize();
        return true;
    }
}
