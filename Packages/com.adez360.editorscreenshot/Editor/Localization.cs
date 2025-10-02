// Localization.cs
// Simple 3-language dictionary (EN/ZH/JA). Avoids tuple/target-typed new for Unity C# compatibility.

using System.Collections.Generic;
using System.Globalization;

namespace FreecamPreview
{
    public enum Lang { English, Chinese, ChineseSimplified, Japanese }

    public static class Loc
    {
        static Lang _lang = Lang.Chinese;
        public static Lang Language { get => _lang; set => _lang = value; }

        // key -> [en, zh, zh-cn, ja]
        static readonly Dictionary<string, string[]> dict = new Dictionary<string, string[]>
        {
            // Window / Toolbar
            { "PanelTitle", new[] { "EditorScreenShot", "EditorScreenShot", "EditorScreenShot", "EditorScreenShot" } },
            { "Lang",       new[] { "Language", "語言", "语言", "言語" } },

            // Big actions / Quick row
            { "CaptureImage",   new[] { "Capture Image", "拍攝截圖", "拍摄截图", "キャプチャ" } },

            // Groups
            { "CameraSettings",  new[] { "Camera Settings", "相機設定", "相机设置", "カメラ設定" } },
            { "OutputSettings",  new[] { "Output Settings", "輸出設定", "输出设置", "出力設定" } },
            { "SceneViewSection",new[] { "Scene View", "Scene View", "场景视图", "シーンビュー" } },

            // Scene Sync
            { "SceneSyncOn",     new[] { "Scene Sync: ON", "場景同步：開", "场景同步：开", "シーン同期：ON" } },
            { "SceneSyncOff",    new[] { "Scene Sync: OFF", "場景同步：關", "场景同步：关", "シーン同期：OFF" } },

            // Common fields
            { "Camera",        new[] { "Camera", "相機", "相机", "カメラ" } },
            { "Browse",        new[] { "Browse", "瀏覽", "浏览", "参照" } },
            { "Resolution",    new[] { "Resolution", "解析度", "分辨率", "解像度" } },
            { "Portrait",      new[] { "Portrait", "直式", "竖屏", "縦向き" } },
            { "Format",        new[] { "Format", "格式", "格式", "フォーマット" } },
            { "SaveLocation",  new[] { "Save Location", "儲存位置", "保存位置", "保存先" } },

            // Short labels
            { "WidthShort",  new[] { "W", "W", "W", "W" } },
            { "HeightShort", new[] { "H", "H", "H", "H" } },

            // Optics / Distortion
            { "FOV",      new[] { "FOV", "視角", "视角", "視野角" } },
            { "Focal",    new[] { "Focal Length (mm)", "焦距 (mm)", "焦距 (mm)", "焦点距離 (mm)" } },
            { "Fisheye",  new[] { "Fisheye", "魚眼", "鱼眼", "フィッシュアイ" } },

            // Scene View overlay
            { "ShowSafeFrame", new[] { "Show Safe Frame", "顯示參考線", "显示参考线", "セーフフレーム表示" } },
            { "SFThirds",      new[] { "Thirds Grid", "三分線", "三分线", "三分割グリッド" } },
            { "SFDiagonals",   new[] { "Diagonals", "對角線", "对角线", "対角線" } },
            { "SFCenter",      new[] { "Center Cross", "中心十字", "中心十字", "センタークロス" } },
            { "TitleSafe",     new[] { "Title Safe %", "Title Safe %", "标题安全区 %", "タイトルセーフ %" } },
            { "LineWidth",     new[] { "Line Width", "線寬", "线宽", "線幅" } },
            { "LineColor",     new[] { "Line Color", "線色", "线色", "線色" } },
            { "MaskOpacity",   new[] { "Mask Opacity", "遮罩透明度", "遮罩透明度", "マスク不透明度" } },
            { "SafeFrameTip",  new[] { "Try to match the window ratio to the reference frame for better preview accuracy.", "盡可能地讓視窗比例符合參考線邊框，截圖成果會更接近預覽效果。", "尽可能让窗口比例符合参考线边框，截图成果会更接近预览效果。", "ウィンドウの比率をリファレンスフレームに合わせることで、プレビュー精度が向上します。" } },

            // Aspect names
            { "UHD_4K_16_9", new[] { "UHD 4K 16:9", "UHD 4K 16:9", "UHD 4K 16:9", "UHD 4K 16:9" } },
            { "FHD_16_9",    new[] { "FHD 16:9", "FHD 16:9", "FHD 16:9", "FHD 16:9" } },
            { "QHD_16_9",    new[] { "QHD 16:9", "QHD 16:9", "QHD 16:9", "QHD 16:9" } },
            { "HD_16_9",     new[] { "HD 16:9", "HD 16:9", "HD 16:9", "HD 16:9" } },
            { "Square_1_1",  new[] { "Square 1:1", "正方形 1:1", "正方形 1:1", "正方形 1:1" } },
            { "Custom",      new[] { "Custom", "自訂", "自定义", "カスタム" } },

            // Status / Help
            { "Status",           new[] { "Status", "狀態", "状态", "ステータス" } },
            { "Reset",            new[] { "Reset", "重設", "重置", "リセット" } },
            { "KeysHelp",         new[] {
                "WASD: move | Q/E: up/down | RMB: look (hold) | Shift/Ctrl: sprint/slow | Z/C: roll | X: level | R: toggle aim | O: scene sync | P: screenshot | Ctrl+Shift+E: open panel",
                "WASD: 移動｜Q/E: 上/下｜右鍵: 環視(按住)｜Shift/Ctrl: 快/慢｜Z/C: 滾轉｜X: 水平回中｜R: 切換對準｜O: 場景同步｜P: 截圖｜Ctrl+Shift+E: 開啟面板",
                "WASD: 移动｜Q/E: 上/下｜右键: 环视(按住)｜Shift/Ctrl: 快/慢｜Z/C: 滚转｜X: 水平回中｜R: 切换对准｜O: 场景同步｜P: 截图｜Ctrl+Shift+E: 打开面板",
                "WASD: 移動 | Q/E: 上下 | 右クリック: 視点(長押し) | Shift/Ctrl: 速/遅 | Z/C: ロール | X: 水平 | R: 切替ターゲット | O: シーン同期 | P: スクショ | Ctrl+Shift+E: パネル開く"
            } },

            // Quality
            { "MSAA",             new[] { "RT MSAA", "RT MSAA", "RT MSAA", "RT MSAA" } },
            { "FileNameTemplate", new[] { "File Name", "檔名模板", "文件名模板", "ファイル名テンプレート" } },

            // Status and UI elements
            { "CameraSpeed",      new[] { "Camera Speed", "攝影機速度", "相机速度", "カメラ速度" } },
            { "LockObject",       new[] { "Lock Object", "鎖定物件", "锁定对象", "ロック対象" } },
            { "Display",          new[] { "Display", "Display", "Display", "Display" } },
            { "Repair",           new[] { "Repair", "修復", "修复", "修復" } },
            { "Normal",           new[] { "Normal", "正常", "正常", "正常" } },
            { "NoCamera",         new[] { "No Camera Found", "未找到相機", "未找到相机", "カメラが見つかりません" } },
            { "MissingComponents",new[] { "Missing Components", "缺少組件", "缺少组件", "コンポーネント不足" } },
            { "DisplayConflict",  new[] { "Display Conflict", "Display衝突", "Display冲突", "Display衝突" } },
            { "TransparentBG",    new[] { "Transparent BG", "透明背景", "透明背景", "透明背景" } },
            { "JPEGQuality",      new[] { "JPEG Quality", "JPEG 品質", "JPEG 质量", "JPEG 品質" } },
            { "ComponentMissing", new[] { "Component Missing, Click Repair", "組件缺失，請點擊修復", "组件缺失，请点击修复", "コンポーネント不足、修復をクリック" } },

            // Lock object states
            { "Locking",          new[] { "Locking", "鎖定中", "锁定中", "ロック中" } },
            { "Unlocked",         new[] { "Unlocked", "未鎖定", "未锁定", "ロック解除" } },
            { "NoTarget",         new[] { "No Target", "無目標", "无目标", "対象なし" } },
            { "PlayModeOnly",     new[] { "Play Mode Only", "僅播放模式", "仅播放模式", "プレイモードのみ" } },
            { "LockDialog",       new[] { "Lock", "鎖定", "锁定", "ロック" } },
            { "SelectTargetFirst",new[] { "Please select a target object first", "請先選擇要鎖定的物件", "请先选择要锁定的对象", "まず対象オブジェクトを選択してください" } },
            { "OK",               new[] { "OK", "確定", "确定", "OK" } },

            // Display conflict repair
            { "DisplayConflictRepair", new[] { "Display Conflict Repair", "Display 衝突修復", "Display 冲突修复", "Display 衝突修復" } },
            { "DetectedCamerasUsingDisplay", new[] { "Detected {0} cameras using Display {1}", "檢測到 {0} 個相機同時在使用 Display {1}", "检测到 {0} 个相机同时在使用 Display {1}", "{0} 台のカメラが Display {1} を使用中です" } },
            { "CloseOtherCamerasOrChangeDisplay", new[] { "Close other cameras or change plugin's Display channel", "關閉其他相機或改變插件的Display通道", "关闭其他相机或改变插件的Display通道", "他のカメラを閉じるかプラグインのDisplayチャンネルを変更" } },
            { "ChooseRepairMethod", new[] { "Choose repair method:", "請選擇修復方式:", "请选择修复方式:", "修復方法を選択してください:" } },
            { "CloseOtherCameras", new[] { "Close Other Cameras", "關閉其他相機", "关闭其他相机", "他のカメラを閉じる" } },
            { "Cancel",           new[] { "Cancel", "取消", "取消", "キャンセル" } },
            { "ChangeDisplayChannel", new[] { "Change Display Channel", "更改display通道", "更改display通道", "Displayチャンネルを変更" } },
            { "RepairComplete",   new[] { "Repair Complete", "修復完成", "修复完成", "修復完了" } },
            { "RepairFailed",     new[] { "Repair Failed", "修復失敗", "修复失败", "修復失敗" } },
            { "ClosedCamerasCount", new[] { "Closed {0} camera objects to avoid channel overlay.", "已關閉 {0} 個相機物件，避免通道覆蓋。", "已关闭 {0} 个相机对象，避免通道覆盖。", "{0} 台のカメラオブジェクトを閉じてチャンネル重複を回避しました。" } },
            { "NoteCamerasDisabled", new[] { "Note: These camera objects have been disabled and can be manually re-enabled when needed.", "注意：這些相機物件已被禁用，需要時可以手動重新啟用。", "注意：这些相机对象已被禁用，需要时可以手动重新启用。", "注意：これらのカメラオブジェクトは無効化されており、必要に応じて手動で再有効化できます。" } },
            { "MovedToDisplay",   new[] { "Moved EditorScreenShot camera to Display {0} to avoid channel overlay.", "已將 EditorScreenShot 相機移動到 Display {0}，避免通道覆蓋。", "已将 EditorScreenShot 相机移动到 Display {0}，避免通道覆盖。", "EditorScreenShot カメラを Display {0} に移動してチャンネル重複を回避しました。" } },
            { "AllDisplaysOccupied", new[] { "All Display channels are occupied and cannot be automatically repaired. Please manually select another Display.", "所有 Display 通道都被占用，無法自動修復。請手動選擇其他 Display。", "所有 Display 通道都被占用，无法自动修复。请手动选择其他 Display。", "すべてのDisplayチャンネルが使用中で自動修復できません。手動で他のDisplayを選択してください。" } },
            { "FixConflict",      new[] { "Fix Conflict", "修復衝突", "修复冲突", "衝突を修復" } },
        };

