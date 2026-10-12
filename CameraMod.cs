using LMD_Camera;
using LMD_Camera.Config;
using LMD_Camera.Core;
using MelonLoader;

[assembly: MelonInfo(typeof(CameraMod), "LMD-Camera", "1.1.0", "flo-st")]
[assembly: MelonGame("Megagon Industries", "Lonely Mountains: Downhill")]

namespace LMD_Camera;

public class CameraMod : MelonMod
{
    private CameraInputHandler? _cameraInputHandler;

    public override void OnInitializeMelon() =>
        _cameraInputHandler = new CameraInputHandler(new CameraSettings(), LoggerInstance);

    public override void OnUpdate() => _cameraInputHandler?.Update();

    public override void OnLateUpdate() => _cameraInputHandler?.LateUpdate();
}