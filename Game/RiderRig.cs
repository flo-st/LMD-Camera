using Il2CppMegagon.Downhill.Cameras;
using Il2CppMegagon.Downhill.Vehicle.Controller;
using UnityEngine;

namespace LmdCamera.Game;

internal sealed class RiderRig
{
    private const string PlayCameraName = "PlayCamera(Clone)";
    private const string BikeName = "Bike(Clone)";
    private const string RiderMeshPath = "bike/mergedMesh";
    private const string HeadBoneName = "head_tip_BindJNT";

    private static readonly string[] HairNames = { "rider_hair", "rider_facialhair" };

    private readonly GameObject _cameraObject;
    private readonly BikeLocomotion _locomotion;
    private readonly GameObject[] _riderParts;
    private readonly float _foundAt = Time.unscaledTime;

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
        _riderParts = FindHair(bike).Append(riderMesh.gameObject).ToArray();
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
        if (cameraObject is null || bike is null) return null;

        var camera = cameraObject.GetComponent<Camera>();
        var playCamera = cameraObject.GetComponent<PlayCamera>();
        var locomotion = bike.GetComponent<BikeLocomotion>();
        var riderMesh = bike.transform.Find(RiderMeshPath)?.GetComponent<SkinnedMeshRenderer>();
        var head = riderMesh?.bones.FirstOrDefault(bone => bone is { name: HeadBoneName });

        return camera is null || playCamera is null || locomotion is null || riderMesh is null || head is null
            ? null
            : new RiderRig(camera, playCamera, bike.transform, locomotion, riderMesh, head);
    }

    public void SetRiderVisible(bool visible)
    {
        foreach (var part in _riderParts)
            part.SetActive(visible);
    }

    private static IEnumerable<GameObject> FindHair(Transform bike) =>
        bike.GetComponentsInChildren<Transform>(true)
            .Where(child => HairNames.Any(child.name.Contains))
            .Select(child => child.gameObject);
}
