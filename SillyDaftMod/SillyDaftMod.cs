namespace SillyDaftMod;

using Terraria.ModLoader;

public class SillyDaftMod : Mod
{
    public override void PostSetupContent()
    {
        if (ModLoader.TryGetMod("ShopExpander", out var shopExpander))
        {
            shopExpander.Call("SetProvisionSize", ModContent.GetInstance<SillyDaftGlobalNpc>(), ItemLoader.ItemCount * 2);
            //shopExpander.Call("SetNoDistinct", GetGlobalNPC<SillyDaftGlobalNpc>());

            shopExpander.Call("SetModifier", ModContent.GetInstance<FreeloadGlobalNpc>());
        }
    }
}
