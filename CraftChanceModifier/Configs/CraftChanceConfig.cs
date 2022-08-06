using BepInEx.Configuration;

namespace CraftChanceModifier.Configs
{
    internal static class CraftChanceConfig
    {
        internal static ConfigEntry<float> CraftChanceModifier { get; private set; }

        internal static void Initialize(ConfigFile config)
        {
            CraftChanceModifier = config.Bind(nameof(CraftChanceConfig), nameof(CraftChanceModifier), 1.0f, "Craft chance modifier value");
        }
    }
}
