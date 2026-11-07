using UnityEngine;

namespace LmdCamera.Core;

internal sealed class Interval
{
    private readonly float _seconds;
    private float _nextTime;

    public Interval(float seconds) => _seconds = seconds;

    public bool HasElapsed()
    {
        var now = Time.unscaledTime;
        if (now < _nextTime) return false;

        _nextTime = now + _seconds;
        return true;
    }
}
