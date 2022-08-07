namespace ShopExpander;

using Patches;
using Providers;

public class ShopExpander : Mod
{
    public static readonly LazyObjectConfig<int> ProvisionOverrides = new(40);
    public static readonly LazyObjectConfig<bool> ModifierOverrides = new();
    public static readonly LazyObjectConfig<bool> NoDistinctOverrides = new();
    public static readonly LazyObjectConfig<bool> IgnoreErrors = new();
    public static readonly LazyObjectConfig<bool> VanillaCopyOverrrides = new(true);
    public static readonly LazyObjectConfig<(string name, int priority, Action setup)[]> LegacyMultipageSetupMethods = new();

    private static bool textureSetupDone;

    public static ShopExpander Instance => ModContent.GetInstance<ShopExpander>();

    public static CircularBufferProvider Buyback { get; private set; }

    public static ModItem ArrowLeft { get; private set; }
    public static ModItem ArrowRight { get; private set; }

    public static ShopAggregator ActiveShop { get; internal set; }

    public static void ResetAndBindShop()
    {
        ActiveShop = new ShopAggregator();
        ActiveShop.AddPage(Buyback = new("Buyback", ProviderPriority.Buyback));
        Main.instance.shop[Main.npcShop].item = ActiveShop.CurrentFrame;
    }

    public override void Load()
    {
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

        ArrowLeft.DisplayName.SetDefault("Previous page");
        ArrowRight.DisplayName.SetDefault("Next page");

        if (!Main.dedServ)
        {
            Main.RunOnMainThread(() =>
                {
                    TextureAssets.Item[ArrowLeft.Item.type] = TextureAsset(CropTexture(TextureAssets.TextGlyph[0].Value, new Rectangle(4 * 28, 0, 28, 28)));
                    TextureAssets.Item[ArrowRight.Item.type] = TextureAsset(CropTexture(TextureAssets.TextGlyph[0].Value, new Rectangle(5 * 28, 0, 28, 28)));
                    textureSetupDone = true;
                })
                .GetAwaiter()
                .GetResult(); // Use this instead of 'Wait()' so stack trace is more useful
        }
    }

    public override void Unload()
    {
        SetupShopPatch.Unload();

        if (textureSetupDone)
        {
            Main.RunOnMainThread(() =>
                {
                    TextureAssets.Item[ArrowLeft.Item.type].Value.Dispose();
                    TextureAssets.Item[ArrowRight.Item.type].Value.Dispose();
                })
                .GetAwaiter()
                .GetResult(); // Use this instead of 'Wait()' so stack trace is more useful
        }
    }

    public override object Call(params object[] args)
    {
        var command = args[0] as string;
        if (command == null)
        {
            throw new ArgumentException("first argument must be string");
        }

        switch (command)
        {
            case CallApi.SetProvisionSize:
                ProvisionOverrides.SetValue(args[1], AssertAndCast<int>(args, 2, CallApi.SetProvisionSize));
                break;

            case CallApi.SetModifier:
                ModifierOverrides.SetValue(args[1], true);
                break;

            case CallApi.SetNoDistinct:
                NoDistinctOverrides.SetValue(args[1], true);
                break;

            case CallApi.SetVanillaNoCopy:
                VanillaCopyOverrrides.SetValue(args[1], false);
                break;

            case CallApi.AddLegacyMultipageSetupMethods:
                if (args.Length % 3 != 2)
                {
                    throw new ArgumentException("The number of arguments is incorrect (args.Length % 3 != 1) for " + CallApi.AddLegacyMultipageSetupMethods);
                }

                var methods = new (string name, int priority, Action setup)[args.Length / 3];
                for (var i = 0; i < methods.Length; i++)
                {
                    var offset = (i * 3) + 2;
                    methods[i].name = AssertAndCast<string>(args, offset, CallApi.AddLegacyMultipageSetupMethods);
                    methods[i].priority = AssertAndCast<int>(args, offset + 1, CallApi.AddLegacyMultipageSetupMethods);
                    methods[i].setup = AssertAndCast<Action>(args, offset + 2, CallApi.AddLegacyMultipageSetupMethods);
                }

                LegacyMultipageSetupMethods.SetValue(args[1], methods);
                break;

            case CallApi.AddPageFromArray:
                if (ActiveShop == null)
                {
                    throw new InvalidOperationException($"No active shop, try calling {CallApi.ResetAndBindShop} first");
                }

                ActiveShop.AddPage(new ArrayProvider(AssertAndCast<string>(args, 1, CallApi.AddPageFromArray),
                    AssertAndCast<int>(args, 2, CallApi.AddPageFromArray),
                    AssertAndCast<Item[]>(args, 3, CallApi.AddPageFromArray)));
                break;

            case CallApi.ResetAndBindShop:
                ResetAndBindShop();
                break;

            case CallApi.GetLastShopExpanded:
                if (ActiveShop != null)
                {
                    return ActiveShop.GetAllItems().ToArray();
                }

                break;

            default:
                throw new ArgumentException($"Unknown command: {command}");
        }

        return null;
    }

    private static T AssertAndCast<T>(object[] args, int index, string site, bool checkForNull = false)
    {
        if (checkForNull && args[index] == null)
        {
            throw new ArgumentNullException($"args[{index}] cannot be null for {site}");
        }

        if (args[index] is not T casted)
        {
            throw new ArgumentException($"args[{index}] must be {typeof(T).Name} for {site}");
        }

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
