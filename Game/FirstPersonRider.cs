using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace LmdCamera.Game;

internal sealed class FirstPersonRider
{
    private const string RiderBoneName = "neck_BindJNT";
    private const string CosmeticPrefix = "rider_";

    private static readonly HashSet<string> VisibleBones = new()
    {
        "root_BindJNT", "cog_BindJNT", "handlebar_BindJNT", "pedal_BindJNT", "back_suspension_BindJNT",
        "wheel_front_axis_BindJNT", "wheel_front_spin_BindJNT", "wheel_back_axis_BindJNT", "wheel_back_spin_BindJNT",
        "spine_1_BindJNT", "spine_2_BindJNT", "spine_3_BindJNT",
        "hip_L_BindJNT", "hip_R_BindJNT", "knee_L_BindJNT", "knee_R_BindJNT",
        "ankle_L_BindJNT", "ankle_R_BindJNT", "toe_L_BindJNT", "toe_R_BindJNT",
        "shoulder_L_BindJNT", "shoulder_R_BindJNT", "elbow_L_BindJNT", "elbow_R_BindJNT",
        "hand_L_BindJNT", "hand_R_BindJNT", "finger_L_BindJNT", "finger_R_BindJNT",
    };

    private readonly Camera _camera;
    private readonly Camera.CameraCallback? _beforeRender;
    private readonly Camera.CameraCallback? _afterRender;
    private readonly GameObject _holder = new("LMD-Camera FirstPersonRider");
    private readonly MaterialPropertyBlock _properties = new();
    private readonly List<(Renderer Renderer, bool IsOff)> _hiddenRenderers = new();
    private readonly List<(Renderer Renderer, ShadowCastingMode Mode)> _shadowOnlyRenderers = new();
    private readonly List<(SkinnedMeshRenderer View, SkinnedMeshRenderer Source, GameObject Owner)> _views = new();
    private readonly List<(Transform Point, Transform Anchor)> _collapsePoints = new();

    public FirstPersonRider(Transform root, Transform head, Camera camera, bool isRiderHidden)
    {
        Head = head;
        _camera = camera;
        _beforeRender = DelegateSupport.ConvertDelegate<Camera.CameraCallback>(new Action<Camera>(ShowBeforeRender));
        _afterRender = DelegateSupport.ConvertDelegate<Camera.CameraCallback>(new Action<Camera>(HideAfterRender));

        if (isRiderHidden)
        {
            CollectHiddenRenderers(root);
        }
        else
        {
            CreateHeadlessViews(root);
            Update();
        }

        Camera.onPreCull = Il2CppSystem.Delegate.Combine(Camera.onPreCull, _beforeRender)
            .Cast<Camera.CameraCallback>();
        Camera.onPostRender = Il2CppSystem.Delegate.Combine(Camera.onPostRender, _afterRender)
            .Cast<Camera.CameraCallback>();
    }

    public Transform Head { get; }
    public Action? BeforeCameraRender { get; set; }

    public void Update()
    {
        foreach ((Transform point, Transform anchor) in _collapsePoints)
        {
            point.position = anchor.position;
        }
    }

    public void Destroy()
    {
        Camera.onPreCull = Il2CppSystem.Delegate.Remove(Camera.onPreCull, _beforeRender)
            ?.Cast<Camera.CameraCallback>();
        Camera.onPostRender = Il2CppSystem.Delegate.Remove(Camera.onPostRender, _afterRender)
            ?.Cast<Camera.CameraCallback>();

        Object.Destroy(_holder);
    }

