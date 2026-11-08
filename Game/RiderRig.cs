using Il2CppMegagon.Downhill.Cameras;
using Il2CppMegagon.Downhill.Vehicle.Controller;
using UnityEngine;

namespace LmdCamera.Game;

internal sealed class RiderRig
{
    private const string PlayCameraName = "PlayCamera(Clone)";
    private const string BikeName = "Bike(Clone)";
    private const string RagdollName = "RagDoll(Clone)";
    private const string RiderMeshPath = "bike/mergedMesh";
    private const string HeadBoneName = "head_tip_BindJNT";

    private readonly GameObject _cameraObject;
    private readonly BikeLocomotion _locomotion;
    private readonly float _foundAt = Time.unscaledTime;

    private Renderer[] _riderRenderers;

    private RiderRig(
        Camera camera,
        PlayCamera playCamera,
        Transform bike,
        BikeLocomotion locomotion,
        SkinnedMeshRenderer riderMesh,
        Transform head)
    {
        Camera = camera;
        CameraTransform = camera.transform;
        PlayCamera = playCamera;
        BikeTransform = bike;
        Head = head;
        _cameraObject = camera.gameObject;
        _locomotion = locomotion;

        riderMesh.updateWhenOffscreen = true;
        _riderRenderers = FindRiderRenderers(bike);
    }

    public Camera Camera { get; }
    public Transform CameraTransform { get; }
    public PlayCamera PlayCamera { get; }
    public Transform BikeTransform { get; }
    public Transform Head { get; }

    public float Age => Time.unscaledTime - _foundAt;
    public bool IsRagdolling => _locomotion.prop_Boolean_2;
    public bool IsAlive => _cameraObject != null && BikeTransform != null && _cameraObject.activeInHierarchy;

    public static RiderRig? Find()
    {
        var cameraObject = GameObject.Find(PlayCameraName);
        var bike = GameObject.Find(BikeName);
        if (cameraObject == null || bike == null) return null;

        var camera = cameraObject.GetComponent<Camera>();
        var playCamera = cameraObject.GetComponent<PlayCamera>();
        var locomotion = bike.GetComponent<BikeLocomotion>();
        var riderMesh = FindRiderMesh(bike.transform);
        var head = FindHeadBone(riderMesh);

        return camera == null || playCamera == null || locomotion == null || riderMesh == null || head == null
            ? null
            : new RiderRig(camera, playCamera, bike.transform, locomotion, riderMesh, head);
    }

    public static Ragdoll? FindRagdoll()
    {
        var ragdoll = GameObject.Find(RagdollName);
        if (ragdoll == null) return null;

        var head = FindHeadBone(FindRiderMesh(ragdoll.transform));
        return head == null ? null : new Ragdoll(head, FindRiderRenderers(ragdoll.transform));
    }

    public void SetRiderVisible(bool visible)
    {
        if (Array.Exists(_riderRenderers, renderer => renderer == null))
            _riderRenderers = FindRiderRenderers(BikeTransform);

        foreach (var renderer in _riderRenderers)
            renderer.enabled = visible;
    }

    private static SkinnedMeshRenderer? FindRiderMesh(Transform root) =>
        root.Find(RiderMeshPath)?.GetComponent<SkinnedMeshRenderer>();

    private static Transform? FindHeadBone(SkinnedMeshRenderer? mesh) =>
        mesh == null ? null : mesh.bones.FirstOrDefault(bone => bone is { name: HeadBoneName });

    private static Renderer[] FindRiderRenderers(Transform root) =>
        root.GetComponentsInChildren<MeshRenderer>(true).Select(renderer => (Renderer)renderer)
            .Concat(root.GetComponentsInChildren<SkinnedMeshRenderer>(true).Select(renderer => (Renderer)renderer))
            .ToArray();
}
