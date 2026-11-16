using System.Diagnostics.CodeAnalysis;
using MelonLoader;
using UnityEngine;

namespace LmdCamera.Config;

internal sealed class CameraSettings
{
    private const string FilePath = "UserData/LMD-Camera.cfg";

    private readonly MelonPreferences_Category _category = MelonPreferences.CreateCategory("LMD-Camera");
    private readonly MelonPreferences_Entry<ViewMode> _lastView;

    private bool _isChanged;

    public CameraSettings()
    {
        _category.SetFilePath(FilePath);
        _lastView = _category.CreateEntry("LastViewMode", ViewMode.Default);
        Load();
        Save();

        foreach (MelonPreferences_Entry entry in _category.Entries)
        {
            entry.OnEntryValueChangedUntyped.Subscribe((_, _) => _isChanged = true);
        }
    }

    public bool AllowAdjustments { get; private set; }

    public bool IsRiderHidden { get; private set; }
    public bool IsImmersiveMode { get; private set; }
    public CameraViewSettings FirstPerson { get; private set; }
    public float FirstPersonRotationSmoothing { get; private set; }
    public float FirstPersonHeadSmoothing { get; private set; }
    public float FirstPersonTiltSmoothing { get; private set; }
    public float FirstPersonRoll { get; private set; }
    public float FirstPersonFlipCatchUp { get; private set; }

    public CameraViewSettings ThirdPerson { get; private set; }
    public float ThirdPersonHeadingSmoothing { get; private set; }
    public float ThirdPersonVerticalSmoothing { get; private set; }

    public KeyCode FirstPersonKey { get; private set; }
    public KeyCode DefaultKey { get; private set; }
    public KeyCode ThirdPersonKey { get; private set; }
    public KeyCode AdjustUpKey { get; private set; }
    public KeyCode AdjustDownKey { get; private set; }
    public KeyCode AdjustForwardKey { get; private set; }
    public KeyCode AdjustBackKey { get; private set; }
    public KeyCode AdjustPitchUpKey { get; private set; }
    public KeyCode AdjustPitchDownKey { get; private set; }

    public ViewMode LastView
    {
        get => _lastView.Value;
        set
        {
            _lastView.Value = value;
            Save();
        }
    }

    public void Save()
    {
        _category.SaveToFile(false);
        _isChanged = false;
    }

    public bool ReloadIfChanged()
    {
        if (!_isChanged) return false;

        _isChanged = false;
        Load();
        return true;
    }

    [MemberNotNull(nameof(FirstPerson), nameof(ThirdPerson))]
    private void Load()
    {
        AllowAdjustments = Read("AllowKeyAdjustments", false);

        IsRiderHidden = Read("FirstPersonHideRider", false);
        IsImmersiveMode = Read("FirstPersonImmersiveMode", false);
        FirstPerson ??= new CameraViewSettings(_category, "FirstPerson",
            fieldOfView: 90, nearClipPlane: 0.05, forward: 0.08, height: -0.1, pitch: 30, stepMeters: 0.05);
        FirstPersonRotationSmoothing = (float) Read("FirstPersonRotationSmoothing", 10.0);
        FirstPersonHeadSmoothing = (float) Read("FirstPersonHeadSmoothing", 12.0);
        FirstPersonTiltSmoothing = (float) Read("FirstPersonTiltSmoothing", 10.0);
        FirstPersonRoll = (float) Read("FirstPersonRoll", 0.2);
        FirstPersonFlipCatchUp = (float) Read("FirstPersonFlipCatchUp", 1.5);

        ThirdPerson ??= new CameraViewSettings(_category, "ThirdPerson",
            fieldOfView: 80, nearClipPlane: 0.3, forward: -3.3, height: 4.3, pitch: -16, stepMeters: 0.1);
        ThirdPersonHeadingSmoothing = (float) Read("ThirdPersonHeadingSmoothing", 3.0);
        ThirdPersonVerticalSmoothing = (float) Read("ThirdPersonVerticalSmoothing", 6.0);

        FirstPersonKey = Read("FirstPersonKey", KeyCode.Alpha1);
        DefaultKey = Read("DefaultKey", KeyCode.Alpha2);
        ThirdPersonKey = Read("ThirdPersonKey", KeyCode.Alpha3);
        AdjustUpKey = Read("AdjustUpKey", KeyCode.UpArrow);
        AdjustDownKey = Read("AdjustDownKey", KeyCode.DownArrow);
        AdjustForwardKey = Read("AdjustForwardKey", KeyCode.LeftArrow);
        AdjustBackKey = Read("AdjustBackKey", KeyCode.RightArrow);
        AdjustPitchUpKey = Read("AdjustPitchUpKey", KeyCode.RightControl);
        AdjustPitchDownKey = Read("AdjustPitchDownKey", KeyCode.RightShift);
    }

    private T Read<T>(string name, T defaultValue) =>
        (_category.GetEntry<T>(name) ?? _category.CreateEntry(name, defaultValue)).Value;
}
