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
        HideRider = Read("FP_HideRider", false);

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

        DefaultLens = new Lens(ReadNumber("Default_FOV", 38), ReadNumber("Default_NearClip", 0.3));

        FirstPerson = new CameraViewSettings(_category, "FP",
            fieldOfView: 95, nearClipPlane: 0.25, forward: 0.08, height: -0.1, pitch: 30);

        FirstPersonMotion = new FirstPersonMotion(
            RotationSmoothing: ReadNumber("FP_RotationSmoothing", 10),
            HeadSmoothing: ReadNumber("FP_HeadSmoothing", 12),
            Roll: ReadNumber("FP_Roll", 0.2));

        ThirdPerson = new CameraViewSettings(_category, "TP",
            fieldOfView: 70, nearClipPlane: 0.3, forward: -4.5, height: 3, pitch: 0);

        ThirdPersonMotion = new ThirdPersonMotion(
            HeadingSmoothing: ReadNumber("TP_HeadingSmoothing", 3),
            VerticalSmoothing: ReadNumber("TP_VerticalSmoothing", 6),
            LookHeight: ReadNumber("TP_LookHeight", 1));

        Save();
    }

    public bool AllowAdjustments { get; }
    public bool HideRider { get; }
    public ViewKeys ViewKeys { get; }
    public AdjustmentKeys AdjustmentKeys { get; }
    public Lens DefaultLens { get; }
    public CameraViewSettings FirstPerson { get; }
    public CameraViewSettings ThirdPerson { get; }
    public FirstPersonMotion FirstPersonMotion { get; }
    public ThirdPersonMotion ThirdPersonMotion { get; }

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

    private float ReadNumber(string name, double defaultValue) => (float)Read(name, defaultValue);
}
