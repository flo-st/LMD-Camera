using UnityEngine;

namespace LmdCamera.Config;

internal readonly record struct Lens(float FieldOfView, float NearClipPlane);

internal sealed record ViewKeys(KeyCode FirstPerson, KeyCode Default, KeyCode ThirdPerson);

internal sealed record AdjustmentKeys(
    KeyCode Up,
    KeyCode Down,
    KeyCode Forward,
    KeyCode Back,
    KeyCode PitchUp,
    KeyCode PitchDown);

internal readonly record struct FirstPersonMotion(
    float RotationSmoothing,
    float HeadSmoothing,
    float TiltSmoothing,
    float Roll,
    float FlipCatchUp);

internal readonly record struct ThirdPersonMotion(
    float HeadingSmoothing,
    float VerticalSmoothing);
