using UnityEngine;

namespace LmdCamera.Core;

internal static class Smoothing
{
    public static float GetFactor(float rate) =>
        rate > 0f ? 1f - Mathf.Exp(-rate * Time.deltaTime) : 1f;
}
