using Terraria;

namespace ShopExpander.Patches
{
    internal static class AddShopPatch
    {
        public static void Load()
        {
            On.Terraria.Chest.AddItemToShop += Prefix;
        }

        private static int Prefix(On.Terraria.Chest.orig_AddItemToShop orig, Chest self, Item newItem)
        {
            if (self != Main.instance.shop[Main.npcShop] || ShopExpander.Instance.ActiveShop == null)
                return orig(self, newItem);

            Item insertItem = newItem.Clone();
            insertItem.favorited = false;
            insertItem.buyOnce = true;
            if (insertItem.value > 0)
            {
                insertItem.value /= 5;
                if (insertItem.value < 1)
                    insertItem.value = 1;
            }

            ShopExpander.Instance.Buyback.AddItem(insertItem);
            ShopExpander.Instance.ActiveShop.RefreshFrame();

            return 0; //TODO: Fix PostSellItem hook
        }
    }
}