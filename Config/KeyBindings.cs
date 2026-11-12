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

internal sealed record AdjustmentKeys(KeyCode Up, KeyCode Down, KeyCode Forward, KeyCode Back, KeyCode PitchUp, KeyCode PitchDown)
{
    public (KeyCode Key, Adjustment Change)[] CreateBindings(double positionStep, double pitchStep) => new[]
    {
        (Up, new Adjustment(Height: positionStep)),
        (Down, new Adjustment(Height: -positionStep)),
        (Forward, new Adjustment(Forward: positionStep)),
        (Back, new Adjustment(Forward: -positionStep)),
        (PitchDown, new Adjustment(Pitch: pitchStep)),
        (PitchUp, new Adjustment(Pitch: -pitchStep)),
    };
}
