namespace ShopExpander.Patches;

using Terraria.UI;

internal static class LeftRightClickPatch
{
    public static void Load()
    {
        On_ItemSlot.HandleShopSlot += HandleShopSlot;
    }

    private static void HandleShopSlot(On_ItemSlot.orig_HandleShopSlot orig, Item[] inv, int slot, bool rightClickIsValid, bool leftClickIsValid)
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
        if (ShopExpanderMod.ActiveShop == null)
        {
            return false;
        }

        if (inv[slot].type == ShopExpanderMod.ArrowLeft.Item.type)
        {
            if (skip)
            {
                ShopExpanderMod.ActiveShop.MoveFirst();
            }
            else if (Main.mouseLeftRelease)
            {
                ShopExpanderMod.ActiveShop.MoveLeft();
            }

            return true;
        }

        if (inv[slot].type == ShopExpanderMod.ArrowRight.Item.type)
        {
            if (skip)
            {
                ShopExpanderMod.ActiveShop.MoveLast();
            }
            else if (Main.mouseLeftRelease)
            {
                ShopExpanderMod.ActiveShop.MoveRight();
            }

            return true;
        }

        return false;
    }
}
