using MelonLoader;

namespace LmdCamera.Config;

internal sealed class CameraViewSettings
{
    private const double PitchStepDegrees = 2;

    private readonly double _stepMeters;
    private readonly MelonPreferences_Entry<double> _fieldOfView;
    private readonly MelonPreferences_Entry<double> _nearClipPlane;
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
        double pitch,
        double stepMeters)
    {
        _stepMeters = stepMeters;
        _fieldOfView = category.CreateEntry($"{prefix}FOV", fieldOfView);
        _nearClipPlane = category.CreateEntry($"{prefix}NearClip", nearClipPlane);
        _forward = category.CreateEntry($"{prefix}Forward", forward);
        _height = category.CreateEntry($"{prefix}Height", height);
        _pitch = category.CreateEntry($"{prefix}Pitch", pitch);
    }

    public float FieldOfView => (float) _fieldOfView.Value;
    public float NearClipPlane => (float) _nearClipPlane.Value;
    public float Forward => (float) _forward.Value;
    public float Height => (float) _height.Value;
    public float Pitch => (float) _pitch.Value;

    public void Adjust(int forwardSteps, int heightSteps, int pitchSteps)
    {
        _forward.Value = Math.Round(_forward.Value + forwardSteps * _stepMeters, 2);
        _height.Value = Math.Round(_height.Value + heightSteps * _stepMeters, 2);
        _pitch.Value = Math.Round(_pitch.Value + pitchSteps * PitchStepDegrees, 2);
    }
}
