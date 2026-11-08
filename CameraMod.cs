using LmdCamera;
using LmdCamera.Config;
using LmdCamera.Core;
using MelonLoader;

[assembly: MelonInfo(typeof(CameraMod), "LMD-Camera", "1.2.0", "flo-st")]
[assembly: MelonGame("Megagon Industries", "Lonely Mountains: Downhill")]

namespace LmdCamera;

public class CameraMod : MelonMod
{
    private CameraController? _controller;

    public override void OnInitializeMelon() =>
        _controller = new CameraController(new CameraSettings(), LoggerInstance);

    public override void OnUpdate() => _controller?.Update();

    public override void OnLateUpdate() => _controller?.LateUpdate();
}
