using UnityEditor;
using EditorScreenShot.Runtime;

namespace EditorScreenShot.Editor
{
    /// <summary>
    /// Registers the editor implementation of IEditorShotService at editor load time.
    /// </summary>
    [InitializeOnLoad]
    public static class EditorShotServiceImpl
    {
        static EditorShotServiceImpl()
        {
            EditorShotService.Current = new Impl();
        }

        class Impl : IEditorShotService
        {
            public void Capture()
            {
                EditorScreenShotWindow.CaptureGlobal();
            }

            public void ToggleSceneSync()
            {
                EditorScreenShotWindow.ToggleSceneSyncGlobal();
            }
        }
    }
}


