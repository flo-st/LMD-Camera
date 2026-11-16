using LmdCamera.Config;
using LmdCamera.Game;
using MelonLoader;
using UnityEngine;

namespace LmdCamera.Core;

internal sealed class CameraController
{
    private const float RigSearchIntervalSeconds = 0.5f;
    private const float RigSettleSeconds = 1f;

    private readonly CameraSettings _settings;
    private readonly MelonLogger.Instance _log;
    private readonly FirstPersonFollower _firstPerson;
    private readonly ThirdPersonFollower _thirdPerson;

    private RiderRig? _rig;
    private CameraFollower? _follower;
    private ViewMode? _view;
    private float _nextRigSearchTime;

    public CameraController(CameraSettings settings, MelonLogger.Instance log)
    {
        _settings = settings;
        _log = log;
        _firstPerson = new FirstPersonFollower(settings);
        _thirdPerson = new ThirdPersonFollower(settings);
    }

    public void Update()
    {
        if (_view is null)
        {
            RefreshRig();
            return;
        }

        if (_settings.ReloadIfChanged() && _rig is { IsAlive: true } rig) ApplyView(_view.Value, rig);

        HandleViewKeys();
        if (_settings.AllowAdjustments && _follower is not null) HandleAdjustmentKeys(_follower.View);
    }

    public void LateUpdate()
    {
        if (_rig is not { } rig) return;

        if (!rig.IsAlive)
        {
            rig.Release();
            _rig = null;
            _follower = null;
            _view = null;
            return;
        }

        rig.Update(isRagdollSmoothed: _follower is not null);
        _follower?.Follow(rig, Time.deltaTime);
    }

    private void RefreshRig()
    {
        float time = Time.unscaledTime;
        if (time < _nextRigSearchTime) return;

        _nextRigSearchTime = time + RigSearchIntervalSeconds;
        _rig ??= RiderRig.Find();
        if (_rig is { AgeSeconds: >= RigSettleSeconds, IsAlive: true } rig) ApplyView(_settings.LastView, rig);
    }

    private void HandleViewKeys()
    {
        if (ReadRequestedView() is not { } view || view == _view || _rig is not { IsAlive: true } rig) return;

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
        if (Input.GetKeyDown(_settings.FirstPersonKey)) return ViewMode.FirstPerson;
        if (Input.GetKeyDown(_settings.DefaultKey)) return ViewMode.Default;
        if (Input.GetKeyDown(_settings.ThirdPersonKey)) return ViewMode.ThirdPerson;

        return null;
    }

    private void HandleAdjustmentKeys(CameraViewSettings view)
    {
        int forwardSteps = ReadDirection(_settings.AdjustForwardKey, _settings.AdjustBackKey);
        int heightSteps = ReadDirection(_settings.AdjustUpKey, _settings.AdjustDownKey);
        int pitchSteps = ReadDirection(_settings.AdjustPitchDownKey, _settings.AdjustPitchUpKey);
        if (forwardSteps == 0 && heightSteps == 0 && pitchSteps == 0) return;

        view.Adjust(forwardSteps, heightSteps, pitchSteps);
        _settings.Save();
    }

    private void ApplyView(ViewMode view, RiderRig rig)
    {
        _follower = view switch
        {
            ViewMode.FirstPerson => _firstPerson,
            ViewMode.ThirdPerson => _thirdPerson,
            _ => null,
        };

        rig.SetCamera(_follower?.View);
        rig.SetFirstPerson(view == ViewMode.FirstPerson, _settings.IsRiderHidden);
        _follower?.Reset(rig);

        _view = view;
        _log.Msg($"View: {view}");
    }

    private static int ReadDirection(KeyCode positiveKey, KeyCode negativeKey) =>
        (Input.GetKeyDown(positiveKey) ? 1 : 0) - (Input.GetKeyDown(negativeKey) ? 1 : 0);
}
