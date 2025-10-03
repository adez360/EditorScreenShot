// Assets/360/Editor/EditorScreenShotOutput.cs
// Output settings and screenshot functionality for EditorScreenShot

using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using FreecamPreview;

namespace EditorScreenShot
{
    public static class EditorScreenShotOutput
    {
        // Get preset width and height
        public static (int, int) GetPresetWH(AspectPreset p, int customW, int customH) => p switch
        {
            AspectPreset.UHD_4K_16_9 => (3840, 2160),
            AspectPreset.QHD_16_9    => (2560, 1440),
            AspectPreset.FHD_16_9    => (1920, 1080),
            AspectPreset.HD_16_9     => (1280, 720),
            AspectPreset.Square_1_1  => (1024, 1024),
            _                        => (customW, customH),
        };

        // Get current width and height
        public static (int, int) GetCurrentWH(EditorScreenShotData data)
        {
            var (w, h) = GetPresetWH(data.preset, data.customW, data.customH);
            if (data.preset == AspectPreset.Custom) 
            { 
                w = data.customW; 
                h = data.customH; 
            }
            if (data.portrait) 
                (w, h) = (h, w);
            return (w, h);
        }

        // Save current frame
        public static void SaveCurrentFrame(EditorScreenShotData data, int w, int h)
        {
            if (!data.camera) return;

            int maxTex = SystemInfo.maxTextureSize;
            if (w > maxTex || h > maxTex)
            {
                EditorUtility.DisplayDialog("Screenshot", $"Size {w}x{h} exceeds max texture size ({maxTex}).", "OK");
                return;
            }

            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            rt.antiAliasing = Mathf.ClosestPowerOfTwo(Mathf.Clamp(data.rtMSAA, 1, 8));

            var prevActive = RenderTexture.active;
            var prevTarget = data.camera.targetTexture;
            var prevFlags = data.camera.clearFlags;
            var prevBG = data.camera.backgroundColor;
            var prevSky = RenderSettings.skybox;

            try
            {
                if (data.png && data.pngKeepAlpha)
                {
                    data.camera.clearFlags = CameraClearFlags.SolidColor;
                    var c = prevBG; c.a = 0f; // transparent background
                    data.camera.backgroundColor = c;
                    RenderSettings.skybox = null;
                }

                data.camera.targetTexture = rt;
                RenderTexture.active = rt;
                data.camera.Render();

                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply();

                Directory.CreateDirectory(data.outDir);
                string ext = data.png ? "png" : "jpg";
                string name = BuildFileName(data.fileNameTemplate, w, h, data.camera);
                string path = Path.Combine(data.outDir, name + "." + ext);
                File.WriteAllBytes(path, data.png ? tex.EncodeToPNG() : tex.EncodeToJPG(Mathf.Clamp(data.jpgQuality, 1, 100)));
                data.lastSavedPath = path;

                EditorUtility.RevealInFinder(path);

                UnityEngine.Object.DestroyImmediate(tex);
            }
            finally
            {
                data.camera.targetTexture = prevTarget;
                RenderTexture.active = prevActive;
                data.camera.clearFlags = prevFlags;
                data.camera.backgroundColor = prevBG;
                RenderSettings.skybox = prevSky;
                rt.Release(); 
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }

        // Build file name
        public static string BuildFileName(string tpl, int w, int h, Camera camera)
        {
            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string cam = camera ? camera.name : "Cam";
            string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string s = tpl.Replace("{scene}", scene).Replace("{camera}", cam)
                          .Replace("{w}", w.ToString()).Replace("{h}", h.ToString())
                          .Replace("{wxh}", $"{w}x{h}").Replace("{yyyyMMdd_HHmmss}", ts);
            foreach (char c in Path.GetInvalidFileNameChars()) s = s.Replace(c.ToString(), "_");
            return s;
        }

        // Global capture method
        public static void CaptureGlobal(EditorScreenShotData data)
        {
            if (data == null) return;
            
            EditorApplication.delayCall += () =>
            {
                if (data == null) return;
                
                // Ensure using dedicated camera
                EditorScreenShotCamera.SyncCameraRefs(data, true);
                var cam = data.camera;
                
                if (cam == null)
                {
                    EditorUtility.DisplayDialog("Screenshot", Loc.T("NoCamera") + ", " + "Please create camera first.", Loc.T("OK"));
                    return;
                }
                
                var (outW, outH) = GetCurrentWH(data);
                SaveCurrentFrame(data, outW, outH);
            };
        }

    }
}
