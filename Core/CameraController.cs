using LmdCamera.Config;
using LmdCamera.Game;
using MelonLoader;
using UnityEngine;

namespace LmdCamera.Core;

internal sealed class CameraController
{
    private const float RigSearchIntervalSeconds = 0.5f;
    private const float RigSettleSeconds = 1f;
    private const double FirstPersonStep = 0.05;
    private const double ThirdPersonStep = 0.1;
    private const double PitchStep = 2;

    private readonly CameraSettings _settings;
    private readonly CameraPositioner _positioner;
    private readonly MelonLogger.Instance _log;
    private readonly (KeyCode Key, Adjustment Change)[] _firstPersonAdjustments;
    private readonly (KeyCode Key, Adjustment Change)[] _thirdPersonAdjustments;

    private RiderRig? _rig;
    private ViewMode _view = ViewMode.None;
    private float _nextRigSearchTime;

    public CameraController(CameraSettings settings, MelonLogger.Instance log)
    {
        _settings = settings;
        _positioner = new CameraPositioner(settings);
        _log = log;
        _firstPersonAdjustments = settings.AdjustmentKeys.CreateBindings(FirstPersonStep, PitchStep);
        _thirdPersonAdjustments = settings.AdjustmentKeys.CreateBindings(ThirdPersonStep, PitchStep);
    }

    public void Update()
    {
        if (Time.unscaledTime >= _nextRigSearchTime)
        {
            _nextRigSearchTime = Time.unscaledTime + RigSearchIntervalSeconds;
            RefreshRig();
        }

        if (_view == ViewMode.None || _rig is not { IsAlive: true } rig) return;

        HandleViewKeys(rig);
        if (_settings.AllowAdjustments) HandleAdjustmentKeys();
    }

    public void LateUpdate()
    {
        if (_rig is { IsAlive: true } rig) _positioner.Follow(rig);
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
