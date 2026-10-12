using LMD_Camera.Config;
using LMD_Camera.Game;
using MelonLoader;
using UnityEngine;

namespace LMD_Camera.Core;

internal class CameraInputHandler
{
    private const float CheckIntervalSeconds = 0.5f;
    private const float InitDelaySeconds = 1f;

    private readonly CameraSettings _cameraSettings;
    private readonly CameraViewApplier _cameraViewApplier;
    private readonly MelonLogger.Instance _log;
    private readonly Interval _checkInterval = new(CheckIntervalSeconds);

    private RiderRig? _rig;
    private ViewMode _view = ViewMode.None;

    public CameraInputHandler(CameraSettings cameraSettings, MelonLogger.Instance log)
    {
        _cameraSettings = cameraSettings;
        _cameraViewApplier = new CameraViewApplier(cameraSettings);
        _log = log;
    }

    public void Update()
    {
        if (_checkInterval.HasElapsed()) FindOrRefreshRig();
        HandleViewSwitchKeys();
    }

    public void LateUpdate()
    {
        if (_rig is { IsAlive: true } rig && _view != ViewMode.None)
            _cameraViewApplier.UpdateCamera(_view, rig);
    }

    private void FindOrRefreshRig()
    {
        if (_rig is { IsAlive: false }) ResetRigAndView();

        _rig ??= RiderRig.Find();

        if (_rig is { Age: >= InitDelaySeconds } rig && _view == ViewMode.None)
            ApplyView(_cameraSettings.LastView, rig);
    }

    private void HandleViewSwitchKeys()
    {
        if (_rig is not { IsAlive: true } rig || _view == ViewMode.None) return;

        var requestedView = _cameraSettings.ViewKeys.Bindings
            .FirstOrDefault(binding => Input.GetKeyDown(binding.Key)).View;

        if (requestedView == ViewMode.None || requestedView == _view) return;

        if (rig.IsRagdolling)
        {
            _log.Msg("Camera switching is disabled during ragdoll");
            return;
        }

        ApplyView(requestedView, rig);
        _cameraSettings.LastView = requestedView;
    }

    private void ApplyView(ViewMode view, RiderRig rig)
    {
        _cameraViewApplier.ApplyViewSettings(view, rig);
        _view = view;
        _log.Msg($"View: {view}");
    }

    private void ResetRigAndView()
    {
        _rig = null;
        _view = ViewMode.None;
    }
}
