using LmdCamera.Config;
using LmdCamera.Game;
using UnityEngine;

namespace LmdCamera.Core;

internal sealed class FirstPersonFollower : ICameraFollower
{
    private const float FlipStartTiltDegrees = 60f;
    private const float MinLengthSquared = 1e-6f;
    private const float FastSpinDegreesPerSecond = 100f;
    private const float SpinSpeedSmoothing = 3f;

    private readonly CameraViewSettings _view;
    private readonly FirstPersonMotion _motion;
    private readonly bool _hideRider;
    private readonly bool _immersiveMode;

    private Vector3 _headOffset;
    private Vector3 _bikeUp = Vector3.up;
    private Vector3 _lastBikeUp = Vector3.up;
    private float _spinSpeed;
    private Quaternion _steadyRotation = Quaternion.identity;
    private FirstPersonRider? _ragdoll;
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
        Transform bike = rig.BikeTransform;
        _headOffset = GetHeadOffset(rig, bike.position, bike.rotation);
        _bikeUp = bike.up;
        _lastBikeUp = bike.up;
        _spinSpeed = 0f;
        _steadyRotation = bike.rotation;
    }

    public void Follow(RiderRig rig)
    {
        if (_immersiveMode && rig.IsRagdolling && TryFollowRagdoll(rig)) return;

        if (_ragdoll is not null)
        {
            ReleaseRagdoll();
            Reset(rig);
        }

        Transform camera = rig.CameraTransform;
        Transform bike = rig.BikeTransform;
        Vector3 bikePosition = bike.position;
        Quaternion bikeRotation = bike.rotation;

        Vector3 headOffset = GetHeadOffset(rig, bikePosition, bikeRotation);
        _headOffset = Vector3.Lerp(_headOffset, headOffset, Smoothing.GetFactor(_motion.HeadSmoothing));
        Vector3 cameraOffset = _headOffset + new Vector3(0f, _view.Height, _view.Forward);

        Vector3 bikeUp = bikeRotation * Vector3.up;
        float catchUp = GetFlipCatchUp(bikeUp);
        _bikeUp = Vector3.Slerp(_bikeUp, bikeUp, Smoothing.GetFactor(_motion.TiltSmoothing * catchUp));
        var forward = Vector3.Cross(bikeRotation * Vector3.right, _bikeUp);
        if (forward.sqrMagnitude >= MinLengthSquared) _steadyRotation = Quaternion.LookRotation(forward, _bikeUp);

        float flip = Mathf.InverseLerp(Mathf.Cos(FlipStartTiltDegrees * Mathf.Deg2Rad), -1f, _bikeUp.y);
        float roll = Mathf.Lerp(_motion.Roll, 1f, flip);
        var up = Vector3.Slerp(Vector3.up, _bikeUp, roll);
        Quaternion targetRotation = Quaternion.LookRotation(_steadyRotation * Vector3.forward, up)
            * Quaternion.Euler(_view.Pitch, 0f, 0f);

        camera.position = bikePosition + bikeRotation * cameraOffset;
        camera.rotation = Quaternion.Slerp(camera.rotation, targetRotation,
            Smoothing.GetFactor(_motion.RotationSmoothing * catchUp));
    }

    public void ReleaseRagdoll()
    {
        _ragdoll?.Destroy();
        _ragdoll = null;
    }

    private float GetFlipCatchUp(Vector3 bikeUp)
    {
        if (Time.deltaTime > 0f)
        {
            float spinSpeed = Vector3.Angle(_lastBikeUp, bikeUp) / Time.deltaTime;
            _spinSpeed = Mathf.Lerp(_spinSpeed, spinSpeed, Smoothing.GetFactor(SpinSpeedSmoothing));
        }

        _lastBikeUp = bikeUp;
        float fastSpin = Mathf.Max(0f, _spinSpeed - FastSpinDegreesPerSecond) / FastSpinDegreesPerSecond;
        return 1f + _motion.FlipCatchUp * fastSpin;
    }

    private bool TryFollowRagdoll(RiderRig rig)
    {
        Transform camera = rig.CameraTransform;

        if (_ragdoll is null)
        {
            _ragdoll = rig.FindRagdoll();
            if (_ragdoll is null) return false;

            _cameraRotationInHead = Quaternion.Inverse(rig.Head.rotation) * camera.rotation;
            _ragdoll.SetHidden(_hideRider, true);
            _ragdoll.BeforeCameraRender = () => PlaceOnHead(rig);
        }

        if (_ragdoll.Head == null) return false;

        _ragdoll.Update();

        Transform head = _ragdoll.Head;
        camera.rotation = Quaternion.Slerp(camera.rotation, head.rotation * _cameraRotationInHead,
            Smoothing.GetFactor(_motion.RotationSmoothing));
        PlaceOnHead(rig);
        return true;
    }

    private void PlaceOnHead(RiderRig rig)
    {
        if (_ragdoll is null || _ragdoll.Head == null) return;

        Transform camera = rig.CameraTransform;
        camera.position = _ragdoll.Head.position + camera.rotation * new Vector3(0f, _view.Height, _view.Forward);
    }

    private static Vector3 GetHeadOffset(RiderRig rig, Vector3 bikePosition, Quaternion bikeRotation) =>
        Quaternion.Inverse(bikeRotation) * (rig.Head.position - bikePosition);
}
