namespace SillyDaftMod;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

internal class FreeloadGlobalNpc : GlobalNPC
{
    public override void SetupShop(int type, Chest shop, ref int nextSlot)
    {
        if (type == NPCID.Dryad && ModLoader.HasMod("ShopExpander"))
        {
            for (var i = 0; i < nextSlot; i++)
            {
                shop.item[i].shopCustomPrice = 0;
            }
        }
    }
}
