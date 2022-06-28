using Terraria;

namespace ShopExpander.Patches
{
    internal static class LeftRightClickPatch
    {
        public static void Load()
        {
            On.Terraria.UI.ItemSlot.HandleShopSlot += PrefixLeft;
            On.Terraria.UI.ItemSlot.HandleShopSlot += PrefixRight;
        }

        private static void PrefixLeft(On.Terraria.UI.ItemSlot.orig_HandleShopSlot orig, Item[] inv, int slot, bool rightClickIsValid, bool leftClickIsValid)
        {
            if (leftClickIsValid && Main.mouseLeft && Main.mouseLeftRelease && ClickedPageArrow(inv, slot, false))
                return;

            orig(inv, slot, rightClickIsValid, leftClickIsValid);
        }

        private static void PrefixRight(On.Terraria.UI.ItemSlot.orig_HandleShopSlot orig, Item[] inv, int slot, bool rightClickIsValid, bool leftClickIsValid)
        {
            if (rightClickIsValid && Main.mouseRight && ClickedPageArrow(inv, slot, true))
                return;

            orig(inv, slot, rightClickIsValid, leftClickIsValid);
        }

        private static bool ClickedPageArrow(Item[] inv, int slot, bool skip)
        {
            if (ShopExpander.Instance.ActiveShop == null)
                return false;

            if (inv[slot].type == ShopExpander.Instance.ArrowLeft.Item.type)
            {
                if (skip)
                    ShopExpander.Instance.ActiveShop.MoveFirst();
                else
                    ShopExpander.Instance.ActiveShop.MoveLeft();
                return true;
            }

            if (inv[slot].type == ShopExpander.Instance.ArrowRight.Item.type)
            {
                if (skip)
                    ShopExpander.Instance.ActiveShop.MoveLast();
                else
                    ShopExpander.Instance.ActiveShop.MoveRight();
                return true;
            }

            return false;
        }
    }
}