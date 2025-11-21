using LMD_Camera.Core;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(LMD_Camera.CameraMod), "LMD-Camera", "1.0.0", "flo-st")]
[assembly: MelonGame("Megagon Industries", "Lonely Mountains: Downhill")]

namespace LMD_Camera
{
    public class CameraMod : MelonMod
    {
        private static CameraMod instance = null!;
        private static GameObject? playCamera;
        private float levelTimer = 0f;
        private const float INIT_DELAY = 1f;

        public override void OnEarlyInitializeMelon() => instance = this;

        public override void OnInitializeMelon()
        {
            ConfigManager.Initialize();
            Log("Camera Mod Initialized");
        }

        public override void OnUpdate()
        {
            if (!IsInLevel()) return;
            
            if (levelTimer < INIT_DELAY)
            {
                levelTimer += Time.deltaTime;
                if (levelTimer >= INIT_DELAY)
                    CameraManager.Initialize();
            }
            else
            {
                if (IsHotkeyPressed()) return;
                CameraManager.UpdateCamera();
            }
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (playCamera == null)
                playCamera = GameObject.Find("PlayCamera(Clone)");

            if (IsInLevel())
                levelTimer = 0f;
            else
                CameraManager.Cleanup();
        }

        private static bool IsInLevel() => playCamera != null && playCamera.activeSelf;

        private static bool IsHotkeyPressed()
        {
            ViewMode? mode = null;
            
            if (Input.GetKeyDown(ConfigManager.FirstPersonKey)) 
                mode = ViewMode.FirstPerson;
            else if (Input.GetKeyDown(ConfigManager.ThirdPersonKey)) 
                mode = ViewMode.ThirdPerson;
            else if (Input.GetKeyDown(ConfigManager.DefaultKey)) 
                mode = ViewMode.Default;

            if (mode.HasValue)
            {
                if (mode.Value == ConfigManager.LastUsedViewMode) return false;

                ConfigManager.LastUsedViewMode = mode.Value;
                CameraManager.SetViewMode(mode.Value);
                return true;
            }
            return false;
        }

        public static void Log(string msg) => instance.LoggerInstance.Msg(msg);
        public static void Error(string msg) => instance.LoggerInstance.Error(msg);
    }

    public enum ViewMode { None, Default, ThirdPerson, FirstPerson }
}