namespace ShopExpander.Patches;

using OnChest = On.Terraria.Chest;

internal static class AddShopPatch
{
    public static void Load()
    {
        OnChest.AddItemToShop += Prefix;
    }

    private static int Prefix(OnChest.orig_AddItemToShop orig, Chest self, Item newItem)
    {
        if (self != Main.instance.shop[Main.npcShop] || ShopExpander.Instance.ActiveShop == null)
        {
            return orig(self, newItem);
        }

        var stack = Main.shopSellbackHelper.Remove(newItem);

        if (stack >= newItem.stack)
        {
            return 0;
        }

        var insertItem = newItem.Clone();
        insertItem.favorited = false;
        insertItem.buyOnce = true;
        insertItem.stack -= stack;

        ShopExpander.Instance.Buyback.AddItem(insertItem);
        ShopExpander.Instance.ActiveShop.RefreshFrame();

        return 0; //TODO: Fix PostSellItem hook
    }
}
