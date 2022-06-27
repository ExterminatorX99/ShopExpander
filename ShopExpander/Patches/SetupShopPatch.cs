using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour.HookGen;
using ShopExpander.Providers;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace ShopExpander.Patches
{
    internal static class SetupShopPatch
    {
        private delegate void orig_SetupShop(int type, Chest shop, ref int nextSlot);

        private delegate void hook_SetupShop(orig_SetupShop orig, int type, Chest shop, ref int nextSlot);

        private static GlobalNPC[] arr;
        private static int[] shopToNpcs;

        private const int maxProvisionTries = 3;

        public static void Load()
        {
            object hooklist = typeof(NPCLoader).GetField("HookSetupShop", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            arr = (GlobalNPC[])hooklist.GetType().GetField("arr", BindingFlags.Public | BindingFlags.Instance).GetValue(hooklist);
            shopToNpcs = (int[])typeof(NPCLoader).GetField("shopToNPC", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

            HookEndpointManager.Add(typeof(NPCLoader).GetMethod("SetupShop", BindingFlags.Public | BindingFlags.Static), (hook_SetupShop)Prefix);
        }

        public static void Unload()
        {
            arr = null;
            shopToNpcs = null;
        }

        private static void Prefix(orig_SetupShop orig, int type, Chest shop, ref int nextSlot)
        {
            ShopExpander.Instance.ResetAndBindShop();
            DynamicPageProvider dyn = new DynamicPageProvider(shop.item, null, ProviderPriority.Vanilla);
            List<GlobalNPC> modifiers = new List<GlobalNPC>();

            if (type < shopToNpcs.Length)
            {
                type = shopToNpcs[type];
            }
            else
            {
                ModNPC npc = NPCLoader.GetNPC(type);
                if (npc != null)
                {
                    DoSetupFor(shop, dyn, "ModNPC", npc.Mod, npc, delegate (Chest c)
                    {
                        int zero = 0;
                        npc.SetupShop(c, ref zero);
                    });
                }
            }

            foreach (GlobalNPC globalNPC in arr)
            {
                if (ShopExpander.Instance.ModifierOverrides.GetValue(globalNPC))
                {
                    modifiers.Add(globalNPC);
                }
                else
                {
                    DoSetupFor(shop, dyn, "GloabalNPC", globalNPC.Mod, globalNPC, delegate (Chest c)
                    {
                        int zero = 0;
                        globalNPC.SetupShop(type, c, ref zero);
                    });
                }
            }

            dyn.Compose();

            foreach (var item in modifiers)
            {
                try
                {
                    int max = dyn.ExtendedItems.Length;
                    item.SetupShop(type, MakeFakeChest(dyn.ExtendedItems), ref max);
                }
                catch (Exception e)
                {
                    LogAndPrint("modifier GlobalNPC", item.Mod, item, e);
                }
            }

            ShopExpander.Instance.ActiveShop.AddPage(dyn);
            ShopExpander.Instance.ActiveShop.RefreshFrame();
        }

        private static void DoSetupFor(Chest shop, DynamicPageProvider mainDyn, string typeText, Mod mod, object obj, Action<Chest> setup)
        {
            try
            {
                var methods = ShopExpander.Instance.LegacyMultipageSetupMethods.GetValue(obj);
                if (methods != null)
                {
                    foreach (var item in methods)
                    {
                        DynamicPageProvider dynPage = new DynamicPageProvider(shop.item, item.name, item.priority);
                        ShopExpander.Instance.ActiveShop.AddPage(dynPage);
                        item.setup?.Invoke();
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
            int sizeToTry = ShopExpander.Instance.ProvisionOverrides.GetValue(obj);
            int numMoreTries = maxProvisionTries;
            List<Exception> exceptions = new List<Exception>(maxProvisionTries);

            bool retry = true;
            while (retry)
            {
                retry = false;
                Chest provision = null;
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
                            dyn.UnProvision(provision.item);
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
            Chest fake = new Chest(false);
            fake.item = items;
            return fake;
        }

        private static Chest ProvisionChest(DynamicPageProvider dyn, object target, int size)
        {
            return MakeFakeChest(dyn.Provision(size, ShopExpander.Instance.NoDistinctOverrides.GetValue(target), ShopExpander.Instance.VanillaCopyOverrrides.GetValue(target)));
        }

        private static void LogAndPrint(string type, Mod mod, object obj, Exception e)
        {
            if (ShopExpander.Instance.IgnoreErrors.GetValue(obj))
                return;
            ShopExpander.Instance.IgnoreErrors.SetValue(obj, true);

            string modName = "N/A";
            if (mod != null && mod.DisplayName != null)
                modName = mod.DisplayName;
            string message = string.Format("Shop Expander failed to load {0} from mod {1}.", type, modName);
            Main.NewText(message, Color.Red);
            Main.NewText("See log for more info. If this error persists, please consider reporting it to the author of the mod mentioned above.", Color.Red);
            ErrorLogger.Log("--- SHOP EXPANDER ERROR ---");
            ErrorLogger.Log(message);
            ErrorLogger.Log(e.ToString());
            ErrorLogger.Log("--- END SHOP EXPANDER ERROR ---");
        }
    }
}