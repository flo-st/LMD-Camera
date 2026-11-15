using Il2CppMegagon.Downhill.Cameras;
using Il2CppMegagon.Downhill.Vehicle.Controller;
using LmdCamera.Config;
using UnityEngine;

namespace LmdCamera.Game;

internal sealed class RiderRig
{
    private const string PlayCameraName = "PlayCamera(Clone)";
    private const string BikeName = "Bike(Clone)";
    private const string RagdollName = "RagDoll(Clone)";
    private const string RiderMeshPath = "bike/mergedMesh";
    private const string HeadBoneName = "head_tip_BindJNT";
    private const string CenterBoneName = "cog_BindJNT";

    private readonly Camera _camera;
    private readonly PlayCamera _playCamera;
    private readonly GameObject _cameraObject;
    private readonly BikeLocomotion _locomotion;
    private readonly Transform _centerBone;
    private readonly float _foundAt = Time.unscaledTime;

    private readonly List<(Rigidbody Body, RigidbodyInterpolation Interpolation)> _ragdollBodies = new();

    private FirstPersonRider? _firstPersonRider;
    private GameObject? _ragdoll;
    private GameObject? _smoothedRagdoll;

    private RiderRig(
        Camera camera,
        PlayCamera playCamera,
        Transform bike,
        BikeLocomotion locomotion,
        SkinnedMeshRenderer riderMesh,
        Transform head,
        Transform centerBone)
    {
        _camera = camera;
        CameraTransform = camera.transform;
        _playCamera = playCamera;
        BikeTransform = bike;
        Head = head;
        _cameraObject = camera.gameObject;
        _locomotion = locomotion;
        _centerBone = centerBone;

        riderMesh.updateWhenOffscreen = true;
    }

    public Transform CameraTransform { get; }
    public Transform BikeTransform { get; }
    public Transform Head { get; }

    public Vector3 RiderCenter => _centerBone.position;

    public float AgeSeconds => Time.unscaledTime - _foundAt;
    public bool IsRagdolling => _locomotion.prop_Boolean_2;
    public bool IsAlive =>
        _cameraObject != null && BikeTransform != null && _centerBone != null && Head != null
        && _cameraObject.activeInHierarchy;

    public static RiderRig? Find()
    {
        var cameraObject = GameObject.Find(PlayCameraName);
        var bike = GameObject.Find(BikeName);
        if (cameraObject == null || bike == null) return null;

        Camera camera = cameraObject.GetComponent<Camera>();
        PlayCamera playCamera = cameraObject.GetComponent<PlayCamera>();
        BikeLocomotion locomotion = bike.GetComponent<BikeLocomotion>();
        SkinnedMeshRenderer? riderMesh = FindRiderMesh(bike.transform);
        Transform? head = FindBone(riderMesh, HeadBoneName);
        Transform? centerBone = FindBone(riderMesh, CenterBoneName);

        return camera == null || playCamera == null || locomotion == null || riderMesh == null || head == null
            || centerBone == null
            ? null
            : new RiderRig(camera, playCamera, bike.transform, locomotion, riderMesh, head, centerBone);
    }

    public FirstPersonRider? FindRagdoll()
    {
        GameObject? ragdoll = FindRagdollObject();
        if (ragdoll == null) return null;

        Transform? head = FindBone(FindRiderMesh(ragdoll.transform), HeadBoneName);
        return head == null ? null : new FirstPersonRider(ragdoll.transform, head, _camera);
    }

    public void SetHidden(bool isRiderHidden, bool isHeadHidden)
    {
        if (isRiderHidden || isHeadHidden) _firstPersonRider ??= new FirstPersonRider(BikeTransform, Head, _camera);

        _firstPersonRider?.SetHidden(isRiderHidden, isHeadHidden);
    }

    public void SetRagdollSmoothed(bool isSmoothed)
    {
        GameObject? ragdoll = isSmoothed ? FindRagdollObject() : null;
        if (ragdoll != null && ragdoll == _smoothedRagdoll) return;

        foreach ((Rigidbody body, RigidbodyInterpolation interpolation) in _ragdollBodies)
        {
            if (body != null) body.interpolation = interpolation;
        }

        _ragdollBodies.Clear();
        _smoothedRagdoll = ragdoll;
        if (ragdoll == null) return;

        foreach (Rigidbody body in ragdoll.GetComponentsInChildren<Rigidbody>(true))
        {
            _ragdollBodies.Add((body, body.interpolation));
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    public void UpdateFirstPersonRider() => _firstPersonRider?.Update();

    public bool TryGetGameLens(out Lens gameLens)
    {
        gameLens = new Lens(_camera.fieldOfView, _camera.nearClipPlane);
        return _playCamera.enabled;
    }

    public void SetLens(Lens lens, bool isGameControlled)
    {
        if (_camera == null || _playCamera == null) return;

        _camera.fieldOfView = lens.FieldOfView;
        _camera.nearClipPlane = lens.NearClipPlane;
        _playCamera.enabled = isGameControlled;
    }

    public void Release(Lens? gameLens)
    {
        SetRagdollSmoothed(false);
        _firstPersonRider?.Destroy();
        if (gameLens is { } lens) SetLens(lens, isGameControlled: true);
    }

    private GameObject? FindRagdollObject()
    {
        if (!IsRagdolling)
        {
            _ragdoll = null;
            return null;
        }

        if (_ragdoll == null) _ragdoll = GameObject.Find(RagdollName);

        return _ragdoll == null ? null : _ragdoll;
    }

    private static SkinnedMeshRenderer? FindRiderMesh(Transform root)
    {
        Transform mesh = root.Find(RiderMeshPath);
        return mesh == null ? null : mesh.GetComponent<SkinnedMeshRenderer>();
    }

    private static Transform? FindBone(SkinnedMeshRenderer? mesh, string name) =>
        mesh == null ? null : mesh.bones.FirstOrDefault(bone => bone != null && bone.name == name);
}
