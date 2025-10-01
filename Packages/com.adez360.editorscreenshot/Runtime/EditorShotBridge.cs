using System;

public static class EditorShotBridge
{
    // Editor window subscribes to this. Runtime raises it on P key.
    public static event Action RequestScreenshot;

    public static void RaiseScreenshotRequest() => RequestScreenshot?.Invoke();
}
