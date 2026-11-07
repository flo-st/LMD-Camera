using UnityEngine;

namespace LmdCamera.Config;

internal sealed record ViewKeys(KeyCode FirstPerson, KeyCode Default, KeyCode ThirdPerson)
{
    public (KeyCode Key, ViewMode View)[] Bindings { get; } =
    {
        (FirstPerson, ViewMode.FirstPerson),
        (Default, ViewMode.Default),
        (ThirdPerson, ViewMode.ThirdPerson),
    };
}

internal sealed record AdjustmentKeys(KeyCode Up, KeyCode Down, KeyCode Left, KeyCode Right, KeyCode PitchUp, KeyCode PitchDown)
{
    public (KeyCode Key, Adjustment Change)[] Bindings(float positionStep, float pitchStep) => new[]
    {
        (Up, new Adjustment(Height: positionStep)),
        (Down, new Adjustment(Height: -positionStep)),
        (Left, new Adjustment(Forward: positionStep)),
        (Right, new Adjustment(Forward: -positionStep)),
        (PitchDown, new Adjustment(Pitch: pitchStep)),
        (PitchUp, new Adjustment(Pitch: -pitchStep)),
    };
}
