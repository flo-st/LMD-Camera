using MelonLoader;
using UnityEngine;

namespace LMD_Camera.Core
{
    public static class ConfigManager
    {
        public static KeyCode FirstPersonKey { get; private set; }
        public static KeyCode ThirdPersonKey { get; private set; }
        public static KeyCode DefaultKey { get; private set; }
        public static bool AllowKeyAdjustments { get; private set; }
        
        public static KeyCode AdjustUpKey { get; private set; }
        public static KeyCode AdjustDownKey { get; private set; }
        public static KeyCode AdjustLeftKey { get; private set; }
        public static KeyCode AdjustRightKey { get; private set; }
        public static KeyCode AdjustPitchUpKey { get; private set; }
        public static KeyCode AdjustPitchDownKey { get; private set; }
        
        public static ViewSettings FirstPerson { get; private set; } = new();
        public static ViewSettings ThirdPerson { get; private set; } = new();
        public static ViewSettings Default { get; private set; } = new();

        public static ViewMode LastUsedViewMode
        {
            get => lastViewMode.Value;
            set { lastViewMode.Value = value; category.SaveToFile(); }
        }

        private static MelonPreferences_Category category = null!;
        private static MelonPreferences_Entry<ViewMode> lastViewMode = null!;
        private static MelonPreferences_Entry<float> fpHeight = null!, fpForward = null!, fpPitch = null!;
        private static MelonPreferences_Entry<float> tpForward = null!, tpHeight = null!, tpPitch = null!;

        public static void Initialize()
        {
            category = MelonPreferences.CreateCategory("LMD-Camera");
            category.SetFilePath("UserData/LMD-Camera.cfg");

            lastViewMode = category.CreateEntry("LastViewMode", ViewMode.Default);
            AllowKeyAdjustments = category.CreateEntry("AllowKeyAdjustments", false).Value;

            LoadKeybindings();
            LoadFirstPersonSettings();
            LoadThirdPersonSettings();
            LoadDefaultSettings();
            
            category.SaveToFile();
            CameraMod.Log("Config loaded");
        }

        private static void LoadKeybindings()
        {
            FirstPersonKey = category.CreateEntry("Key_FirstPerson", KeyCode.Alpha1).Value;
            DefaultKey = category.CreateEntry("Key_Default", KeyCode.Alpha2).Value;
            ThirdPersonKey = category.CreateEntry("Key_ThirdPerson", KeyCode.Alpha3).Value;
            AdjustUpKey = category.CreateEntry("Key_AdjustUp", KeyCode.UpArrow).Value;
            AdjustDownKey = category.CreateEntry("Key_AdjustDown", KeyCode.DownArrow).Value;
            AdjustLeftKey = category.CreateEntry("Key_AdjustLeft", KeyCode.LeftArrow).Value;
            AdjustRightKey = category.CreateEntry("Key_AdjustRight", KeyCode.RightArrow).Value;
            AdjustPitchUpKey = category.CreateEntry("Key_AdjustPitchUp", KeyCode.RightControl).Value;
            AdjustPitchDownKey = category.CreateEntry("Key_AdjustPitchDown", KeyCode.RightShift).Value;
        }

        private static void LoadFirstPersonSettings()
        {
            fpHeight = category.CreateEntry("FP_Height", -0.2f);
            fpForward = category.CreateEntry("FP_Forward", 0f);
            fpPitch = category.CreateEntry("FP_Pitch", 25f);
            
            FirstPerson.FieldOfView = category.CreateEntry("FP_FOV", 95f).Value;
            FirstPerson.VerticalOffset = fpHeight.Value;
            FirstPerson.ForwardOffset = fpForward.Value;
            FirstPerson.PitchOffset = fpPitch.Value;
            FirstPerson.NearClipPlane = category.CreateEntry("FP_NearClip", 0.22f).Value;
            FirstPerson.RotationSmoothing = category.CreateEntry("FP_RotationSmoothing", 10f).Value;
            FirstPerson.HideBikeMesh = category.CreateEntry("FP_HideBike", false).Value;
        }

        private static void LoadThirdPersonSettings()
        {
            tpForward = category.CreateEntry("TP_Forward", -4f);
            tpHeight = category.CreateEntry("TP_Height", 4f);
            tpPitch = category.CreateEntry("TP_Pitch", 10f);
            
            ThirdPerson.FieldOfView = category.CreateEntry("TP_FOV", 70f).Value;
            ThirdPerson.ForwardOffset = tpForward.Value;
            ThirdPerson.VerticalOffset = tpHeight.Value;
            ThirdPerson.PitchOffset = tpPitch.Value;
            ThirdPerson.NearClipPlane = category.CreateEntry("TP_NearClip", 0.3f).Value;
            ThirdPerson.PositionSmoothing = category.CreateEntry("TP_PositionSmoothing", 12f).Value;
        }

        private static void LoadDefaultSettings()
        {
            Default.FieldOfView = category.CreateEntry("Default_FOV", 38f).Value;
            Default.NearClipPlane = category.CreateEntry("Default_NearClip", 0.3f).Value;
        }

        public static void SaveSettings(bool isFirstPerson)
        {
            if (isFirstPerson)
            {
                fpHeight.Value = FirstPerson.VerticalOffset;
                fpForward.Value = FirstPerson.ForwardOffset;
                fpPitch.Value = FirstPerson.PitchOffset;
            }
            else
            {
                tpForward.Value = ThirdPerson.ForwardOffset;
                tpHeight.Value = ThirdPerson.VerticalOffset;
                tpPitch.Value = ThirdPerson.PitchOffset;
            }
            category.SaveToFile();
        }
    }

    public class ViewSettings
    {
        public float FieldOfView { get; set; }
        public float ForwardOffset { get; set; }
        public float VerticalOffset { get; set; }
        public float PitchOffset { get; set; }
        public float NearClipPlane { get; set; }
        public float RotationSmoothing { get; set; }
        public float PositionSmoothing { get; set; }
        public bool HideBikeMesh { get; set; }
    }
}