// Assets/360/Editor/EditorScreenShotData.cs
// Data structures and enums for EditorScreenShot

using UnityEngine;
using System.Collections.Generic;
using FreecamPreview;
using EditorScreenShot.Runtime;

namespace EditorScreenShot
{
    public enum AspectPreset 
    { 
        UHD_4K_16_9, 
        FHD_16_9, 
        QHD_16_9, 
        HD_16_9, 
        Square_1_1, 
        Custom 
    }

    public class EditorScreenShotData
    {
        // Camera references
        public Camera camera;
        public Freecam freecam;
        public FisheyeImageEffect fisheye;

        // Resolution settings
        public AspectPreset preset = AspectPreset.FHD_16_9;
        public bool portrait = false;
        public int customW = 1920, customH = 1080;

        // Output settings
        public bool png = true;
        public int jpgQuality = 95;
        public string outDir = "";
        public bool pngKeepAlpha = true;
        public string lastSavedPath = null;
        public string fileNameTemplate = "{scene}_{yyyyMMdd_HHmmss}";
        public int rtMSAA = 1;

        // Language
        public Lang lang = Loc.GetSystemDefaultLanguage();

        // UI foldout states
        public bool fCameraSettings = true;
        public bool fOutputSettings = true;
        public bool fSceneView = true;

        // Scene View Safe Frame
        public bool showSafeFrame = false;
        public bool sfThirds = true;
        public bool sfDiagonals = false;
        public bool sfCenterCross = true;
        public float sfTitleSafe = 0.90f;
        public float sfLineWidth = 2f;
        public Color sfLineColor = new Color(1f, 1f, 1f, 0.9f);
        public float sfMaskAlpha = 0.35f;

        // Speed field snapshot
        public Dictionary<string, float> speedFieldSnapshot;

        // Scene Sync
        public bool sceneSyncOn = false;
        public ESSSceneSync sceneSync;

        // Scroll
        public Vector2 scroll;
    }
}
