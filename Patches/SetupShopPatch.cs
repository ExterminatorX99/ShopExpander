using HookList = Terraria.ModLoader.Core.HookList<Terraria.ModLoader.GlobalNPC>;

namespace ShopExpander.Patches;

using MonoMod.RuntimeDetour.HookGen;
using Providers;

internal static class SetupShopPatch
{
    public delegate void orig_SetupShop(int type, Chest shop, ref int nextSlot);

    public delegate void hook_SetupShop(orig_SetupShop orig, int type, Chest shop, ref int nextSlot);

    private const int maxProvisionTries = 3;

    private static readonly FieldInfo HookSetupShopFieldInfo = typeof(NPCLoader).GetField("HookSetupShop", BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly FieldInfo GlobalNPCsFieldInfo = typeof(NPCLoader).GetField("globalNPCs", BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly FieldInfo ShopToNPCFieldInfo = typeof(NPCLoader).GetField("shopToNPC", BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly MethodInfo SetupShopMethodInfo = typeof(NPCLoader).GetMethod(nameof(NPCLoader.SetupShop), BindingFlags.Public | BindingFlags.Static)!;

    private static HookList HookSetupShop = null!;
    private static List<GlobalNPC> globalNPCsArray = null!;
    private static int[] shopToNpcs = null!;

    public static void Load()
    {
        HookSetupShop = (HookList)HookSetupShopFieldInfo.GetValue(null)!;
        globalNPCsArray = (List<GlobalNPC>)GlobalNPCsFieldInfo.GetValue(null)!;
        shopToNpcs = (int[])ShopToNPCFieldInfo.GetValue(null)!;

        HookEndpointManager.Add(SetupShopMethodInfo, (hook_SetupShop)Prefix);
    }

    public static void Unload()
    {
        HookSetupShop = null!;
        globalNPCsArray = null!;
        shopToNpcs = null!;

        HookEndpointManager.Remove(SetupShopMethodInfo, (hook_SetupShop)Prefix);
    }

    private static void Prefix(orig_SetupShop orig, int type, Chest shop, ref int nextSlot)
    {
        var vanillaShop = shop.item;
        ShopExpanderMod.ResetAndBindShop();
        var dyn = new DynamicPageProvider(vanillaShop, null, ProviderPriority.Vanilla);
        var modifiers = new List<GlobalNPC>();

        if (type < shopToNpcs.Length)
        {
            type = shopToNpcs[type];
        }
        else
        {
            var npc = NPCLoader.GetNPC(type);
            if (npc != null)
            {
                DoSetupFor(shop, dyn, "ModNPC", npc.Mod, npc, delegate(Chest c)
                {
                    var zero = 0;
                    npc.SetupShop(c, ref zero);
                });
            }
        }

        foreach (var globalNPC in HookSetupShop.Enumerate(globalNPCsArray))
        {
            if (ShopExpanderMod.ModifierOverrides.GetValue(globalNPC))
            {
                modifiers.Add(globalNPC);
            }
            else
            {
                DoSetupFor(shop, dyn, "GloabalNPC", globalNPC.Mod, globalNPC, delegate(Chest c)
                {
                    var zero = 0;
                    globalNPC.SetupShop(type, c, ref zero);
                });
            }
        }

        dyn.Compose();

        foreach (var item in modifiers)
        {
            try
            {
                var max = dyn.ExtendedItems.Length;
                item.SetupShop(type, MakeFakeChest(dyn.ExtendedItems), ref max);
            }
            catch (Exception e)
            {
                LogAndPrint("modifier GlobalNPC", item.Mod, item, e);
            }
        }

        ShopExpanderMod.ActiveShop.AddPage(dyn);
        ShopExpanderMod.ActiveShop.RefreshFrame();
    }

    private static void DoSetupFor(Chest shop, DynamicPageProvider mainDyn, string typeText, Mod mod, object obj, Action<Chest> setup)
    {
        ArgumentNullException.ThrowIfNull(shop);
        ArgumentNullException.ThrowIfNull(mainDyn);
        ArgumentNullException.ThrowIfNull(typeText);
        ArgumentNullException.ThrowIfNull(mod);
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(setup);

        Debug.Assert(ShopExpanderMod.ActiveShop is not null);

        try
        {
            var methods = ShopExpanderMod.LegacyMultipageSetupMethods.GetValue(obj);
            if (methods != null)
            {
                foreach (var (name, priority, action) in methods)
                {
                    var dynPage = new DynamicPageProvider(shop.item, name, priority);
                    ShopExpanderMod.ActiveShop.AddPage(dynPage);

                    action?.Invoke();
                    DoSetupSingle(dynPage, obj, setup);

                    dynPage.Compose();
                }
            }
            else
            {
                DoSetupSingle(mainDyn, obj, setup);
            }
        }
        catch (Exception e)
        {
            LogAndPrint(typeText, mod, obj, e);
        }
    }

    private static void DoSetupSingle(DynamicPageProvider dyn, object obj, Action<Chest> setup)
    {
        var sizeToTry = ShopExpanderMod.ProvisionOverrides.GetValue(obj);
        var numMoreTries = maxProvisionTries;
        var exceptions = new List<Exception>(maxProvisionTries);

        var retry = true;
        while (retry)
        {
            retry = false;
            Chest? provision = null;
            try
            {
                provision = ProvisionChest(dyn, obj, sizeToTry);
                setup(provision);
            }
            catch (IndexOutOfRangeException e)
            {
                exceptions.Add(e);
                if (--numMoreTries > 0)
                {
                    retry = true;
                    if (provision != null)
                    {
                        dyn.UnProvision(provision.item);
                    }

                    sizeToTry *= 2;
                }
                else
                {
                    throw new AggregateException("Failed setup after trying with " + sizeToTry + " slots", exceptions);
                }
            }
        }
    }

    private static Chest MakeFakeChest(Item[] items)
    {
        var fake = new Chest();
        fake.item = items;
        return fake;
    }

    private static Chest ProvisionChest(DynamicPageProvider dyn, object target, int size)
    {
        return MakeFakeChest(dyn.Provision(size, ShopExpanderMod.NoDistinctOverrides.GetValue(target), ShopExpanderMod.VanillaCopyOverrrides.GetValue(target)));
    }

    private static void LogAndPrint(string type, Mod? mod, object obj, Exception e)
    {
        if (ShopExpanderMod.IgnoreErrors.GetValue(obj))
        {
            return;
        }

        ShopExpanderMod.IgnoreErrors.SetValue(obj, true);

        var modName = "N/A";
        if (mod is { DisplayName: { } displayName })
        {
            modName = displayName;
        }

        var message = $"Shop Expander failed to load {type} from mod {modName}.";
        Main.NewText(message, Color.Red);
        Main.NewText("See log for more info. If this error persists, please consider reporting it to the author of the mod mentioned above.", Color.Red);
        var logger = ShopExpanderMod.Instance.Logger;
        logger.Error("--- SHOP EXPANDER ERROR ---");
        logger.Error(message);
        logger.Error(e.ToString());
        logger.Error("--- END SHOP EXPANDER ERROR ---");
    }
}
