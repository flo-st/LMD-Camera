using UnityEngine;

namespace LmdCamera.Game;

internal sealed class Ragdoll
{
    private readonly Renderer[] _renderers;

    public Ragdoll(Transform head, Renderer[] renderers)
    {
        Head = head;
        _renderers = renderers;
    }

    public Transform Head { get; }
    public bool IsAlive => Head != null;

    public void SetVisible(bool visible)
    {
        foreach (var renderer in _renderers)
        {
            if (renderer != null) renderer.enabled = visible;
        }
    }
}
