using CraftChanceModifier.Systems;
using HarmonyLib;
using ProjectM;
using System;

namespace CraftChanceModifier.Hooks
{
    [Harmony]
    internal static class LoadPersistanceHook
    {
        [HarmonyPatch(typeof(LoadPersistenceSystemV2), nameof(LoadPersistenceSystemV2.SetLoadState))]
        [HarmonyPrefix]
        private static void LoadPersistenceSystemV2_SetLoadState_Prefix(ServerStartupState.State loadState)
        {
            try
            {
                if (loadState == ServerStartupState.State.SuccessfulStartup)
                    CraftChanceSystem.ApplyCraftChance();
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
        }
    }
}
