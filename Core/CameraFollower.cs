using LmdCamera.Config;
using LmdCamera.Game;

namespace LmdCamera.Core;

internal abstract class CameraFollower
{
    protected CameraFollower(CameraViewSettings view)
    {
        View = view;
    }

    public CameraViewSettings View { get; }

    public abstract void Reset(RiderRig rig);

    public abstract void Follow(RiderRig rig, float deltaTime);

    protected static float GetSmoothingFactor(float rate, float deltaTime) =>
        rate > 0f ? 1f - MathF.Exp(-rate * deltaTime) : 1f;
}
