using ProjectM;
using Unity.Entities;

namespace CraftChanceModifier.Models
{
    internal class ResourceOutput
    {
        public Entity SourceInventory { get; set; }
        public PrefabGUID Item { get; set; }
        public int Amount { get; set; }

        public ResourceOutput(Entity source, PrefabGUID item, int amount)
        {
            SourceInventory = source;
            Item = item;
            Amount = amount;
        }
    }
}
