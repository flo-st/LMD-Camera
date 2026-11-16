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
    private FirstPersonRider? _ragdollRider;
    private GameObject? _ragdoll;
    private float _gameFieldOfView;
    private float _gameNearClipPlane;
    private bool _isRiderHidden;
    private bool _isRagdollSmoothed;

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
        _playCamera = playCamera;
        _cameraObject = camera.gameObject;
        _locomotion = locomotion;
        _centerBone = centerBone;
        _gameFieldOfView = camera.fieldOfView;
        _gameNearClipPlane = camera.nearClipPlane;

        CameraTransform = camera.transform;
        BikeTransform = bike;
        Head = head;

        riderMesh.updateWhenOffscreen = true;
    }

    public Transform CameraTransform { get; }
    public Transform BikeTransform { get; }
    public Transform Head { get; }
    public bool IsRagdolling { get; private set; }

    public Vector3 RiderCenter => _centerBone.position;
    public float AgeSeconds => Time.unscaledTime - _foundAt;

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
        if (camera == null || playCamera == null || locomotion == null) return null;

        SkinnedMeshRenderer? riderMesh = FindRiderMesh(bike.transform);
        Transform? head = FindBone(riderMesh, HeadBoneName);
        Transform? centerBone = FindBone(riderMesh, CenterBoneName);
        if (riderMesh == null || head == null || centerBone == null) return null;

        return new RiderRig(camera, playCamera, bike.transform, locomotion, riderMesh, head, centerBone);
    }

    public void Update(bool isRagdollSmoothed)
    {
        UpdateRagdoll(isRagdollSmoothed);
        _firstPersonRider?.Update();
        _ragdollRider?.Update();
    }

    public FirstPersonRider? FindRagdollRider()
    {
        if (_ragdollRider is not null || _ragdoll == null) return _ragdollRider;

        Transform? head = FindBone(FindRiderMesh(_ragdoll.transform), HeadBoneName);
        if (head != null) _ragdollRider = new FirstPersonRider(_ragdoll.transform, head, _camera, _isRiderHidden);

        return _ragdollRider;
    }

    public void SetFirstPerson(bool isFirstPerson, bool isRiderHidden)
    {
        _firstPersonRider?.Destroy();
        _firstPersonRider = isFirstPerson ? new FirstPersonRider(BikeTransform, Head, _camera, isRiderHidden) : null;
        _isRiderHidden = isFirstPerson && isRiderHidden;
    }

    public void SetCamera(CameraViewSettings? view)
    {
        if (_camera == null || _playCamera == null) return;

        if (_playCamera.enabled)
        {
            _gameFieldOfView = _camera.fieldOfView;
            _gameNearClipPlane = _camera.nearClipPlane;
        }

        _camera.fieldOfView = view?.FieldOfView ?? _gameFieldOfView;
        _camera.nearClipPlane = view?.NearClipPlane ?? _gameNearClipPlane;
        _playCamera.enabled = view is null;
    }

    public void Release()
    {
        ReleaseRagdoll();
        _firstPersonRider?.Destroy();
        SetCamera(null);
    }

    private void UpdateRagdoll(bool isSmoothed)
    {
        IsRagdolling = _locomotion.prop_Boolean_2;

        if (!IsRagdolling)
        {
            ReleaseRagdoll();
            return;
        }

        if (_ragdoll == null)
        {
            ReleaseRagdoll();
            _ragdoll = GameObject.Find(RagdollName);
            if (_ragdoll == null) return;
        }

        SetRagdollSmoothed(isSmoothed);
        if (_isRiderHidden) FindRagdollRider();
    }

    private void ReleaseRagdoll()
    {
        SetRagdollSmoothed(false);
        _ragdollRider?.Destroy();
        _ragdollRider = null;
        _ragdoll = null;
    }

    private void SetRagdollSmoothed(bool isSmoothed)
    {
        if (isSmoothed == _isRagdollSmoothed) return;

        _isRagdollSmoothed = isSmoothed;

        foreach ((Rigidbody body, RigidbodyInterpolation interpolation) in _ragdollBodies)
        {
            if (body != null) body.interpolation = interpolation;
        }

        _ragdollBodies.Clear();
        if (!isSmoothed || _ragdoll == null) return;

        foreach (Rigidbody body in _ragdoll.GetComponentsInChildren<Rigidbody>(true))
        {
            _ragdollBodies.Add((body, body.interpolation));
            body.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private static SkinnedMeshRenderer? FindRiderMesh(Transform root)
    {
        Transform mesh = root.Find(RiderMeshPath);
        return mesh == null ? null : mesh.GetComponent<SkinnedMeshRenderer>();
    }

    private static Transform? FindBone(SkinnedMeshRenderer? mesh, string name) =>
        mesh == null ? null : mesh.bones.FirstOrDefault(bone => bone != null && bone.name == name);
}
