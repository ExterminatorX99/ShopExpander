namespace ShopExpander.Patches;

using Providers;
using HookList = Terraria.ModLoader.Core.GlobalHookList<GlobalNPC>;

internal static class SetupShopPatch
{
    private static readonly FieldInfo HookModifyActiveShopFieldInfo = typeof(NPCLoader).GetField("HookModifyActiveShop", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static HookList _hookModifyActiveShop = null!;

    public static void Load()
    {
        _hookModifyActiveShop = (HookList)HookModifyActiveShopFieldInfo.GetValue(null)!;

        On_Chest.SetupShop_string_NPC += On_ChestOnSetupShop_string_NPC;
    }

    public static void Unload()
    {
        _hookModifyActiveShop = null!;

        On_Chest.SetupShop_string_NPC -= On_ChestOnSetupShop_string_NPC;
    }

    private static void On_ChestOnSetupShop_string_NPC(On_Chest.orig_SetupShop_string_NPC orig, Chest self, string shopName, NPC? npc)
    {
        ShopExpanderMod.ResetAndBindShop();

        if (npc != null)
        {
            // Ignore all shops from this npc
            if (ShopExpanderMod.NpcTypeIgnoreList.Contains(npc.type))
            {
                orig(self, shopName, npc);
                return;
            }

            if (ShopExpanderConfig.Instance.DisableShopPagingForNPCs.Any(x => x.Type == npc.type))
            {
                orig(self, shopName, npc);
                return;
            }

            // Ignore specific shop from this npc
            if (ShopExpanderMod.NpcShopIgnoreList.Contains((npc.type, shopName)))
            {
                orig(self, shopName, npc);
                return;
            }
        }

        var items = new List<Item?>();
        if (NPCShopDatabase.TryGetNPCShop(shopName, out var shop))
        {
            shop.FillShop(items, npc);
        }

        Item?[] itemsArray = npc != null
            ? ModifyActiveShop(npc, shopName, items)
            : items.ToArray();

        foreach (ref var item in itemsArray.AsSpan())
        {
            item ??= new Item();
            item.isAShopItem = true;
        }

        var dyn = new DynamicPageProvider(itemsArray!, null, ProviderPriority.Vanilla);

        dyn.Compose();

        ShopExpanderMod.ActiveShop.AddPage(dyn);
        ShopExpanderMod.ActiveShop.RefreshFrame();
    }

    private static Item?[] ModifyActiveShop(NPC npc, string shopName, List<Item?> items)
    {
        const int extraCapacity = Chest.maxItems;

        items.EnsureCapacity(items.Count + extraCapacity);
        for (var i = 0; i < extraCapacity; i++)
        {
            items.Add(null);
        }

        var shopContentsArray = items.ToArray();

        NPCLoader.GetNPC(npc.type)?.ModifyActiveShop(shopName, shopContentsArray);
        foreach (var g in _hookModifyActiveShop.Enumerate(npc))
        {
            g.ModifyActiveShop(npc, shopName, shopContentsArray);
        }

        return shopContentsArray;
    }
}
