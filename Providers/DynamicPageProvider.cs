namespace ShopExpander.Providers;

public class DynamicPageProvider : ArrayProvider
{
    private readonly List<ProvisionedSegment> _provisions = new();
    private readonly Item[] _vanillaShop;
    private readonly Item[] _vanillaShopCopy;

    public DynamicPageProvider(string? name, int priority) : this(Array.Empty<Item>(), name, priority) { }

    public DynamicPageProvider(Item[] vanillaShop, string? name, int priority) : base(name, priority, Array.Empty<Item>())
    {
        ArgumentNullException.ThrowIfNull(vanillaShop);

        _vanillaShop = vanillaShop;
        _vanillaShopCopy = vanillaShop.Where(x => !x.IsAir).Select(x => x.Clone()).ToArray();
        FixNumPages();
    }

    public Item[] Provision(int capacity, bool noDistinct, bool vanillaCopy)
    {
        var items = new ProvisionedSegment(capacity, noDistinct);
        if (vanillaCopy)
        {
            for (var i = 0; i < _vanillaShopCopy.Length; i++)
            {
                items.Items[i] = _vanillaShopCopy[i].Clone();
            }
        }

        _provisions.Add(items);
        return items.Items;
    }

    public void UnProvision(Item[] items)
    {
        _provisions.RemoveAll(x => x.Items == items);
    }

    public void Compose()
    {
        ExtendedItems = ExtendedItems.Concat(
                _vanillaShop.Where(x => !x.IsAir))
            .Concat(
                _provisions.Where(x => !x.NoDistinct)
                    .SelectMany(x => x.Items.Where(y => !y.IsAir)))
            .Distinct(new ItemSameType())
            .Concat(
                _provisions.Where(x => x.NoDistinct)
                    .SelectMany(x => x.Items.Where(y => !y.IsAir)))
            .ToArray();

        FixNumPages();
        _provisions.Clear();
    }

    private class ProvisionedSegment
    {
        public Item[] Items { get; }
        public bool NoDistinct { get; }

        public ProvisionedSegment(int size, bool noDistinct)
        {
            Items = new Item[size];
            for (var i = 0; i < size; i++)
            {
                Items[i] = new Item();
            }

            NoDistinct = noDistinct;
        }
    }

    private class ItemSameType : IEqualityComparer<Item>
    {
        public bool Equals(Item? x, Item? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.type == y.type && x.Name == y.Name;
        }

        public int GetHashCode(Item obj)
        {
            return HashCode.Combine(obj.type, obj.Name);
        }
    }
}
