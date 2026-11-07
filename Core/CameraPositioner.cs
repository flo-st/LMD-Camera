using LmdCamera.Config;
using LmdCamera.Game;
using UnityEngine;

namespace LmdCamera.Core;

internal sealed class CameraPositioner
{
    private const float MinLookDistanceSquared = 1e-6f;

    private readonly CameraSettings _settings;

    public CameraPositioner(CameraSettings settings) => _settings = settings;

    public void Apply(ViewMode view, RiderRig rig)
    {
        var lens = _settings.GetLens(view);
        rig.Camera.fieldOfView = lens.FieldOfView;
        rig.Camera.nearClipPlane = lens.NearClipPlane;
        rig.PlayCamera.enabled = view == ViewMode.Default;
        rig.SetRiderVisible(view != ViewMode.FirstPerson || !_settings.HideBike);
    }

    public void Follow(ViewMode view, RiderRig rig)
    {
        switch (view)
        {
            case ViewMode.FirstPerson:
                FollowFirstPerson(rig, _settings.FirstPerson);
                break;

            case ViewMode.ThirdPerson:
                FollowThirdPerson(rig, _settings.ThirdPerson);
                break;
        }
    }

    private static void FollowFirstPerson(RiderRig rig, CameraViewSettings view)
    {
        var camera = rig.CameraTransform;
        var bikeRotation = rig.BikeTransform.rotation;
        var targetRotation = bikeRotation * Quaternion.Euler(view.Pitch, 0f, 0f);

        camera.position = rig.Head.position + bikeRotation * new Vector3(0f, view.Height, view.Forward);
        camera.rotation = Quaternion.Slerp(camera.rotation, targetRotation, FollowFactor(view.Smoothing));
    }

    private static void FollowThirdPerson(RiderRig rig, CameraViewSettings view)
    {
        var camera = rig.CameraTransform;
        var bike = rig.BikeTransform;
        var bikeRotation = bike.rotation;

        var focus = bike.position + bikeRotation * new Vector3(0f, view.Height, 0f);
        var targetPosition = focus + bikeRotation * new Vector3(0f, 0f, view.Forward);
        var position = Vector3.Lerp(camera.position, targetPosition, FollowFactor(view.Smoothing));
        camera.position = position;

        var lookDirection = focus - position;
        if (lookDirection.sqrMagnitude < MinLookDistanceSquared) return;

        camera.rotation = Quaternion.LookRotation(lookDirection, Vector3.up) * Quaternion.Euler(view.Pitch, 0f, 0f);
    }

    private static float FollowFactor(float smoothing) =>
        smoothing > 0f ? 1f - Mathf.Exp(-smoothing * Time.deltaTime) : 1f;
}
