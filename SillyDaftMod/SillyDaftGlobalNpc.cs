namespace SillyDaftMod;

using Terraria.ID;
using Terraria.ModLoader;

internal class SillyDaftGlobalNpc : GlobalNPC
{
    // This method demonstrates how Shop Expander can handle mods, that add way too many items using the vanilla method.
    public override void ModifyShop(NPCShop shop)
    {
        if (shop.NpcType == NPCID.Dryad && ModLoader.HasMod("ShopExpander"))
        {
            for (var i = 0; i < ItemLoader.ItemCount; i++)
            {
                shop.Add(i);
                shop.Add(i);
            }
        }
    }
}