        /// <summary>
        /// Auto-select appropriate language based on system language
        /// </summary>
        public static Lang GetSystemDefaultLanguage()
        {
            try
            {
                // Get current system language
                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                string languageCode = currentCulture.TwoLetterISOLanguageName.ToLower();
                string cultureName = currentCulture.Name.ToLower();

                // Map based on language and region codes
                if (languageCode == "zh")
                {
                    // Simplified Chinese regions
                    if (cultureName.Contains("cn") || cultureName.Contains("hans"))
                        return Lang.ChineseSimplified;
                    // Traditional Chinese regions (Taiwan, Hong Kong, Macau, etc.)
                    else if (cultureName.Contains("tw") || cultureName.Contains("hk") || 
                             cultureName.Contains("mo") || cultureName.Contains("hant"))
                        return Lang.Chinese;
                    // Default to Simplified Chinese
                    else
                        return Lang.ChineseSimplified;
                }
                else if (languageCode == "ja")
                {
                    return Lang.Japanese;
                }
                else
                {
                    // Default to English
                    return Lang.English;
                }
            }
            catch
            {
                // If detection fails, return default language (Traditional Chinese)
                return Lang.Chinese;
            }
        }

        /// <summary>
        /// Get friendly display name for system language
        /// </summary>
        public static string GetSystemLanguageDisplayName()
        {
            try
            {
                CultureInfo currentCulture = CultureInfo.CurrentCulture;
                return $"{currentCulture.DisplayName} ({currentCulture.Name})";
            }
            catch
            {
                return "Unknown";
            }
        }

        public static string T(string key)
        {
            if (!dict.TryGetValue(key, out var arr)) return key;
            switch (_lang)
            {
                case Lang.English:         return arr[0];
                case Lang.Chinese:         return arr[1];
                case Lang.ChineseSimplified: return arr[2];
                case Lang.Japanese:        return arr[3];
                default:                   return arr[1];
            }
        }
    }
}
