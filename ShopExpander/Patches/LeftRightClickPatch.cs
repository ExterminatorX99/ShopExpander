using Terraria;

namespace ShopExpander.Patches
{
    internal static class LeftRightClickPatch
    {
        public static void Load()
        {
            On.Terraria.UI.ItemSlot.LeftClick_ItemArray_int_int += PrefixLeft;
            On.Terraria.UI.ItemSlot.RightClick_ItemArray_int_int += PrefixRight;
        }

        private static void PrefixLeft(On.Terraria.UI.ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            if (Prefix(inv, context, slot, false))
                orig(inv, context, slot);
        }

        private static void PrefixRight(On.Terraria.UI.ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
        {
            if (Main.mouseRight)
            {
                if (Prefix(inv, context, slot, true))
                    orig(inv, context, slot);
            }
            else
            {
                orig(inv, context, slot);
            }
        }

        private static bool Prefix(Item[] inv, int context, int slot, bool skip)
        {
            if (ShopExpander.Instance.ActiveShop == null)
                return true;

            if (inv[slot].type == ShopExpander.Instance.ArrowLeft.Item.type)
            {
                if (context == 15)
                    if (skip)
                        ShopExpander.Instance.ActiveShop.MoveFirst();
                    else
                        ShopExpander.Instance.ActiveShop.MoveLeft();
                else
                    inv[slot] = new Item();
                return false;
            }
            if (inv[slot].type == ShopExpander.Instance.ArrowRight.Item.type)
            {
                if (context == 15)
                    if (skip)
                        ShopExpander.Instance.ActiveShop.MoveLast();
                    else
                        ShopExpander.Instance.ActiveShop.MoveRight();
                else
                    inv[slot] = new Item();
                return false;
            }

            return true;
        }
    }
}