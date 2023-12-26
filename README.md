# Shop Expander

Shop expander aims to solve a problem encountered by many players using multiple large mods. Town NPCs have a limited amount of free shop inventory space. When multiple different mods add items to a single vanilla NPC's shop, it can overflow, causing any additional items to not show up. The issue is further compounded by the fact that some items might be added to the same shop by multiple different mods, resulting in wasted slots.

This mod solves the problem, by modifying tModLoader's shop setup process. It provides each mod with a new empty inventory to put items in, then combines the inventories. This new Expanded shop is then divided into pages for displaying. As a bonus, you also get a full 28 slot empty page for the buyback buffer.

For more game-play info visit the mod's home page on the Terraria Forums: https://forums.terraria.org/index.php?threads/shop-expander.78272/

## How it works (Outdated)

Shop Expander overrides `NPCLoader.SetupShop` to change how shops are created. The new method behaves similarly to the original, but every time `ModNPC.SetupShop` or `GlobalNPC.SetupShop` is called, a new empty chest is provided instead of the current shop. `nextSlot` will always be `0` and any changes to it are irrelevant. This chest becomes part of a new `ShoppingList` instance, which is stored in `ShopExpander.Instance.LastShopExpanded`. Since the game recreates the shop every time it is opened, it's only necessary to store the last shop created. After setting up the shop, the inventories in `ShoppingList` are aggregated by collecting every non-air item. At this point, all duplicate items are removed as well.

`ShoppingList` divides the new expanded shop inventory into multiple pages, each containing 38 items for sale. The active page is used to replace the original shop inventory passed to `NPCLoader.SetupShop`.

Shop Expander overrides `Chest.AddShop`, which is responsible for inserting items for buyback into an open shop. Every `ShoppingList` has a 38 slot page for storing these items, but the page is not displayed while empty.

The page selection buttons are created by adding two new items into the game. The left and right click events for these items are hooked via patches to `ItemSlot.LeftClick` and `ItemSlot.RightClick`.

## Limitations (Outdated)

There is obviously no way to know how a mod may choose to interact with a shop inventory. As long as the mod only modifies the shop as outlined on the [tModLoader documentation](http://blushiemagic.github.io/tModLoader/html/class_terraria_1_1_mod_loader_1_1_global_n_p_c.html#a5fd0754440bfc039de5425b200c202a1), this approach will work without issues.

> Add an item by setting the defaults of shop.item[nextSlot] then incrementing nextSlot. In the end, nextSlot must have a value of 1 greater than the highest index in shop.item that contains an item.

Some mods however might wish to modify or remove existing items in a shop. This wont be possible in the default case, but it can be done using Shop Expander's `Mod.Call` API. This use case seems to be rare enough, so this shouldn't effect many mods. Since mods can't assume the load order of Setup Shop hooks, without using more advanced tricks, in vanilla tModLoader it's only possible to modify vanilla shop items. Shop Expander makes this easier as well.

By default each mod is given a 40 slot chest for it's items. Since this is the size of a full vanilla shop, it should be more than enough. None the less, it's possible to request a larger inventory.

## Mod.Call

Shop expander provides some functions through `Mod.Call` that you can use in your mod without needing to add a reference to it.

| Call template | Effect |
| --- | --- |
| `Mod.Call("AddPageFromArray", string name, int priority, Item[] items)` | Returns `null`. |
| `Mod.Call("ResetAndBindShop")` | Clears the current shop and creates a new empty buyback buffer |
| `Mod.Call("GetLastShopExpanded")` | Returns the full contents of the last opened shop as an `Item[]`. May return `null` if no shop has been opened yet. |
| `Mod.Call("AddNpcTypeToIgnoreList", int npcType)` | Disables paging for shops from the npc with type `npcType` |
| `Mod.Call("AddNpcShopToIgnoreList", int npcType, string shopName)` | Disabled paging for the shop with `shopName` from the npc with type `npcType` |

You can see an example of these in [SillyDaftMod](SillyDaftMod/SillyDaftMod.cs). If you wish to add or modify items in the open shop outside of `SetupShop`, add Shop Expander as reference and use the members of `ShopExpander.Instance.LastShopExpanded`.
