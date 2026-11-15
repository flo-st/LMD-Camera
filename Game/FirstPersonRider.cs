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

    private static readonly HashSet<string> VisibleBones = new[]
        {
            "root", "cog", "handlebar", "pedal", "back_suspension",
            "wheel_front_axis", "wheel_front_spin", "wheel_back_axis", "wheel_back_spin",
            "spine_1", "spine_2", "spine_3",
        }
        .Concat(new[] { "hip", "knee", "ankle", "toe", "shoulder", "elbow", "hand", "finger" }
            .SelectMany(part => new[] { $"{part}_L", $"{part}_R" }))
        .Select(name => $"{name}_BindJNT")
        .ToHashSet();

    private readonly Camera _camera;
    private readonly Camera.CameraCallback? _beforeRender;
    private readonly Camera.CameraCallback? _afterRender;
    private readonly GameObject _holder = new("LMD-Camera FirstPersonRider");
    private readonly MaterialPropertyBlock _properties = new();
    private readonly List<(Renderer Renderer, ShadowCastingMode Mode)> _shadowOnly = new();
    private readonly List<(SkinnedMeshRenderer View, SkinnedMeshRenderer Source, GameObject Owner)> _views = new();
    private readonly List<(Transform Point, Transform Anchor)> _collapsePoints = new();
    private readonly List<(Renderer Renderer, bool IsOff)> _riderRenderers = new();

    private bool _isHeadHidden;
    private bool _isRiderHidden;
    private bool _isRegistered;

    public FirstPersonRider(Transform root, Transform head, Camera camera)
    {
        Head = head;
        _camera = camera;
        _beforeRender = DelegateSupport.ConvertDelegate<Camera.CameraCallback>(new Action<Camera>(HideBeforeRender));
        _afterRender = DelegateSupport.ConvertDelegate<Camera.CameraCallback>(new Action<Camera>(RestoreAfterRender));

        foreach (SkinnedMeshRenderer mesh in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (mesh.bones.Any(bone => bone is { name: RiderBoneName }))
            {
                _views.Add((CreateView(mesh), mesh, mesh.gameObject));
            }
        }

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (IsCosmetic(renderer.transform, root) || _views.Any(view => view.Source.Pointer == renderer.Pointer))
            {
                _shadowOnly.Add((renderer, renderer.shadowCastingMode));
            }

            if (renderer.TryCast<MeshRenderer>() != null || renderer.TryCast<SkinnedMeshRenderer>() != null)
            {
                _riderRenderers.Add((renderer, renderer.forceRenderingOff));
            }
        }
    }

    public Transform Head { get; }
    public Action? BeforeCameraRender { get; set; }

    public void SetHidden(bool isRiderHidden, bool isHeadHidden)
    {
        _isRiderHidden = isRiderHidden;
        _isHeadHidden = isHeadHidden;

        bool isViewNeeded = isHeadHidden && !isRiderHidden;
        foreach ((SkinnedMeshRenderer view, _, _) in _views)
        {
            if (view != null) view.gameObject.SetActive(isViewNeeded);
        }

        UpdateRenderCallbacks();
    }

    public void Update()
    {
        if (!_isHeadHidden || _isRiderHidden) return;

        foreach ((Transform point, Transform anchor) in _collapsePoints)
        {
            if (anchor != null) point.position = anchor.position;
        }
    }

    public void Destroy()
    {
        SetHidden(false, false);
        Object.Destroy(_holder);
    }

    private void UpdateRenderCallbacks()
    {
        bool isNeeded = _isHeadHidden || _isRiderHidden;
        if (isNeeded == _isRegistered) return;

        _isRegistered = isNeeded;
        if (isNeeded)
        {
            Camera.onPreCull = Il2CppSystem.Delegate.Combine(Camera.onPreCull, _beforeRender)
                .Cast<Camera.CameraCallback>();
            Camera.onPostRender = Il2CppSystem.Delegate.Combine(Camera.onPostRender, _afterRender)
                .Cast<Camera.CameraCallback>();
            return;
        }

        var beforeRender = Il2CppSystem.Delegate.Remove(Camera.onPreCull, _beforeRender);
        Camera.onPreCull = beforeRender?.Cast<Camera.CameraCallback>();

        var afterRender = Il2CppSystem.Delegate.Remove(Camera.onPostRender, _afterRender);
        Camera.onPostRender = afterRender?.Cast<Camera.CameraCallback>();
    }

    private SkinnedMeshRenderer CreateView(SkinnedMeshRenderer source)
    {
        Il2CppReferenceArray<Transform> bones = source.bones;
        Vector3[] bindPositions = GetBindPositions(source.sharedMesh, bones.Length);
        var viewBones = new Il2CppReferenceArray<Transform>(bones.Length);
        var points = new Dictionary<IntPtr, Transform>();

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
        view.gameObject.SetActive(false);
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

    private void HideBeforeRender(Camera camera)
    {
        if (camera.Pointer != _camera.Pointer) return;

        BeforeCameraRender?.Invoke();

        if (_isRiderHidden)
        {
            foreach ((Renderer renderer, _) in _riderRenderers)
            {
                if (renderer != null) renderer.forceRenderingOff = true;
            }

            return;
        }

        if (!_isHeadHidden) return;

        foreach ((Renderer renderer, _) in _shadowOnly)
        {
            if (renderer != null) renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
        }

        foreach ((SkinnedMeshRenderer view, SkinnedMeshRenderer source, GameObject owner) in _views)
        {
            if (view == null || source == null) continue;

            source.GetPropertyBlock(_properties);
            view.SetPropertyBlock(_properties);
            view.forceRenderingOff = !source.enabled || !owner.activeInHierarchy;
        }
    }

    private void RestoreAfterRender(Camera camera)
    {
        if (camera.Pointer != _camera.Pointer) return;

        if (_isRiderHidden)
        {
            foreach ((Renderer renderer, bool isOff) in _riderRenderers)
            {
                if (renderer != null) renderer.forceRenderingOff = isOff;
            }

            return;
        }

        if (!_isHeadHidden) return;

        foreach ((Renderer renderer, ShadowCastingMode mode) in _shadowOnly)
        {
            if (renderer != null) renderer.shadowCastingMode = mode;
        }

        foreach ((SkinnedMeshRenderer view, _, _) in _views)
        {
            if (view != null) view.forceRenderingOff = true;
        }
    }
}
