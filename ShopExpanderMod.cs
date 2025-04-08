namespace ShopExpander;

using Patches;
using Providers;
using Terraria.Localization;

// ReSharper disable once ClassNeverInstantiated.Global
public class ShopExpanderMod : Mod
{
    private static bool _textureSetupDone;

    public static CircularBufferProvider Buyback { get; private set; } = null!;

    public static ModItem ArrowLeft { get; private set; } = null!;
    public static ModItem ArrowRight { get; private set; } = null!;

    public static ShopAggregator? ActiveShop { get; private set; }

    // Ignore all shops from this npc
    internal static HashSet<int> NpcTypeIgnoreList { get; private set; } = null!;

    // Ignore specific shop from this npc
    internal static HashSet<(int Type, string ShopName)> NpcShopIgnoreList { get; private set; } = null!;

    [MemberNotNull(nameof(ActiveShop))]
    public static void ResetAndBindShop()
    {
        ActiveShop = new ShopAggregator();
        ActiveShop.AddPage(Buyback = new(Language.GetTextValue("Mods.ShopExpander.Items.Buyback"), ProviderPriority.Buyback));
        Main.instance.shop[Main.npcShop].item = ActiveShop.CurrentFrame;
    }

    public override void Load()
    {
        NpcTypeIgnoreList = new();
        NpcShopIgnoreList = new();

        ArrowLeft = new ArrowItem("ArrowLeft");
        AddContent(ArrowLeft);

        ArrowRight = new ArrowItem("ArrowRight");
        AddContent(ArrowRight);
    }

    public override void PostSetupContent()
    {
        SetupShopPatch.Load();
        AddShopPatch.Load();
        LeftRightClickPatch.Load();

        if (!Main.dedServ)
        {
            Main.QueueMainThreadAction(() =>
            {
                TextureAssets.Item[ArrowLeft.Item.type] = TextureAsset(CropTexture(TextureAssets.TextGlyph[0].Value, new Rectangle(4 * 28, 0, 28, 28)));
                TextureAssets.Item[ArrowRight.Item.type] = TextureAsset(CropTexture(TextureAssets.TextGlyph[0].Value, new Rectangle(5 * 28, 0, 28, 28)));
                _textureSetupDone = true;
            });
        }
    }

    public override void Unload()
    {
        NpcTypeIgnoreList = null!;
        NpcShopIgnoreList = null!;

        SetupShopPatch.Unload();

        Main.QueueMainThreadAction(() =>
        {
            if (!_textureSetupDone)
            {
                return;
            }

            TextureAssets.Item[ArrowLeft.Item.type].Value.Dispose();
            TextureAssets.Item[ArrowRight.Item.type].Value.Dispose();
        });
    }

    public override object? Call(params object[] args)
    {
        if (args[0] is not string command)
        {
            throw new ArgumentException("first argument must be string");
        }

        int argNum = 1;

        switch (command)
        {
            case CallApi.AddPageFromArray:
            {
                if (ActiveShop == null)
                {
                    throw new InvalidOperationException($"No active shop, try calling {CallApi.ResetAndBindShop} first");
                }

                var name = AssertAndCast<string>(args, ref argNum, CallApi.AddPageFromArray);
                var priority = AssertAndCast<int>(args, ref argNum, CallApi.AddPageFromArray);
                var items = AssertAndCast<Item[]>(args, ref argNum, CallApi.AddPageFromArray);

                ActiveShop.AddPage(new ArrayProvider(name, priority, items));
                break;
            }
            case CallApi.ResetAndBindShop:
            {
                ResetAndBindShop();
                break;
            }
            case CallApi.GetLastShopExpanded:
            {
                return ActiveShop?.GetAllItems().ToArray();
            }
            case CallApi.AddNpcTypeToIgnoreList:
            {
                int npcType = AssertAndCast<int>(args, ref argNum, CallApi.AddNpcTypeToIgnoreList);

                return NpcTypeIgnoreList.Add(npcType);
            }
            case CallApi.AddNpcShopToIgnoreList:
            {
                int npcType = AssertAndCast<int>(args, ref argNum, CallApi.AddNpcShopToIgnoreList);
                string shopName = AssertAndCast<string>(args, ref argNum, CallApi.AddNpcShopToIgnoreList);

                return NpcShopIgnoreList.Add((npcType, shopName));
            }
            default:
            {
                throw new ArgumentException($"Unknown command: {command}");
            }
        }

        return null;
    }

    private static T AssertAndCast<T>(object[] args, ref int index, string site)
    {
        if (args[index] is not T casted)
        {
            throw new ArgumentException($"args[{index}] must be {typeof(T).Name} for {site}");
        }

        index++;

        return casted;
    }

    private static Texture2D CropTexture(Texture2D texture, Rectangle newBounds)
    {
        var newTexture = new Texture2D(Main.graphics.GraphicsDevice, newBounds.Width, newBounds.Height);
        var area = newBounds.Width * newBounds.Height;
        var data = new Color[area];

        texture.GetData(0, newBounds, data, 0, area);
        newTexture.SetData(data);

        return newTexture;
    }

    private Asset<Texture2D> TextureAsset(Texture2D texture)
    {
        using MemoryStream stream = new(texture.Width * texture.Height);

        texture.SaveAsPng(stream, texture.Width, texture.Height);
        stream.Position = 0;

        return Assets.CreateUntracked<Texture2D>(stream, ".png");
    }
}
