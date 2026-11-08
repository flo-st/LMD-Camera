using UnityEngine;

namespace LmdCamera.Core;

internal static class Damping
{
    public static float Factor(float rate) =>
        rate > 0f ? 1f - Mathf.Exp(-rate * Time.deltaTime) : 1f;
}
