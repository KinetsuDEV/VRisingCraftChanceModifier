using CraftChanceModifier.Systems;
using HarmonyLib;
using ProjectM;
using System;
using VRisingUtils.Utils;

namespace CraftChanceModifier.Hooks
{
    [Harmony]
    internal static class StartCraftingSystemHook
    {
        [HarmonyPatch(typeof(StartCraftingSystem), nameof(StartCraftingSystem.OnUpdate))]
        [HarmonyPrefix]
        private static void StartCraftingSystem_OnUpdate_Prefix(StartCraftingSystem __instance)
        {
            try
            {
                CraftChanceSystem.RollCraftChance(__instance);
            }
            catch (Exception e)
            {
                LogUtils.Logger.LogError(e);
            }
        }
    }
}
