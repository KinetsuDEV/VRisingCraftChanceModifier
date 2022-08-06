using CraftChanceModifier.Configs;
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using VRisingUtils.Extensions;
using VRisingUtils.Models;
using VRisingUtils.Utils;
using Wetstone.API;

namespace CraftChanceModifier.Systems
{
    internal static class CraftChanceSystem
    {
        internal static void RollCraftChance(StartCraftingSystem craftingSystem)
        {
            if (craftingSystem._StartCraftItemEventQuery.IsEmpty || IsSuccessCraft())
                return;

            RunCraftFailure(craftingSystem);
        }

        private static void RunCraftFailure(StartCraftingSystem craftingSystem)
        {
            var craftEventsEntities = craftingSystem._StartCraftItemEventQuery.ToEntityArray(Allocator.Temp);

            foreach (var craftEventEntity in craftEventsEntities)
            {
                var craftEvent = craftingSystem.EntityManager.GetComponentData<StartCraftItemEvent>(craftEventEntity);
                var fromCharacter = craftingSystem.EntityManager.GetComponentData<FromCharacter>(craftEventEntity);
                var user = craftingSystem.EntityManager.GetComponentData<User>(fromCharacter.User);
                var workstationEntity = craftEvent.Workstation.GetNetworkedEntity(craftingSystem._NetworkIdSystem._NetworkIdToEntityMap)._Entity;

                if (!InventoryUtilities.TryGetInventoryEntity(craftingSystem.EntityManager, workstationEntity, out Entity workstationInventory) || workstationInventory == Entity.Null)
                    return;

                if (!InventoryUtilities.TryGetInventoryEntity(craftingSystem.EntityManager, fromCharacter.Character, out Entity playerInventory) || playerInventory == Entity.Null)
                    return;

                if (!CanCraft(craftEvent.RecipeId, workstationInventory, playerInventory, out List<InventorySearchResult> searchResults))
                    return;

                user.SendSystemMessage($"You have failed to craft {ItemUtils.GetRecipeItemName(craftEvent.RecipeId)}");

                foreach (var searchResult in searchResults)
                    foreach (var item in searchResult.ItemsFound)
                        InventoryUtilitiesServer.TryRemoveItem(craftingSystem.EntityManager, item.Inventory, item.Item, item.AmountFound);

                craftingSystem.EntityManager.DestroyEntity(craftEventEntity);
            }
        }

        private static bool IsSuccessCraft()
        {
            var successRate = Math.Min(100, CraftChanceConfig.CraftChanceModifier.Value * 100);
            return new Random(DateTime.Now.Millisecond).Next(100) < successRate;
        }

        private static bool CanCraft(PrefabGUID recipeId, Entity workstationInventory, Entity playerInventory, out List<InventorySearchResult> searchResults)
        {
            var recipeRequirements = VWorld.Server.EntityManager.GetBuffer<RecipeRequirementBuffer>(recipeId.GetEntity());
            var results = new List<InventorySearchResult>();

            foreach (var recipeRequirement in recipeRequirements)
            {
                var workstationSearchResults = InventoryUtils.Search(workstationInventory, recipeRequirement.Guid, recipeRequirement.Stacks);
                results.Add(workstationSearchResults);

                if (workstationSearchResults.FoundAll)
                    continue;
                
                var playerSearchResults = InventoryUtils.Search(playerInventory, recipeRequirement.Guid, recipeRequirement.Stacks - workstationSearchResults.AmountFound);
                results.Add(playerSearchResults);

                if (!playerSearchResults.FoundAll)
                    break;
            }

            searchResults = results;
            return InventorySearchResult.AllItemsWhereFound(results);
        }
    }
}
