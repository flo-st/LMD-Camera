namespace LmdCamera.Config;

internal readonly record struct Lens(float FieldOfView, float NearClipPlane);

internal readonly record struct Adjustment(double Forward = 0, double Height = 0, double Pitch = 0);

internal readonly record struct FirstPersonMotion(float RotationSmoothing, float HeadSmoothing, float Roll);

internal readonly record struct ThirdPersonMotion(float HeadingSmoothing, float VerticalSmoothing, float LookHeight);
