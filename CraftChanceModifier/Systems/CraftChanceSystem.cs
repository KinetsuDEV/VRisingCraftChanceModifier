using CraftChanceModifier.Configs;
using CraftChanceModifier.Models;
using ProjectM;
using ProjectM.Network;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Wetstone.API;

namespace CraftChanceModifier.Systems
{
    internal static class CraftChanceSystem
    {
        internal static void CraftSucceeded(StartCraftingSystem craftingSystem)
        {
            if (craftingSystem._StartCraftItemEventQuery.IsEmpty || IsSuccessCraft())
                return;

            var craftEventsEntities = craftingSystem._StartCraftItemEventQuery.ToEntityArray(Allocator.Temp);

            foreach (var craftEventEntity in craftEventsEntities)
            {
                var craftEvent = VWorld.Server.EntityManager.GetComponentData<StartCraftItemEvent>(craftEventEntity);
                var fromCharacter = VWorld.Server.EntityManager.GetComponentData<FromCharacter>(craftEventEntity);
                var user = VWorld.Server.EntityManager.GetComponentData<User>(fromCharacter.User);
                var workstationEntity = craftEvent.Workstation.GetNetworkedEntity(craftingSystem._NetworkIdSystem._NetworkIdToEntityMap)._Entity;

                if (!InventoryUtilities.TryGetInventoryEntity(craftingSystem.EntityManager, workstationEntity, out Entity workstationInventory) || workstationInventory == Entity.Null)
                    return;

                if (!InventoryUtilities.TryGetInventoryEntity(craftingSystem.EntityManager, fromCharacter.Character, out Entity playerInventory) || playerInventory == Entity.Null)
                    return;

                if (!CanCraft(craftEvent.RecipeId, workstationInventory, playerInventory, out List<ResourceOutput> resourcesFound))
                    return;

                user.SendSystemMessage($"You have failed to craft {GetRecipeItemName(craftEvent.RecipeId)}");

                foreach (var item in resourcesFound)
                    InventoryUtilitiesServer.TryRemoveItem(VWorld.Server.EntityManager, item.SourceInventory, item.Item, item.Amount);

                VWorld.Server.EntityManager.DestroyEntity(craftEventEntity);
            }
        }

        private static bool IsSuccessCraft()
        {
            var successRate = Math.Min(100, CraftChanceConfig.CraftChanceModifier.Value * 100);
            return new Random(DateTime.Now.Millisecond).Next(100) < successRate;
        }

        private static bool CanCraft(PrefabGUID recipeId, Entity workstationInventory, Entity playerInventory, out List<ResourceOutput> resourcesFound)
        {
            resourcesFound = new List<ResourceOutput>();
            var recipeEntity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>().PrefabLookupMap[recipeId];
            var recipeRequirements = VWorld.Server.EntityManager.GetBuffer<RecipeRequirementBuffer>(recipeEntity);

            var workstationInventoryBuffer = VWorld.Server.EntityManager.GetBuffer<InventoryBuffer>(workstationInventory);
            var playerInventoryBuffer = VWorld.Server.EntityManager.GetBuffer<InventoryBuffer>(playerInventory);

            foreach (var recipeRequirement in recipeRequirements)
            {
                var ownedAmount = 0;

                foreach (var inventoryItem in workstationInventoryBuffer)
                {
                    if (ownedAmount >= recipeRequirement.Stacks)
                        break;

                    if (inventoryItem.ItemType == recipeRequirement.Guid)
                    {
                        var neededAmount = Math.Min(inventoryItem.Stacks, recipeRequirement.Stacks - ownedAmount);
                        resourcesFound.Add(new ResourceOutput(workstationInventory, recipeRequirement.Guid, neededAmount));
                        ownedAmount += neededAmount;
                    }
                }

                foreach (var inventoryItem in playerInventoryBuffer)
                {
                    if (ownedAmount >= recipeRequirement.Stacks)
                        break;

                    if (inventoryItem.ItemType == recipeRequirement.Guid)
                    {
                        var neededAmount = Math.Min(inventoryItem.Stacks, recipeRequirement.Stacks - ownedAmount);
                        resourcesFound.Add(new ResourceOutput(playerInventory, recipeRequirement.Guid, neededAmount));
                        ownedAmount += neededAmount;
                    }
                }

                if (ownedAmount < recipeRequirement.Stacks)
                    return false;
            }

            return true;
        }

        private static string GetRecipeItemName(PrefabGUID recipeId)
        {
            var recipeEntity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>().PrefabLookupMap[recipeId];
            var recipeOutput = VWorld.Server.EntityManager.GetBuffer<RecipeOutputBuffer>(recipeEntity);

            if (recipeOutput.IsEmpty)
                return string.Empty;

            var itemEntity = VWorld.Server.GetExistingSystem<PrefabCollectionSystem>().PrefabLookupMap[recipeOutput[0].Guid];

            if (!VWorld.Server.EntityManager.HasComponent<ItemData>(itemEntity))
                return string.Empty;

            var itemData = VWorld.Server.EntityManager.GetComponentData<ItemData>(itemEntity);
            var managedItemData = VWorld.Server.GetExistingSystem<GameDataSystem>().ManagedDataRegistry.GetOrDefault<ManagedItemData>(itemData.ItemTypeGUID);

            return managedItemData.Name.IsEmpty ? string.Empty : managedItemData.Name.ToString();
        }
    }
}