    private void CollectHiddenRenderers(Transform root)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.TryCast<MeshRenderer>() != null || renderer.TryCast<SkinnedMeshRenderer>() != null)
            {
                _hiddenRenderers.Add((renderer, renderer.forceRenderingOff));
            }
        }
    }

    private void CreateHeadlessViews(Transform root)
    {
        var points = new Dictionary<IntPtr, Transform>();
        foreach (SkinnedMeshRenderer mesh in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (mesh.bones.Any(bone => bone is { name: RiderBoneName }))
            {
                _views.Add((CreateView(mesh, points), mesh, mesh.gameObject));
            }
        }

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (IsCosmetic(renderer.transform, root) || _views.Any(view => view.Source.Pointer == renderer.Pointer))
            {
                _shadowOnlyRenderers.Add((renderer, renderer.shadowCastingMode));
            }
        }
    }

    private SkinnedMeshRenderer CreateView(SkinnedMeshRenderer source, Dictionary<IntPtr, Transform> points)
    {
        Il2CppReferenceArray<Transform> bones = source.bones;
        Vector3[] bindPositions = GetBindPositions(source.sharedMesh, bones.Length);
        var viewBones = new Il2CppReferenceArray<Transform>(bones.Length);

        for (var i = 0; i < bones.Length; i++)
        {
            Transform bone = bones[i];
            viewBones[i] = IsHidden(bone) ? GetCollapsePoint(FindAnchor(i, bones, bindPositions), points) : bone;
        }

        SkinnedMeshRenderer view = new GameObject(source.name).AddComponent<SkinnedMeshRenderer>();
        view.transform.SetParent(_holder.transform, false);
        view.sharedMesh = source.sharedMesh;
        view.sharedMaterials = source.sharedMaterials;
        view.rootBone = source.rootBone;
        view.bones = viewBones;
        view.updateWhenOffscreen = true;
        view.shadowCastingMode = ShadowCastingMode.Off;
        view.forceRenderingOff = true;
        return view;
    }

    private Transform GetCollapsePoint(Transform anchor, Dictionary<IntPtr, Transform> points)
    {
        if (points.TryGetValue(anchor.Pointer, out Transform? point)) return point;

        point = new GameObject("CollapsedPart").transform;
        point.SetParent(_holder.transform, false);
        point.localScale = Vector3.zero;
        points[anchor.Pointer] = point;
        _collapsePoints.Add((point, anchor));
        return point;
    }

    private void ShowBeforeRender(Camera camera)
    {
        if (camera.Pointer != _camera.Pointer) return;

        BeforeCameraRender?.Invoke();
        SetFirstPersonLook(true);
    }

    private void HideAfterRender(Camera camera)
    {
        if (camera.Pointer == _camera.Pointer) SetFirstPersonLook(false);
    }

    private void SetFirstPersonLook(bool isShown)
    {
        foreach ((Renderer renderer, bool isOff) in _hiddenRenderers)
        {
            if (renderer != null) renderer.forceRenderingOff = isShown || isOff;
        }

        foreach ((Renderer renderer, ShadowCastingMode mode) in _shadowOnlyRenderers)
        {
            if (renderer != null) renderer.shadowCastingMode = isShown ? ShadowCastingMode.ShadowsOnly : mode;
        }

        foreach ((SkinnedMeshRenderer view, SkinnedMeshRenderer source, GameObject owner) in _views)
        {
            if (view == null || source == null) continue;

            bool isViewShown = isShown && source.enabled && owner.activeInHierarchy;
            if (isViewShown)
            {
                source.GetPropertyBlock(_properties);
                view.SetPropertyBlock(_properties);
            }

            view.forceRenderingOff = !isViewShown;
        }
    }

    private static bool IsHidden(Transform bone) => bone != null && !VisibleBones.Contains(bone.name);

    private static Transform FindAnchor(int boneIndex, Il2CppReferenceArray<Transform> bones, Vector3[] bindPositions)
    {
        Transform anchor = bones[boneIndex];
        float closestDistance = float.MaxValue;

        for (var i = 0; i < bones.Length; i++)
        {
            Transform candidate = bones[i];
            if (candidate == null || !VisibleBones.Contains(candidate.name)) continue;

            float distance = (bindPositions[i] - bindPositions[boneIndex]).sqrMagnitude;
            if (distance >= closestDistance) continue;

            anchor = candidate;
            closestDistance = distance;
        }

        return anchor;
    }

    private static Vector3[] GetBindPositions(Mesh mesh, int boneCount)
    {
        Il2CppStructArray<Matrix4x4> bindPoses = mesh.bindposes;
        var positions = new Vector3[boneCount];

        for (var i = 0; i < boneCount && i < bindPoses.Length; i++)
        {
            positions[i] = bindPoses[i].inverse.MultiplyPoint3x4(Vector3.zero);
        }

        return positions;
    }

    private static bool IsCosmetic(Transform transform, Transform root)
    {
        Transform current = transform;
        while (current != null && current != root)
        {
            if (current.name.StartsWith(CosmeticPrefix)) return true;

            current = current.parent;
        }

        return false;
    }
}
