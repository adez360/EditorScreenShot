using System;
using System.IO;

namespace EditorScreenShot.Runtime
{
    public static class OutputPathResolver
    {
    // Default: C:\Users\(Username)\Pictures\UnityScreenShots (OneDrive preferred)
    public static string GetDefaultScreenshotDir()
    {
        try
        {
            string one = Environment.GetEnvironmentVariable("OneDrive");
            if (!string.IsNullOrEmpty(one))
            {
                string pics = Path.Combine(one, "Pictures");
                if (Directory.Exists(pics)) return Path.Combine(pics, "UnityScreenShots");

                string zhPics = Path.Combine(one, "圖片");
                if (Directory.Exists(zhPics)) return Path.Combine(zhPics, "UnityScreenShots");
            }

            string myPics = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            if (!string.IsNullOrEmpty(myPics)) return Path.Combine(myPics, "UnityScreenShots");
        }
        catch { /* fallback below */ }

        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UnityScreenShots");
    }
}
}


