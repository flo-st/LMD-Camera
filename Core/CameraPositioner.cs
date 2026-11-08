using LmdCamera.Config;
using LmdCamera.Game;

namespace LmdCamera.Core;

internal sealed class CameraPositioner
{
    private readonly CameraSettings _settings;
    private readonly FirstPersonFollower _firstPerson;
    private readonly ThirdPersonFollower _thirdPerson;

    private ICameraFollower? _follower;
    private bool _hideRider;

    public CameraPositioner(CameraSettings settings)
    {
        _settings = settings;
        _firstPerson = new FirstPersonFollower(settings);
        _thirdPerson = new ThirdPersonFollower(settings);
    }

    public void Apply(ViewMode view, RiderRig rig)
    {
        var lens = _settings.GetLens(view);
        rig.Camera.fieldOfView = lens.FieldOfView;
        rig.Camera.nearClipPlane = lens.NearClipPlane;
        rig.PlayCamera.enabled = view == ViewMode.Default;
        _hideRider = view == ViewMode.FirstPerson && _settings.HideRider;
        rig.SetRiderVisible(!_hideRider);

        _follower = view switch
        {
            ViewMode.FirstPerson => _firstPerson,
            ViewMode.ThirdPerson => _thirdPerson,
            _ => null,
        };

        _follower?.Reset(rig);
    }

    public void Follow(RiderRig rig)
    {
        _follower?.Follow(rig);

        if (_hideRider) rig.SetRiderVisible(false);
    }
}
