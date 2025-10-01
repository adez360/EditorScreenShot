using System;

namespace EditorScreenShot.Runtime
{
    /// <summary>
    /// Editor-only screenshot and scene sync gateway (safe to call at runtime).
    /// Player builds should provide a no-op default.
    /// </summary>
    public interface IEditorShotService
    {
        void Capture();
        void ToggleSceneSync();
    }

    /// <summary>
    /// Static accessor for the service. In Player, remains a no-op unless set by integrator.
    /// In Editor, an implementation will be registered at load time.
    /// </summary>
    public static class EditorShotService
    {
        class NullService : IEditorShotService
        {
            public void Capture() { }
            public void ToggleSceneSync() { }
        }

        static IEditorShotService _current = new NullService();
        public static IEditorShotService Current
        {
            get => _current;
            set => _current = value ?? new NullService();
        }
    }
}


