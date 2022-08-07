namespace ShopExpander.Patches;

using On.Terraria.UI;

internal static class LeftRightClickPatch
{
    public static void Load()
    {
        ItemSlot.HandleShopSlot += HandleShopSlot;
    }

    private static void HandleShopSlot(ItemSlot.orig_HandleShopSlot orig, Item[] inv, int slot, bool rightClickIsValid, bool leftClickIsValid)
    {
        if (leftClickIsValid && Main.mouseLeft && ClickedPageArrow(inv, slot, false))
        {
            return;
        }

        if (rightClickIsValid && Main.mouseRight && ClickedPageArrow(inv, slot, true))
        {
            return;
        }

        orig(inv, slot, rightClickIsValid, leftClickIsValid);
    }

    private static bool ClickedPageArrow(Item[] inv, int slot, bool skip)
    {
        if (ShopExpander.ActiveShop == null)
        {
            return false;
        }

        if (inv[slot].type == ShopExpander.ArrowLeft.Item.type)
        {
            if (skip)
            {
                ShopExpander.ActiveShop.MoveFirst();
            }
            else if (Main.mouseLeftRelease)
            {
                ShopExpander.ActiveShop.MoveLeft();
            }

            return true;
        }

        if (inv[slot].type == ShopExpander.ArrowRight.Item.type)
        {
            if (skip)
            {
                ShopExpander.ActiveShop.MoveLast();
            }
            else if (Main.mouseLeftRelease)
            {
                ShopExpander.ActiveShop.MoveRight();
            }

            return true;
        }

        return false;
    }
}
