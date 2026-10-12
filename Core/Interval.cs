using UnityEngine;

namespace LMD_Camera.Core;

internal class Interval
{
    private readonly float _seconds;
    private float _nextTime;

    public Interval(float seconds) => _seconds = seconds;

    public bool HasElapsed()
    {
        if (Time.unscaledTime < _nextTime) return false;

        _nextTime = Time.unscaledTime + _seconds;
        return true;
    }
}
