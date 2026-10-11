using System;
using System.IO;

namespace Launcher.Helpers;

public static class Constants
{
    public const string LogFile = "Launcher.log";
    public const string SettingsFile = "Launcher.xml";

    public const string SaveDirectory = "OSFRLauncher"; // Existing profile preserved for backward compatibility.
    public const string ServersDirectory = "Servers";

    public const string ClientExecutableName = "FreeRealms.exe";
    public const string DirectXDownloadUrl = "https://www.microsoft.com/download/details.aspx?id=35";

    public static readonly string[] DefaultServerUrls = []; // Configure the New Dawn server explicitly.

    public static readonly string LogsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");

    public static readonly string SavePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                SaveDirectory);
}