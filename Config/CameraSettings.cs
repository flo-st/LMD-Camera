using MelonLoader;
using UnityEngine;

namespace LmdCamera.Config;

internal sealed class CameraSettings
{
    private const string FilePath = "UserData/LMD-Camera.cfg";

    private readonly MelonPreferences_Category _category = MelonPreferences.CreateCategory("LMD-Camera");
    private readonly MelonPreferences_Category _keys = MelonPreferences.CreateCategory("LMD-Camera-Keys");
    private readonly MelonPreferences_Entry<ViewMode> _lastView;

    public CameraSettings()
    {
        _category.SetFilePath(FilePath);
        _keys.SetFilePath(FilePath);

        _lastView = _category.CreateEntry("LastViewMode", ViewMode.Default);
        AllowAdjustments = Read("AllowKeyAdjustments", false);
        IsRiderHidden = Read("FP_HideRider", false);
        IsImmersiveMode = Read("FP_ImmersiveMode", false);

        ViewKeys = new ViewKeys(
            FirstPerson: ReadKey("FirstPerson", KeyCode.Alpha1),
            Default: ReadKey("Default", KeyCode.Alpha2),
            ThirdPerson: ReadKey("ThirdPerson", KeyCode.Alpha3));

        AdjustmentKeys = new AdjustmentKeys(
            Up: ReadKey("AdjustUp", KeyCode.UpArrow),
            Down: ReadKey("AdjustDown", KeyCode.DownArrow),
            Forward: ReadKey("AdjustForward", KeyCode.LeftArrow),
            Back: ReadKey("AdjustBack", KeyCode.RightArrow),
            PitchUp: ReadKey("AdjustPitchUp", KeyCode.RightControl),
            PitchDown: ReadKey("AdjustPitchDown", KeyCode.RightShift));

        FirstPerson = new CameraViewSettings(_category, "FP",
            fieldOfView: 90, nearClipPlane: 0.05, forward: 0.08, height: -0.1, pitch: 30);

        FirstPersonMotion = new FirstPersonMotion(
            RotationSmoothing: ReadNumber("FP_RotationSmoothing", 10),
            HeadSmoothing: ReadNumber("FP_HeadSmoothing", 12),
            TiltSmoothing: ReadNumber("FP_TiltSmoothing", 10),
            Roll: ReadNumber("FP_Roll", 0.2),
            FlipCatchUp: ReadNumber("FP_FlipCatchUp", 1.5));

        ThirdPerson = new CameraViewSettings(_category, "TP",
            fieldOfView: 80, nearClipPlane: 0.3, forward: -3.3, height: 4.3, pitch: -16);

        ThirdPersonMotion = new ThirdPersonMotion(
            HeadingSmoothing: ReadNumber("TP_HeadingSmoothing", 3),
            VerticalSmoothing: ReadNumber("TP_VerticalSmoothing", 6));

        _category.SaveToFile(false);
        _keys.SaveToFile(false);
    }

    public bool AllowAdjustments { get; }
    public bool IsRiderHidden { get; }
    public bool IsImmersiveMode { get; }
    public ViewKeys ViewKeys { get; }
    public AdjustmentKeys AdjustmentKeys { get; }
    public CameraViewSettings FirstPerson { get; }
    public CameraViewSettings ThirdPerson { get; }
    public FirstPersonMotion FirstPersonMotion { get; }
    public ThirdPersonMotion ThirdPersonMotion { get; }

    public ViewMode LastView
    {
        get => _lastView.Value;
        set
        {
            _lastView.Value = value;
            Save();
        }
    }

    public void Save() => _category.SaveToFile(false);

    private KeyCode ReadKey(string name, KeyCode defaultValue) => _keys.CreateEntry(name, defaultValue).Value;

    private T Read<T>(string name, T defaultValue) => _category.CreateEntry(name, defaultValue).Value;

    private float ReadNumber(string name, double defaultValue) => (float) Read(name, defaultValue);
}
