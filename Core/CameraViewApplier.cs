using LMD_Camera.Config;
using LMD_Camera.Game;
using UnityEngine;

namespace LMD_Camera.Core;

internal class CameraViewApplier
{
    private const float FirstPersonStep = 0.05f;
    private const float ThirdPersonStep = 0.1f;
    private const float PitchStep = 2f;
    private const float MinLookDistanceSquared = 1e-6f;

    private readonly CameraSettings _cameraSettings;
    private readonly (KeyCode Key, Adjustment Change)[] _firstPersonAdjustments;
    private readonly (KeyCode Key, Adjustment Change)[] _thirdPersonAdjustments;

    public CameraViewApplier(CameraSettings cameraSettings)
    {
        _cameraSettings = cameraSettings;
        _firstPersonAdjustments = cameraSettings.AdjustmentKeys.Bindings(FirstPersonStep, PitchStep);
        _thirdPersonAdjustments = cameraSettings.AdjustmentKeys.Bindings(ThirdPersonStep, PitchStep);
    }

    public void ApplyViewSettings(ViewMode mode, RiderRig rig)
    {
        var lens = _cameraSettings.GetLensForView(mode);
        rig.Camera.fieldOfView = lens.FieldOfView;
        rig.Camera.nearClipPlane = lens.NearClipPlane;
        rig.PlayCamera.enabled = mode is ViewMode.Default;
        rig.SetRiderVisible(!(mode is ViewMode.FirstPerson && _cameraSettings.HideBike));
    }

    public void UpdateCamera(ViewMode view, RiderRig rig)
    {
        switch (view)
        {
            case ViewMode.FirstPerson:
                ApplyKeyAdjustments(_cameraSettings.FirstPerson, _firstPersonAdjustments);
                PositionFirstPersonCamera(rig, _cameraSettings.FirstPerson);
                break;

            case ViewMode.ThirdPerson:
                ApplyKeyAdjustments(_cameraSettings.ThirdPerson, _thirdPersonAdjustments);
                PositionThirdPersonCamera(rig, _cameraSettings.ThirdPerson);
                break;
        }
    }

    private static void PositionFirstPersonCamera(RiderRig rig, CameraViewSettings cameraViewSettings)
    {
        var bike = rig.BikeTransform;
        var camera = rig.Camera.transform;

        camera.position = rig.Head.position + bike.up * cameraViewSettings.Height + bike.forward * cameraViewSettings.Forward;
        camera.rotation = Quaternion.Slerp(
            camera.rotation,
            bike.rotation * Quaternion.Euler(cameraViewSettings.Pitch, 0f, 0f),
            CalcFollowFactor(cameraViewSettings.Smoothing));
    }

    private static void PositionThirdPersonCamera(RiderRig rig, CameraViewSettings cameraViewSettings)
    {
        var bike = rig.BikeTransform;
        var camera = rig.Camera.transform;

        var focus = bike.position + bike.up * cameraViewSettings.Height;
        camera.position = Vector3.Lerp(
            camera.position,
            focus + bike.forward * cameraViewSettings.Forward,
            CalcFollowFactor(cameraViewSettings.Smoothing));

        var lookDirection = focus - camera.position;
        if (lookDirection.sqrMagnitude < MinLookDistanceSquared) return;

        camera.rotation = Quaternion.LookRotation(lookDirection, Vector3.up) * Quaternion.Euler(cameraViewSettings.Pitch, 0f, 0f);
    }

    private static float CalcFollowFactor(float smoothing) =>
        smoothing > 0f ? 1f - Mathf.Exp(-smoothing * Time.deltaTime) : 1f;

    private void ApplyKeyAdjustments(CameraViewSettings cameraViewSettings, (KeyCode Key, Adjustment Change)[] bindings)
    {
        if (!_cameraSettings.AllowAdjustments) return;

        var changed = false;

        foreach (var (Key, Change) in bindings)
        {
            if (!Input.GetKeyDown(Key)) continue;

            cameraViewSettings.Adjust(Change);
            changed = true;
        }

        if (!changed) return;

        _cameraSettings.SavePreferences();
    }
}
