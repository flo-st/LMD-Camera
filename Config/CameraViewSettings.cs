using MelonLoader;

namespace LmdCamera.Config;

internal sealed class CameraViewSettings
{
    private readonly MelonPreferences_Entry<double> _forward;
    private readonly MelonPreferences_Entry<double> _height;
    private readonly MelonPreferences_Entry<double> _pitch;

    public CameraViewSettings(
        MelonPreferences_Category category,
        string prefix,
        double fieldOfView,
        double nearClipPlane,
        double forward,
        double height,
        double pitch)
    {
        Lens = new Lens(
            (float)category.CreateEntry($"{prefix}_FOV", fieldOfView).Value,
            (float)category.CreateEntry($"{prefix}_NearClip", nearClipPlane).Value);

        _forward = category.CreateEntry($"{prefix}_Forward", forward);
        _height = category.CreateEntry($"{prefix}_Height", height);
        _pitch = category.CreateEntry($"{prefix}_Pitch", pitch);
    }

    public Lens Lens { get; }
    public float Forward => (float)_forward.Value;
    public float Height => (float)_height.Value;
    public float Pitch => (float)_pitch.Value;

    public void Adjust(Adjustment adjustment)
    {
        _forward.Value = Round(_forward.Value + adjustment.Forward);
        _height.Value = Round(_height.Value + adjustment.Height);
        _pitch.Value = Round(_pitch.Value + adjustment.Pitch);
    }

    private static double Round(double value) => Math.Round(value, 2);
}
