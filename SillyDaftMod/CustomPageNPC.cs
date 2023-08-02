namespace SillyDaftMod;

using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

internal class CustomPageNPC : GlobalNPC
{
    public override void ModifyActiveShop(NPC npc, string shopName, Item[] items)
    {
        if (npc.type == NPCID.Dryad)
        {
            SetupCustomPages(items);
        }
    }

    //This method demonstrates how you can add simple custom item pages,
    //without needing to add a reference to Shop Expander.
    private void SetupCustomPages(Item[] items)
    {
        Item[] items1 =
        {
            MakeItem(ItemID.DirtBlock, "Cube of Earth"),
            MakeItem(ItemID.MoneyTrough, "Bar of Soap"),
            MakeItem(ItemID.CopperShortsword, "Legendary Terrablade"),
            MakeItem(ItemID.Wood, "Spaghetti"),
        };

        Item[] items2 =
        {
            MakeItem(ItemID.StoneBlock, "Cube of Rock"),
            MakeItem(ItemID.LastPrism, "First Prism"),
            MakeItem(ItemID.EmptyBucket, "Stylish Hat"),
            MakeItem(ItemID.Shadewood, "Spicy Spaghetti"),
        };

        if (ModLoader.TryGetMod("ShopExpander", out var shopMod))
        {
            shopMod.Call("AddPageFromArray", "Cool Items", -2, items1);
            shopMod.Call("AddPageFromArray", "Even Cooler Items", -1, items2);
        }
        else
        {
            //If Shop Expander isn't loaded, fall back to vanilla
            var itemsToAdd = items1.Concat(items2);
            using var enumerator = itemsToAdd.GetEnumerator();

            foreach (ref Item item in items.AsSpan())
            {
                // Skip 'air' items and null items.
                if (item == null || item.type == ItemID.None)
                {
                    continue;
                }

                item = enumerator.Current;

                if (!enumerator.MoveNext())
                {
                    break;
                }
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
