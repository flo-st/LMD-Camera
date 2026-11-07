using LmdCamera.Config;
using LmdCamera.Game;
using MelonLoader;
using UnityEngine;

namespace LmdCamera.Core;

internal sealed class CameraController
{
    private const float RigSearchIntervalSeconds = 0.5f;
    private const float RigSettleSeconds = 1f;
    private const float FirstPersonStep = 0.05f;
    private const float ThirdPersonStep = 0.1f;
    private const float PitchStep = 2f;

    private readonly CameraSettings _settings;
    private readonly CameraPositioner _positioner;
    private readonly MelonLogger.Instance _log;
    private readonly Interval _rigSearch = new(RigSearchIntervalSeconds);
    private readonly (KeyCode Key, Adjustment Change)[] _firstPersonAdjustments;
    private readonly (KeyCode Key, Adjustment Change)[] _thirdPersonAdjustments;

    private RiderRig? _rig;
    private ViewMode _view = ViewMode.None;

    public CameraController(CameraSettings settings, MelonLogger.Instance log)
    {
        _settings = settings;
        _positioner = new CameraPositioner(settings);
        _log = log;
        _firstPersonAdjustments = settings.AdjustmentKeys.Bindings(FirstPersonStep, PitchStep);
        _thirdPersonAdjustments = settings.AdjustmentKeys.Bindings(ThirdPersonStep, PitchStep);
    }

    private bool IsCustomView => _view is ViewMode.FirstPerson or ViewMode.ThirdPerson;

    public void Update()
    {
        if (_rigSearch.HasElapsed()) RefreshRig();
        if (_view == ViewMode.None || _rig is not { IsAlive: true } rig) return;

        HandleViewKeys(rig);
        if (_settings.AllowAdjustments) HandleAdjustmentKeys();
    }

    public void LateUpdate()
    {
        if (IsCustomView && _rig is { IsAlive: true } rig)
            _positioner.Follow(_view, rig);
    }

    private void RefreshRig()
    {
        if (_rig is { IsAlive: false })
        {
            _rig = null;
            _view = ViewMode.None;
        }

        _rig ??= RiderRig.Find();

        if (_view == ViewMode.None && _rig is { Age: >= RigSettleSeconds } rig)
            ApplyView(_settings.LastView, rig);
    }

    private void HandleViewKeys(RiderRig rig)
    {
        var requestedView = ReadRequestedView();
        if (requestedView == ViewMode.None || requestedView == _view) return;

        if (rig.IsRagdolling)
        {
            _log.Msg("Camera switching is disabled during ragdoll");
            return;
        }

        ApplyView(requestedView, rig);
        _settings.LastView = requestedView;
    }

    private ViewMode ReadRequestedView()
    {
        foreach (var (key, view) in _settings.ViewKeys.Bindings)
        {
            if (Input.GetKeyDown(key)) return view;
        }

        return ViewMode.None;
    }

    private void HandleAdjustmentKeys()
    {
        switch (_view)
        {
            case ViewMode.FirstPerson:
                Adjust(_settings.FirstPerson, _firstPersonAdjustments);
                break;

            case ViewMode.ThirdPerson:
                Adjust(_settings.ThirdPerson, _thirdPersonAdjustments);
                break;
        }
    }

    private void Adjust(CameraViewSettings view, (KeyCode Key, Adjustment Change)[] bindings)
    {
        var changed = false;

        foreach (var (key, change) in bindings)
        {
            if (!Input.GetKeyDown(key)) continue;

            view.Adjust(change);
            changed = true;
        }

        if (changed) _settings.Save();
    }

    private void ApplyView(ViewMode view, RiderRig rig)
    {
        _positioner.Apply(view, rig);
        _view = view;
        _log.Msg($"View: {view}");
    }
}
