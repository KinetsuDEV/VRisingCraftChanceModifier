using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CraftChanceModifier.Configs;
using HarmonyLib;
using System.Reflection;
using Wetstone.API;

namespace CraftChanceModifier
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency("xyz.molenzwiebel.wetstone")]
    public class Plugin : BasePlugin
    {
        private const string PLUGIN_GUID = "CraftChanceModifier";
        private const string PLUGIN_NAME = "CraftChanceModifier";
        private const string PLUGIN_VERSION = "1.0.0";
        private Harmony harmony;

        internal static ManualLogSource Logger { get; private set; }

        public override void Load()
        {
            if (!VWorld.IsServer)
            {
                Log.LogWarning($"Plugin {PLUGIN_NAME} is server side only");
                return;
            }

            Logger = Log;
            CraftChanceConfig.Initialize(Config);

            harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.LogInfo($"Plugin {PLUGIN_NAME} {PLUGIN_VERSION} loaded successfully!");
        }

        public override bool Unload()
        {
            if (!VWorld.IsServer)
                return true;

            Config.Clear();
            harmony.UnpatchSelf();

            return true;
        }
    }
}
