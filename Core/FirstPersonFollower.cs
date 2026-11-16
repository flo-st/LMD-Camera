using LmdCamera.Config;
using LmdCamera.Game;
using UnityEngine;

namespace LmdCamera.Core;

internal sealed class FirstPersonFollower : CameraFollower
{
    private const float FlipStartUp = 0.5f;
    private const float MinLengthSquared = 1e-6f;
    private const float FastSpinDegreesPerSecond = 100f;
    private const float SpinSpeedSmoothing = 3f;

    private readonly CameraSettings _settings;

    private Vector3 _headOffset;
    private Vector3 _bikeUp = Vector3.up;
    private Vector3 _lastBikeUp = Vector3.up;
    private Vector3 _forward = Vector3.forward;
    private float _spinDegreesPerSecond;
    private FirstPersonRider? _ragdoll;
    private Quaternion _cameraRotationInHead;

    public FirstPersonFollower(CameraSettings settings) : base(settings.FirstPerson)
    {
        _settings = settings;
    }

    private Vector3 ViewOffset => new(0f, View.Height, View.Forward);

    public override void Reset(RiderRig rig)
    {
        Transform bike = rig.BikeTransform;
        _headOffset = bike.InverseTransformPoint(rig.Head.position);
        _bikeUp = bike.up;
        _lastBikeUp = _bikeUp;
        _forward = bike.forward;
        _spinDegreesPerSecond = 0f;
        _ragdoll = null;
    }

    public override void Follow(RiderRig rig, float deltaTime)
    {
        Transform camera = rig.CameraTransform;

        if (_settings.IsImmersiveMode && rig.FindRagdollRider() is { } ragdoll)
        {
            FollowRagdoll(camera, ragdoll, rig.Head, deltaTime);
            return;
        }

        if (_ragdoll is not null) Reset(rig);

        Transform bike = rig.BikeTransform;
        Vector3 headOffset = bike.InverseTransformPoint(rig.Head.position);
        float headFactor = GetSmoothingFactor(_settings.FirstPersonHeadSmoothing, deltaTime);
        _headOffset = Vector3.Lerp(_headOffset, headOffset, headFactor);

        Vector3 bikeUp = bike.up;
        float catchUp = GetFlipCatchUp(bikeUp, deltaTime);
        float tiltFactor = GetSmoothingFactor(_settings.FirstPersonTiltSmoothing * catchUp, deltaTime);
        _bikeUp = Vector3.Slerp(_bikeUp, bikeUp, tiltFactor);

        var forward = Vector3.Cross(bike.right, _bikeUp);
        if (forward.sqrMagnitude >= MinLengthSquared) _forward = forward;

        float flip = Mathf.InverseLerp(FlipStartUp, -1f, _bikeUp.y);
        var up = Vector3.Slerp(Vector3.up, _bikeUp, Mathf.Lerp(_settings.FirstPersonRoll, 1f, flip));
        Quaternion targetRotation = Quaternion.LookRotation(_forward, up) * Quaternion.Euler(View.Pitch, 0f, 0f);
        float rotationFactor = GetSmoothingFactor(_settings.FirstPersonRotationSmoothing * catchUp, deltaTime);

        camera.position = bike.TransformPoint(_headOffset + ViewOffset);
        camera.rotation = Quaternion.Slerp(camera.rotation, targetRotation, rotationFactor);
    }

    private float GetFlipCatchUp(Vector3 bikeUp, float deltaTime)
    {
        if (deltaTime > 0f)
        {
            float degreesPerSecond = Vector3.Angle(_lastBikeUp, bikeUp) / deltaTime;
            float spinFactor = GetSmoothingFactor(SpinSpeedSmoothing, deltaTime);
            _spinDegreesPerSecond = Mathf.Lerp(_spinDegreesPerSecond, degreesPerSecond, spinFactor);
        }

        _lastBikeUp = bikeUp;
        float fastSpin = MathF.Max(0f, _spinDegreesPerSecond / FastSpinDegreesPerSecond - 1f);
        return 1f + _settings.FirstPersonFlipCatchUp * fastSpin;
    }

    private void FollowRagdoll(Transform camera, FirstPersonRider ragdoll, Transform riderHead, float deltaTime)
    {
        Transform head = ragdoll.Head;

        if (ragdoll != _ragdoll)
        {
            _ragdoll = ragdoll;
            _cameraRotationInHead = Quaternion.Inverse(riderHead.rotation) * camera.rotation;
            ragdoll.BeforeCameraRender = () => PlaceOnHead(camera, head);
        }

        Quaternion targetRotation = head.rotation * _cameraRotationInHead;
        float rotationFactor = GetSmoothingFactor(_settings.FirstPersonRotationSmoothing, deltaTime);

        camera.rotation = Quaternion.Slerp(camera.rotation, targetRotation, rotationFactor);
        PlaceOnHead(camera, head);
    }

    private void PlaceOnHead(Transform camera, Transform head)
    {
        if (head == null) return;

        camera.position = head.position + camera.rotation * ViewOffset;
    }
}
