using BepInEx;
using BepInEx.Configuration;

using System.Linq;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using static UnityEngine.UI.CanvasScaler;

namespace ChangeCanvasScaler
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<ScreenMatchMode> ScreenMatchMode;
        public static ConfigEntry<float> MatchWidthOrHeight;
        public static ConfigEntry<bool> LogCanvasScalers;
        public static ConfigEntry<bool> CanvasScalerListAsBlacklist;
        public static ConfigEntry<string> CanvasScalerList;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            ScreenMatchMode = Config.Bind(
                "General",
                "ScreenMatchMode",
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight,
                "Scale the canvas area with the width as reference, the height as reference, or something in between."
                );

            MatchWidthOrHeight = Config.Bind(
                "General",
                "MatchWidthOrHeight",
                1f,
                "0 = width, 1 = height. Only used if ScreenMatchMode is set to MatchWidthOrHeight"
                );

            CanvasScalerList = Config.Bind(
                "General",
                "CanvasScalerList",
                string.Empty,
                "A semicolon delimited list of CanvasScaler names that should be whitelisted or blacklisted"
                );

            CanvasScalerListAsBlacklist = Config.Bind(
                "General",
                "CanvasScalerListAsBlacklist",
                true,
                "Whether CanvasScalerList should operate as a blacklist or a whitelist. False = Whitelist True = Blacklist"
                );

            LogCanvasScalers = Config.Bind(
                "Logging",
                "LogCanvasScalers",
                false,
                "Whether found CanvasScaler names should be printed to the log");

            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            Task.Run(ChangeScalers);
        }

        private void ChangeScalers()
        {
            var canvasScalers = FindObjectsOfType<CanvasScaler>();
            var canvasScalerList = CanvasScalerList.Value.Split(';');

            foreach (var canvasScaler in canvasScalers)
            {
                var csPath = string.Join("/", canvasScaler.GetComponentsInParent<Transform>().Select(t => t.name).Reverse().ToArray());

                if (LogCanvasScalers.Value)
                {
                    Logger.LogInfo($"Found canvas scaler at {csPath}");
                }

                foreach (var csName in canvasScalerList)
                {
                    if (CanvasScalerListAsBlacklist.Value)
                    {
                        if (csName == csPath)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (csName != csPath)
                        {
                            return;
                        }
                    }
                }

                ThreadingHelper.Instance.StartSyncInvoke(() =>
                {
                    canvasScaler.m_ScreenMatchMode = ScreenMatchMode.Value;
                    canvasScaler.matchWidthOrHeight = MatchWidthOrHeight.Value;
                });
            }
        }
    }
}
