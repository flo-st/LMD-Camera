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
    private readonly MelonLogger.Instance _log;
    private readonly FirstPersonFollower _firstPerson;
    private readonly ThirdPersonFollower _thirdPerson;
    private readonly (KeyCode Key, Adjustment Change)[] _firstPersonAdjustments;
    private readonly (KeyCode Key, Adjustment Change)[] _thirdPersonAdjustments;

    private RiderRig? _rig;
    private ICameraFollower? _follower;
    private ViewMode? _view;
    private Lens? _gameLens;
    private float _nextRigSearchTime;

    public CameraController(CameraSettings settings, MelonLogger.Instance log)
    {
        _settings = settings;
        _log = log;
        _firstPerson = new FirstPersonFollower(settings);
        _thirdPerson = new ThirdPersonFollower(settings);
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

        if (_view is null || _rig is not { IsAlive: true } rig) return;

        HandleViewKeys(rig);
        if (_settings.AllowAdjustments) HandleAdjustmentKeys();
    }

    public void LateUpdate()
    {
        if (_rig is not { } rig) return;

        if (!rig.IsAlive)
        {
            ReleaseRig(rig);
            return;
        }

        rig.SetRagdollSmoothed(_view is ViewMode.FirstPerson or ViewMode.ThirdPerson);
        _follower?.Follow(rig);
        rig.UpdateFirstPersonRider();
    }

    private void RefreshRig()
    {
        _rig ??= RiderRig.Find();
        if (_view is not null || _rig is not { AgeSeconds: >= RigSettleSeconds } rig) return;

        if (rig.PlayCamera.enabled) _gameLens = new Lens(rig.Camera.fieldOfView, rig.Camera.nearClipPlane);

        ApplyView(_settings.LastView, rig);
    }

    private void ReleaseRig(RiderRig rig)
    {
        _firstPerson.ReleaseRagdoll();
        rig.SetRagdollSmoothed(false);
        rig.DestroyFirstPersonRider();
        if (_gameLens is { } gameLens) rig.RestoreGameCamera(gameLens);

        _rig = null;
        _follower = null;
        _view = null;
    }

    private void HandleViewKeys(RiderRig rig)
    {
        if (ReadRequestedView() is not { } view || view == _view) return;

        if (rig.IsRagdolling)
        {
            _log.Msg("Camera switching is disabled during ragdoll");
            return;
        }

        ApplyView(view, rig);
        _settings.LastView = view;
    }

    private ViewMode? ReadRequestedView()
    {
        foreach ((KeyCode key, ViewMode view) in _settings.ViewKeys.Bindings)
        {
            if (Input.GetKeyDown(key)) return view;
        }

        return null;
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

        foreach ((KeyCode key, Adjustment change) in bindings)
        {
            if (!Input.GetKeyDown(key)) continue;

            view.Adjust(change);
            changed = true;
        }

        if (changed) _settings.Save();
    }

    private void ApplyView(ViewMode view, RiderRig rig)
    {
        if (_gameLens is not { } gameLens) return;

        Lens lens = view switch
        {
            ViewMode.FirstPerson => _settings.FirstPerson.Lens,
            ViewMode.ThirdPerson => _settings.ThirdPerson.Lens,
            _ => gameLens,
        };
        rig.Camera.fieldOfView = lens.FieldOfView;
        rig.Camera.nearClipPlane = lens.NearClipPlane;
        rig.PlayCamera.enabled = view == ViewMode.Default;

        bool isFirstPerson = view == ViewMode.FirstPerson;
        rig.SetHidden(isFirstPerson && _settings.HideRider, isFirstPerson);
        _firstPerson.ReleaseRagdoll();

        _follower = view switch
        {
            ViewMode.FirstPerson => _firstPerson,
            ViewMode.ThirdPerson => _thirdPerson,
            _ => null,
        };

        _follower?.Reset(rig);
        _view = view;
        _log.Msg($"View: {view}");
    }
}
