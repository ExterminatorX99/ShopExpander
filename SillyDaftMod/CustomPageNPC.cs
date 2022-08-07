namespace SillyDaftMod;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

internal class CustomPageNPC : GlobalNPC
{
    public override void SetupShop(int type, Chest shop, ref int nextSlot)
    {
        if (type == NPCID.Dryad)
        {
            SetupCustomPages(shop, ref nextSlot);
        }
    }

    //This method demonstrates how you can add simple custom item pages,
    //without needing to add a reference to Shop Expander.
    private void SetupCustomPages(Chest shop, ref int nextSlot)
    {
        Item[] items1 = { MakeItem(ItemID.DirtBlock, "Cube of Earth"), MakeItem(ItemID.MoneyTrough, "Bar of Soap"), MakeItem(ItemID.CopperShortsword, "Legendary Terrablade"), MakeItem(ItemID.Wood, "Spaghetti") };

        Item[] items2 = { MakeItem(ItemID.StoneBlock, "Cube of Rock"), MakeItem(ItemID.LastPrism, "First Prism"), MakeItem(ItemID.EmptyBucket, "Stylish Hat"), MakeItem(ItemID.Shadewood, "Spicy Spaghetti") };

        if (ModLoader.TryGetMod("ShopExpander", out var shopMod))
        {
            shopMod.Call("AddPageFromArray", "Cool Items", -2, items1);
            shopMod.Call("AddPageFromArray", "Even Cooler Items", -1, items2);
        }
        else
        {
            //If Shop Expander isn't loaded, fall back to vanilla
            foreach (var item in items1)
            {
                shop.item[nextSlot++] = item;
            }

            foreach (var item in items2)
            {
                shop.item[nextSlot++] = item;
            }
        }
    }

    private static Item MakeItem(int type, string name)
    {
        var item = new Item(type);
        item.SetNameOverride(name);
        item.rare = ItemRarityID.Blue;
        if (item.value == 0)
        {
            item.value = 1;
        }

        item.value *= 11;
        return item;
    }
}
