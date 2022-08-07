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

            int stack = Main.shopSellbackHelper.Remove(newItem);

            if (stack >= newItem.stack)
                return 0;

            Item insertItem = newItem.Clone();
            insertItem.favorited = false;
            insertItem.buyOnce = true;
            insertItem.stack -= stack;

            ShopExpander.Instance.Buyback.AddItem(insertItem);
            ShopExpander.Instance.ActiveShop.RefreshFrame();

            return 0; //TODO: Fix PostSellItem hook
        }
    }
}