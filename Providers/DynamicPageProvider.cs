namespace ShopExpander.Providers;

public class DynamicPageProvider : ArrayProvider
{
    private readonly Item[] _vanillaShop;

    public DynamicPageProvider(Item[] vanillaShop, string? name, int priority) : base(name, priority, Array.Empty<Item>())
    {
        ArgumentNullException.ThrowIfNull(vanillaShop);

        _vanillaShop = vanillaShop;
        FixNumPages();
    }

    public void Compose()
    {
        ExtendedItems = ExtendedItems.Concat(
                _vanillaShop.Where(x => !x.IsAir))
            .ToArray();

        FixNumPages();
    }
}
