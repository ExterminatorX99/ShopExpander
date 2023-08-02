namespace SillyDaftMod;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

internal class FreeloadGlobalNpc : GlobalNPC
{
    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
    {
        if (npc.type == NPCID.Dryad && ModLoader.HasMod("ShopExpander"))
        {
            foreach (Item item in items)
            {
                // Skip 'air' items and null items.
                if (item == null || item.type == ItemID.None)
                {
                    continue;
                }

                item.shopCustomPrice = 0;
            }
        }
    }
}
