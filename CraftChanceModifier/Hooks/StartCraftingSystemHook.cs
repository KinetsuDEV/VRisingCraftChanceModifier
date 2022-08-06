using CraftChanceModifier.Systems;
using HarmonyLib;
using ProjectM;
using System;

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
                CraftChanceSystem.CraftSucceeded(__instance);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
        }
    }
}
