using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SillyDaftMod
{
    internal class FreeloadGlobalNpc : GlobalNPC
    {
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (type == NPCID.Dryad && ModLoader.HasMod("ShopExpander"))
            {
                for (int i = 0; i < nextSlot; i++)
                {
                    shop.item[i].shopCustomPrice = 0;
                }
            }
        }
    }
}