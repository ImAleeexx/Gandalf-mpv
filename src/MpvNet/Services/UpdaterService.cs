using System.Diagnostics;
using System.IO;
using MpvNet.Help;

namespace MpvNet.Services;

public static class UpdaterService
{
    private static readonly string TempUpdatePath = Path.Combine(Path.GetTempPath(), "GandalfPlayerUpdate.exe");
    
    public static bool CheckAndInstallUpdate()
    {
        try
        {
            string? latestVersion = GandalfApi.GetVersion();
            
            if (string.IsNullOrEmpty(latestVersion))
            {
                Terminal.WriteError("Could not retrieve latest version.");
                return false;
            }

            string currentVersion = AppInfo.Version.ToString();
            
            if (IsNewerVersion(latestVersion, currentVersion))
            {
                Terminal.WriteError($"New version available: {latestVersion} (current: {currentVersion})");
                return DownloadAndInstallUpdate();
            }
            else
            {
                Terminal.WriteError($"You are running the latest version: {currentVersion}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Terminal.WriteError($"Error checking for updates: {ex.Message}");
            return false;
        }
    }

    private static bool IsNewerVersion(string latestVersion, string currentVersion)
    {
        try
        {
            var latest = new Version(latestVersion);
            var current = new Version(currentVersion);
            return latest > current;
        }
        catch
        {
            return false;
        }
    }

    private static string GetGandalfLinkParam()
    {
        //Get startup parameters for gandalf link
        if (GandalfSession.IsGandalf)
        {
            return "gandalf://open?link-id="+GandalfSession.LinkId+"&token="+GandalfSession.Token;
        }
        return "";

    }

    private static bool DownloadAndInstallUpdate()
    {
        try
        {
            Terminal.WriteError("Downloading update...");
            
            byte[]? setupFile = GandalfApi.GetLatestSetupFile();
            
            if (setupFile == null || setupFile.Length == 0)
            {
                Terminal.WriteError("Failed to download update file.");
                return false;
            }

            // Clean up old update file if exists
            if (File.Exists(TempUpdatePath))
                File.Delete(TempUpdatePath);

            File.WriteAllBytes(TempUpdatePath, setupFile);
            Terminal.WriteError($"Update downloaded to: {TempUpdatePath}");

            // Launch installer with silent mode and auto-start after installation
            var gandalfLinkParam = GetGandalfLinkParam();
            var startInfo = new ProcessStartInfo
            {
                FileName = TempUpdatePath,
                Arguments = "/SP- /silent /noicons /GANDALF=\""+gandalfLinkParam+"\"",
                UseShellExecute = true,
                Verb = "runas" // Request admin elevation
            };

            Process.Start(startInfo);
            
            Terminal.WriteError("Installer launched. Application will close to apply updates.");
            return true;
        }
        catch (Exception ex)
        {
            Terminal.WriteError($"Error installing update: {ex.Message}");
            
            // Clean up on failure
            try
            {
                if (File.Exists(TempUpdatePath))
                    File.Delete(TempUpdatePath);
            }
            catch { }
            
            return false;
        }
    }

    public static void CleanupUpdateFiles()
    {
        try
        {
            if (File.Exists(TempUpdatePath))
                File.Delete(TempUpdatePath);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
