using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using BepInEx;
using BepInEx.Configuration;
using Logging;
using UnityEngine;
using UnityEngine.Rendering;


namespace Configs;
internal static class ConfigManagerWatcher
{


    private static readonly List<string> ConfigManagerGUIDs = new List<string>()
    {
        "_shudnal.ConfigurationManager",
        "com.bepis.bepinex.configurationmanager"
    };
    private static BaseUnityPlugin _configManager = null;
    private const string WindowChangedEventName = "DisplayingWindowChanged";
    private const string DisplayingWindowName = "DisplayingWindow";
    private static PropertyInfo _dispWindowInfo = null;


    /// <summary>
    ///     Gets and caches a reference to the in-game config manager if one is installed.
    /// </summary>
    private static BaseUnityPlugin ConfigManager
    {
        get
        {
            if (_configManager == null)
            {
                foreach (string GUID in ConfigManagerGUIDs)
                {
                    if (Chainloader.PluginInfos.TryGetValue(GUID, out PluginInfo configManagerInfo) && configManagerInfo.Instance)
                    {
                        _configManager = configManagerInfo.Instance;
                        break;
                    }
                }
            }
            return _configManager;
        }
    }   

    /// <summary>
    ///     Caches the PropertyInfo for ConfigManager.DisplayingWindow
    /// </summary>
    private static PropertyInfo DisplayWindowInfo
    {
        get
        {
            _dispWindowInfo ??= ConfigManager.GetType().GetProperty(DisplayingWindowName);
            return _dispWindowInfo;
        }
    }

    /// <summary>
    ///     Event triggered after a the in-game configuration manager is closed.
    /// </summary>
    internal static event Action OnConfigurationWindowClosed;

    /// <summary>
    ///     Safely invoke the <see cref="OnConfigWindowClosed"/> event.
    /// </summary>
    private static void InvokeOnConfigurationWindowClosed()
    {
        OnConfigurationWindowClosed?.SafeInvoke();
    }

    /// <summary>
    ///     Checks for in-game configuration manager and
    ///     sets Up OnConfigWindowClosed event if it is present
    /// </summary>
    internal static void CheckForConfigManager(this ConfigFile config
        )
    {
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
        {
            return;
        }

        if (ConfigManager == null)
        {
            return;
        }
        Log.LogDebug($"Configuration manager found, hooking {WindowChangedEventName}");

        EventInfo eventinfo = ConfigManager.GetType().GetEvent(WindowChangedEventName);
        if (eventinfo == null)
        {
            return;
        }

        Action<object, object> local = new(OnConfigManagerDisplayingWindowChanged);
        Delegate converted = Delegate.CreateDelegate(
            eventinfo.EventHandlerType,
            local.Target,
            local.Method
        );
        eventinfo.AddEventHandler(ConfigManager, converted);
    }

    /// <summary>
    ///     Invokes OnConfigWindowClosed if window has changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void OnConfigManagerDisplayingWindowChanged(object sender, object e)
    {
        if (DisplayWindowInfo == null)
        {
            return;
        }

        bool ConfigurationManagerWindowShown = (bool)DisplayWindowInfo.GetValue(ConfigManager, null);
        if (!ConfigurationManagerWindowShown)
        {
            InvokeOnConfigurationWindowClosed();
        }
    }

}
