using MelonLoader;
using UnityEngine;

namespace LmdCamera.Config;

internal sealed class CameraSettings
{
    private readonly MelonPreferences_Category _category = MelonPreferences.CreateCategory("LMD-Camera");
    private readonly MelonPreferences_Entry<ViewMode> _lastView;

    public CameraSettings()
    {
        _category.SetFilePath("UserData/LMD-Camera.cfg");

        _lastView = _category.CreateEntry("LastViewMode", ViewMode.Default);
        AllowAdjustments = Read("AllowKeyAdjustments", false);
        HideBike = Read("FP_HideBike", false);

        ViewKeys = new ViewKeys(
            FirstPerson: Read("Key_FirstPerson", KeyCode.Alpha1),
            Default: Read("Key_Default", KeyCode.Alpha2),
            ThirdPerson: Read("Key_ThirdPerson", KeyCode.Alpha3));

        AdjustmentKeys = new AdjustmentKeys(
            Up: Read("Key_AdjustUp", KeyCode.UpArrow),
            Down: Read("Key_AdjustDown", KeyCode.DownArrow),
            Left: Read("Key_AdjustLeft", KeyCode.LeftArrow),
            Right: Read("Key_AdjustRight", KeyCode.RightArrow),
            PitchUp: Read("Key_AdjustPitchUp", KeyCode.RightControl),
            PitchDown: Read("Key_AdjustPitchDown", KeyCode.RightShift));

        DefaultLens = new Lens(Read("Default_FOV", 38f), Read("Default_NearClip", 0.3f));

        FirstPerson = new CameraViewSettings(_category, "FP", "RotationSmoothing",
            fieldOfView: 95f, nearClipPlane: 0.22f, forward: 0f, height: -0.2f, pitch: 25f, smoothing: 10f);

        ThirdPerson = new CameraViewSettings(_category, "TP", "PositionSmoothing",
            fieldOfView: 70f, nearClipPlane: 0.3f, forward: -4f, height: 4f, pitch: 10f, smoothing: 7f);

        Save();
    }

    public bool AllowAdjustments { get; }
    public bool HideBike { get; }
    public ViewKeys ViewKeys { get; }
    public AdjustmentKeys AdjustmentKeys { get; }
    public Lens DefaultLens { get; }
    public CameraViewSettings FirstPerson { get; }
    public CameraViewSettings ThirdPerson { get; }

    public ViewMode LastView
    {
        get => _lastView.Value is ViewMode.None ? ViewMode.Default : _lastView.Value;
        set
        {
            _lastView.Value = value;
            Save();
        }
    }

    public Lens GetLens(ViewMode view) => view switch
    {
        ViewMode.FirstPerson => FirstPerson.Lens,
        ViewMode.ThirdPerson => ThirdPerson.Lens,
        _ => DefaultLens,
    };

    public void Save() => _category.SaveToFile(false);

    private T Read<T>(string name, T defaultValue) => _category.CreateEntry(name, defaultValue).Value;
}
