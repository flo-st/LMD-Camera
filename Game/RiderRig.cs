using Il2CppMegagon.Downhill.Cameras;
using Il2CppMegagon.Downhill.Vehicle.Controller;
using UnityEngine;

namespace LMD_Camera.Game;

internal class RiderRig
{
    private const string PlayCameraName = "PlayCamera(Clone)";
    private const string BikeName = "Bike(Clone)";
    private const string RiderMeshPath = "bike/mergedMesh";
    private const string HeadBoneName = "head_tip_BindJNT";

    private static readonly string[] HairNames = { "rider_hair", "rider_facialhair" };

    private readonly BikeLocomotion _locomotion;
    private readonly List<GameObject> _riderParts;
    private readonly float _foundAt = Time.unscaledTime;

    private RiderRig(
        Camera camera,
        PlayCamera playCamera,
        GameObject bike,
        BikeLocomotion locomotion,
        SkinnedMeshRenderer riderMesh,
        Transform head)
    {
        Camera = camera;
        PlayCamera = playCamera;
        Bike = bike;
        Head = head;
        _locomotion = locomotion;

        riderMesh.updateWhenOffscreen = true;
        _riderParts = FindHair(bike).Append(riderMesh.gameObject).ToList();
    }

    public Camera Camera { get; }
    public PlayCamera PlayCamera { get; }
    public GameObject Bike { get; }
    public Transform Head { get; }

    public Transform BikeTransform => Bike.transform;
    public float Age => Time.unscaledTime - _foundAt;
    public bool IsRagdolling => _locomotion.prop_Boolean_2;
    public bool IsAlive => Camera != null && Bike != null && Camera.gameObject.activeInHierarchy;

    public static RiderRig? Find()
    {
        var cameraObject = GameObject.Find(PlayCameraName);
        var bike = GameObject.Find(BikeName);

        var camera = cameraObject?.GetComponent<Camera>();
        var playCamera = cameraObject?.GetComponent<PlayCamera>();
        var locomotion = bike?.GetComponent<BikeLocomotion>();
        var riderMesh = bike?.transform.Find(RiderMeshPath)?.GetComponent<SkinnedMeshRenderer>();
        var head = riderMesh?.bones.FirstOrDefault(bone => bone is { name: HeadBoneName });

        return camera is null || playCamera is null || bike is null
            || locomotion is null || riderMesh is null || head is null
            ? null
            : new RiderRig(camera, playCamera, bike, locomotion, riderMesh, head);
    }

    public void SetRiderVisible(bool visible)
    {
        foreach (var part in _riderParts)
            part.SetActive(visible);
    }

    private static IEnumerable<GameObject> FindHair(GameObject bike) =>
        bike.GetComponentsInChildren<Transform>(true)
            .Where(child => HairNames.Any(hairName => child.name.Contains(hairName)))
            .Select(child => child.gameObject);
}
