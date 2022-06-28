using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SillyDaftMod
{
    internal class SillyDaftGlobalNpc : GlobalNPC
    {
        //This method demonstrates how Shop Expander can handle mods, that add
        //way too many items using the vanilla method.
        //(To allow this many items, the provision size was changed in SillyDaftMod.PostSetupContent)
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (type == NPCID.Dryad)
            {
                for (int i = 0; i < ItemLoader.ItemCount; i++)
                {
                    shop.item[nextSlot++].SetDefaults(i);
                    shop.item[nextSlot++].SetDefaults(i);
                }
            }
        }
    }
}