using MelonLoader;

namespace LmdCamera.Config;

internal readonly record struct Lens(float FieldOfView, float NearClipPlane);

internal readonly record struct Adjustment(float Forward = 0f, float Height = 0f, float Pitch = 0f);

internal sealed class CameraViewSettings
{
    private readonly MelonPreferences_Entry<float> _forward;
    private readonly MelonPreferences_Entry<float> _height;
    private readonly MelonPreferences_Entry<float> _pitch;

    public CameraViewSettings(
        MelonPreferences_Category category,
        string prefix,
        string smoothingKey,
        float fieldOfView,
        float nearClipPlane,
        float forward,
        float height,
        float pitch,
        float smoothing)
    {
        Lens = new Lens(
            category.CreateEntry($"{prefix}_FOV", fieldOfView).Value,
            category.CreateEntry($"{prefix}_NearClip", nearClipPlane).Value);

        Smoothing = category.CreateEntry($"{prefix}_{smoothingKey}", smoothing).Value;

        _forward = category.CreateEntry($"{prefix}_Forward", forward);
        _height = category.CreateEntry($"{prefix}_Height", height);
        _pitch = category.CreateEntry($"{prefix}_Pitch", pitch);
    }

    public Lens Lens { get; }
    public float Smoothing { get; }
    public float Forward => _forward.Value;
    public float Height => _height.Value;
    public float Pitch => _pitch.Value;

    public void Adjust(Adjustment adjustment)
    {
        _forward.Value += adjustment.Forward;
        _height.Value += adjustment.Height;
        _pitch.Value += adjustment.Pitch;
    }
}
