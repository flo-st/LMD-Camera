using LmdCamera.Game;

namespace LmdCamera.Core;

internal interface ICameraFollower
{
    void Reset(RiderRig rig);

    void Follow(RiderRig rig);
}
