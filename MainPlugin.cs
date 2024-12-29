using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Utils;
using Jotunn.Managers;
using Configs;
using Logging;

// To begin using: rename the solution and project, then find and replace all instances of "ModTemplate"
// Next: rename the main plugin as desired.

// If using Jotunn then the following files should be removed from Configs:
// - ConfigManagerWatcher
// - ConfigurationManagerAttributes
// - SaveInvokeEvents
// - ConfigFileExtensions should be editted to only include `DisableSaveOnConfigSet`

// If not using Jotunn
// - Remove the decorators on the MainPlugin
// - Swap from using SynchronizationManager.OnConfigurationWindowClosed to using ConfigManagerWatcher.OnConfigurationWindowClosed
// - Remove calls to SynchronizationManager.OnConfigurationSynchronized
// - Adjust using statements as needed
// - Remove nuget Jotunn package via manage nuget packages
// - Uncomment the line: <Import Project="$(JotunnProps)" Condition="Exists('$(JotunnProps)')" /> in the csproj file

namespace ModTemplate;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency(Jotunn.Main.ModGuid, Jotunn.Main.Version)]
[NetworkCompatibility(CompatibilityLevel.VersionCheckOnly, VersionStrictness.Patch)]
[SynchronizationMode(AdminOnlyStrictness.IfOnServer)]
internal sealed class MainPlugin : BaseUnityPlugin
{
    public const string PluginName = "ModTemplate";
    internal const string Author = "Searica";
    public const string PluginGUID = $"{Author}.Valheim.{PluginName}";
    public const string PluginVersion = "0.1.0";

    internal static MainPlugin Instance;
    internal static ConfigFile ConfigFile;
    internal static ConfigFileWatcher ConfigFileWatcher;


    // Global settings
    internal const string GlobalSection = "Global";

    public void Awake()
    {
        Instance = this;
        ConfigFile = Config;
        Log.Init(Logger);

        Config.DisableSaveOnConfigSet();
        SetUpConfigEntries();
        Config.Save();
        Config.SaveOnConfigSet = true;

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), harmonyInstanceId: PluginGUID);
        Game.isModded = true;

        // Re-initialization after reloading config and don't save since file was just reloaded
        ConfigFileWatcher = new(Config);
        ConfigFileWatcher.OnConfigFileReloaded += () =>
        {
            // do stuff
        };

        SynchronizationManager.OnConfigurationSynchronized += (obj, e) =>
        {
            // do stuff
        };

        SynchronizationManager.OnConfigurationWindowClosed += () =>
        {
            // do stuff
        };

    }

    internal void SetUpConfigEntries()
    {
        
    }

    public void OnDestroy()
    {
        Config.Save();
    }

}
