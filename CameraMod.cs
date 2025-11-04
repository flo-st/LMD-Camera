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
        private bool isInLevel = false;
        private bool hasSeenMenu = false;
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
            if (!hasSeenMenu) return;
            if (IsHotkeyPressed()) return;

            if (isInLevel && levelTimer < INIT_DELAY)
            {
                levelTimer += Time.deltaTime;
                if (levelTimer >= INIT_DELAY)
                    CameraManager.SetViewMode(ConfigManager.LastUsedViewMode);
            }
        }

        public override void OnLateUpdate()
        {
            if (isInLevel && hasSeenMenu) 
                CameraManager.UpdateCamera();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName.Contains("SplashScreen")) return;
            
            bool wasInLevel = isInLevel;
            isInLevel = !string.IsNullOrEmpty(sceneName) && !sceneName.Contains("Menu");

            if (!hasSeenMenu && sceneName.Contains("Menu"))
            {
                hasSeenMenu = true;
                Log("Game ready");
            }

            if (isInLevel && !wasInLevel && hasSeenMenu)
            {
                levelTimer = 0f;
                CameraManager.Initialize();
            }
            else if (!isInLevel && wasInLevel)
            {
                CameraManager.Cleanup();
            }
        }

        private bool IsHotkeyPressed()
        {
            if (!isInLevel) return false;

            ViewMode? mode = null;
            if (Input.GetKeyDown(ConfigManager.FirstPersonKey)) mode = ViewMode.FirstPerson;
            else if (Input.GetKeyDown(ConfigManager.ThirdPersonKey)) mode = ViewMode.ThirdPerson;
            else if (Input.GetKeyDown(ConfigManager.DefaultKey)) mode = ViewMode.Default;

            if (mode.HasValue)
            {
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