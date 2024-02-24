namespace ShopExpander;

using Terraria.ModLoader.Config;

// ReSharper disable once ClassNeverInstantiated.Global
public class ShopExpanderConfig : ModConfig
{
    public static ShopExpanderConfig Instance => ModContent.GetInstance<ShopExpanderConfig>();

    public override ConfigScope Mode => ConfigScope.ClientSide;

    // ReSharper disable once CollectionNeverUpdated.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public List<NPCDefinition> DisableShopPagingForNPCs = [];
}
