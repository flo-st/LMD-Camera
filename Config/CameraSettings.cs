using MelonLoader;
using UnityEngine;

namespace LMD_Camera.Config;

internal class CameraSettings
{
    private readonly MelonPreferences_Category _category = MelonPreferences.CreateCategory("LMD-Camera");
    private readonly MelonPreferences_Entry<ViewMode> _lastView;

    public CameraSettings()
    {
        _category.SetFilePath("UserData/LMD-Camera.cfg");

        _lastView = _category.CreateEntry("LastViewMode", ViewMode.Default);
        AllowAdjustments = ReadPreference("AllowKeyAdjustments", false);
        HideBike = ReadPreference("FP_HideBike", false);

        ViewKeys = new ViewKeys(
            FirstPerson: ReadPreference("Key_FirstPerson", KeyCode.Alpha1),
            Default: ReadPreference("Key_Default", KeyCode.Alpha2),
            ThirdPerson: ReadPreference("Key_ThirdPerson", KeyCode.Alpha3));

        AdjustmentKeys = new AdjustmentKeys(
            Up: ReadPreference("Key_AdjustUp", KeyCode.UpArrow),
            Down: ReadPreference("Key_AdjustDown", KeyCode.DownArrow),
            Left: ReadPreference("Key_AdjustLeft", KeyCode.LeftArrow),
            Right: ReadPreference("Key_AdjustRight", KeyCode.RightArrow),
            PitchUp: ReadPreference("Key_AdjustPitchUp", KeyCode.RightControl),
            PitchDown: ReadPreference("Key_AdjustPitchDown", KeyCode.RightShift));

        DefaultLens = new Lens(ReadPreference("Default_FOV", 38f), ReadPreference("Default_NearClip", 0.3f));

        FirstPerson = new CameraViewSettings(_category, "FP", "RotationSmoothing",
            fieldOfView: 95f, nearClipPlane: 0.22f, forward: 0f, height: -0.2f, pitch: 25f, smoothing: 10f);

        ThirdPerson = new CameraViewSettings(_category, "TP", "PositionSmoothing",
            fieldOfView: 70f, nearClipPlane: 0.3f, forward: -4f, height: 4f, pitch: 10f, smoothing: 12f);

        SavePreferences();
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
            SavePreferences();
        }
    }

    public Lens GetLensForView(ViewMode mode) => mode switch
    {
        ViewMode.FirstPerson => FirstPerson.Lens,
        ViewMode.ThirdPerson => ThirdPerson.Lens,
        _ => DefaultLens,
    };

    public void SavePreferences() => _category.SaveToFile(false);

    private T ReadPreference<T>(string name, T defaultValue) => _category.CreateEntry(name, defaultValue).Value;
}
