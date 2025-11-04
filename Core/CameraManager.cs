using UnityEngine;
using Il2CppMegagon.Downhill.Cameras;

namespace LMD_Camera.Core
{
    public static class CameraManager
    {
        private static Camera? cam;
        private static Transform? camTransform, bike, headBone;
        private static PlayCamera? playCam;
        private static SkinnedMeshRenderer? bikeRenderer;
        private static readonly List<GameObject> hairObjects = new();
        private static ViewMode currentMode = ViewMode.None;

        public static void Initialize()
        {
            if (FindReferences())
                CameraMod.Log("Camera initialized");
        }

        public static void Cleanup()
        {
            if (playCam != null) playCam.enabled = true;
            if (bikeRenderer != null) bikeRenderer.enabled = true;
            ShowHairObjects();

            currentMode = ViewMode.None;
            camTransform = bike = headBone = null;
            cam = null;
            playCam = null;
            bikeRenderer = null;
            hairObjects.Clear();
        }

        public static void SetViewMode(ViewMode mode)
        {
            if (mode == currentMode || !EnsureReferences()) return;

            currentMode = mode;
            ViewSettings s = GetSettings(mode);
            
            cam!.fieldOfView = s.FieldOfView;
            cam.nearClipPlane = s.NearClipPlane;
            playCam!.enabled = (mode == ViewMode.Default);

            if (mode == ViewMode.FirstPerson)
                SetupFirstPersonView(s);
            else
                ResetBikeVisibility();

            CameraMod.Log($"View: {mode}");
        }

        public static void UpdateCamera()
        {
            if (!EnsureReferences()) return;

            if (currentMode == ViewMode.FirstPerson)
            {
                HandleAdjustments(ConfigManager.FirstPerson, true);
                UpdateFirstPerson();
            }
            else if (currentMode == ViewMode.ThirdPerson)
            {
                HandleAdjustments(ConfigManager.ThirdPerson, false);
                UpdateThirdPerson();
            }
        }

        private static bool EnsureReferences() => 
            (cam != null && bike != null) || FindReferences();

        private static bool FindReferences()
        {
            var camObj = GameObject.Find("PlayCamera(Clone)");
            var bikeObj = GameObject.Find("Bike(Clone)");
            
            if (camObj == null || bikeObj == null) return false;

            camTransform = camObj.transform;
            cam = camObj.GetComponent<Camera>();
            playCam = camObj.GetComponent<PlayCamera>();
            bike = bikeObj.transform;

            return cam != null && playCam != null;
        }

        private static void SetupFirstPersonView(ViewSettings s)
        {
            FindHeadBone();
            if (headBone == null) return;

            if (bikeRenderer != null) bikeRenderer.enabled = !s.HideBikeMesh;
            
            if (s.HideBikeMesh) HideHairObjects();
            else ShowHairObjects();
        }

        private static void ResetBikeVisibility()
        {
            if (bikeRenderer != null) bikeRenderer.enabled = true;
            ShowHairObjects();
        }

        private static void FindHeadBone()
        {
            if (bike == null || headBone != null) return;

            var bikeChild = bike.Find("bike")?.Find("mergedMesh");
            if (bikeChild == null) return;
            
            bikeRenderer = bikeChild.GetComponent<SkinnedMeshRenderer>();
            if (bikeRenderer == null) return;

            bikeRenderer.updateWhenOffscreen = true;
            foreach (var bone in bikeRenderer.bones)
            {
                if (bone.name == "head_tip_BindJNT")
                {
                    headBone = bone;
                    return;
                }
            }
        }

        private static void FindHairObjects()
        {
            if (hairObjects.Count > 0) return;
            
            foreach (var obj in GameObject.FindObjectsOfType<GameObject>())
            {
                if (obj.name.Contains("rider_hair") || obj.name.Contains("rider_facialhair"))
                    hairObjects.Add(obj);
            }
        }

        private static void HideHairObjects()
        {
            FindHairObjects();
            hairObjects.ForEach(obj => obj?.SetActive(false));
        }

        private static void ShowHairObjects() => 
            hairObjects.ForEach(obj => obj?.SetActive(true));

        private static void HandleAdjustments(ViewSettings s, bool isFirstPerson)
        {
            if (!ConfigManager.AllowKeyAdjustments) return;

            bool changed = false;
            float posIncrement = isFirstPerson ? 0.05f : 0.1f;
            float pitchIncrement = 2f;

            if (Input.GetKeyDown(ConfigManager.AdjustUpKey))
                { s.VerticalOffset += posIncrement; changed = true; }
            else if (Input.GetKeyDown(ConfigManager.AdjustDownKey))
                { s.VerticalOffset -= posIncrement; changed = true; }
            else if (Input.GetKeyDown(ConfigManager.AdjustLeftKey))
                { s.ForwardOffset += posIncrement; changed = true; }
            else if (Input.GetKeyDown(ConfigManager.AdjustRightKey))
                { s.ForwardOffset -= posIncrement; changed = true; }
            else if (Input.GetKeyDown(ConfigManager.AdjustPitchDownKey))
                { s.PitchOffset += pitchIncrement; changed = true; }
            else if (Input.GetKeyDown(ConfigManager.AdjustPitchUpKey))
                { s.PitchOffset -= pitchIncrement; changed = true; }

            if (changed)
            {
                ConfigManager.SaveSettings(isFirstPerson);
                CameraMod.Log($"Adjusted - V:{s.VerticalOffset:F2} F:{s.ForwardOffset:F2} P:{s.PitchOffset:F1}");
            }
        }

        private static void UpdateFirstPerson()
        {
            if (headBone == null || camTransform == null || bike == null) return;

            ViewSettings s = ConfigManager.FirstPerson;
            
            Vector3 targetPos = headBone.position + 
                bike.up * s.VerticalOffset + 
                bike.forward * s.ForwardOffset;
            
            camTransform.position = targetPos;

            Quaternion targetRot = bike.rotation * Quaternion.Euler(s.PitchOffset, 0f, 0f);
            
            camTransform.rotation = s.RotationSmoothing > 0 
                ? Quaternion.Slerp(camTransform.rotation, targetRot, Time.deltaTime * s.RotationSmoothing)
                : targetRot;
        }

        private static void UpdateThirdPerson()
        {
            if (bike == null || camTransform == null) return;

            ViewSettings s = ConfigManager.ThirdPerson;
            
            Vector3 targetPos = bike.position + 
                bike.up * s.VerticalOffset + 
                bike.forward * s.ForwardOffset;
            
            camTransform.position = s.PositionSmoothing > 0
                ? Vector3.Lerp(camTransform.position, targetPos, Time.deltaTime * s.PositionSmoothing)
                : targetPos;
            
            Quaternion targetRot = Quaternion.LookRotation(
                bike.position + bike.up * s.VerticalOffset - camTransform.position, 
                Vector3.up) * Quaternion.Euler(s.PitchOffset, 0f, 0f);
            
            camTransform.rotation = targetRot;
        }

        private static ViewSettings GetSettings(ViewMode mode) => mode switch
        {
            ViewMode.FirstPerson => ConfigManager.FirstPerson,
            ViewMode.ThirdPerson => ConfigManager.ThirdPerson,
            _ => ConfigManager.Default
        };
    }
}