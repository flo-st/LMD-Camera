using LmdCamera.Config;
using LmdCamera.Game;
using MelonLoader;
using UnityEngine;

namespace LmdCamera.Core;

internal sealed class CameraController
{
    private const float RigSearchIntervalSeconds = 0.5f;
    private const float RigSettleSeconds = 1f;
    private const double FirstPersonStepMeters = 0.05;
    private const double ThirdPersonStepMeters = 0.1;
    private const double PitchStepDegrees = 2;

    private readonly CameraSettings _settings;
    private readonly MelonLogger.Instance _log;
    private readonly FirstPersonFollower _firstPerson;
    private readonly ThirdPersonFollower _thirdPerson;

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
    }

    public void Update()
    {
        if (_view is null)
        {
            RefreshRig();
            return;
        }

        HandleViewKeys();
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
        if (Time.unscaledTime < _nextRigSearchTime) return;

        _nextRigSearchTime = Time.unscaledTime + RigSearchIntervalSeconds;
        _rig ??= RiderRig.Find();
        if (_rig is not { AgeSeconds: >= RigSettleSeconds } rig) return;

        if (rig.TryGetGameLens(out Lens gameLens)) _gameLens = gameLens;

        ApplyView(_settings.LastView, rig);
    }

    private void ReleaseRig(RiderRig rig)
    {
        _firstPerson.ReleaseRagdoll();
        rig.Release(_gameLens);

        _rig = null;
        _follower = null;
        _view = null;
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
        ViewKeys keys = _settings.ViewKeys;
        if (Input.GetKeyDown(keys.FirstPerson)) return ViewMode.FirstPerson;
        if (Input.GetKeyDown(keys.Default)) return ViewMode.Default;
        if (Input.GetKeyDown(keys.ThirdPerson)) return ViewMode.ThirdPerson;

        return null;
    }

    private void HandleAdjustmentKeys()
    {
        switch (_view)
        {
            case ViewMode.FirstPerson:
                AdjustView(_settings.FirstPerson, FirstPersonStepMeters);
                break;

            case ViewMode.ThirdPerson:
                AdjustView(_settings.ThirdPerson, ThirdPersonStepMeters);
                break;
        }
    }

    private void AdjustView(CameraViewSettings view, double stepMeters)
    {
        AdjustmentKeys keys = _settings.AdjustmentKeys;
        int forward = ReadDirection(keys.Forward, keys.Back);
        int height = ReadDirection(keys.Up, keys.Down);
        int pitch = ReadDirection(keys.PitchDown, keys.PitchUp);
        if (forward == 0 && height == 0 && pitch == 0) return;

        view.Adjust(forward * stepMeters, height * stepMeters, pitch * PitchStepDegrees);
        _settings.Save();
    }

    private void ApplyView(ViewMode view, RiderRig rig)
    {
        if (_gameLens is not { } gameLens) return;

        _firstPerson.ReleaseRagdoll();
        _follower = view switch
        {
            ViewMode.FirstPerson => _firstPerson,
            ViewMode.ThirdPerson => _thirdPerson,
            _ => null,
        };

        Lens lens = view switch
        {
            ViewMode.FirstPerson => _settings.FirstPerson.Lens,
            ViewMode.ThirdPerson => _settings.ThirdPerson.Lens,
            _ => gameLens,
        };
        rig.SetLens(lens, isGameControlled: _follower is null);

        bool isFirstPerson = view == ViewMode.FirstPerson;
        rig.SetHidden(isFirstPerson && _settings.IsRiderHidden, isFirstPerson);

        _follower?.Reset(rig);
        _view = view;
        _log.Msg($"View: {view}");
    }

    private static int ReadDirection(KeyCode positiveKey, KeyCode negativeKey) =>
        (Input.GetKeyDown(positiveKey) ? 1 : 0) - (Input.GetKeyDown(negativeKey) ? 1 : 0);
}
