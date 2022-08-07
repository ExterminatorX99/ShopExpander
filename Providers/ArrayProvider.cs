namespace ShopExpander.Providers;

public class ArrayProvider : IShopPageProvider
{
    public Item[] ExtendedItems { get; protected set; }

    public ArrayProvider(string? name, int priority, Item[] items)
    {
        ArgumentNullException.ThrowIfNull(items);

        Name = name;
        Priority = priority;
        ExtendedItems = items;
        FixNumPages();
    }

    public string? Name { get; set; }
    public int Priority { get; set; }
    public int NumPages { get; private set; }

    public IEnumerable<Item> GetPage(int pageNum)
    {
        var offset = pageNum * 38;

        for (var i = 0; i < ShopAggregator.FrameCapacity; i++)
        {
            var sourceIndex = i + offset;
            if (sourceIndex >= ExtendedItems.Length)
            {
                yield break;
            }

            yield return ExtendedItems[sourceIndex];
        }
    }

    protected void FixNumPages()
    {
        if (ExtendedItems.Length > 0)
        {
            NumPages = ((ExtendedItems.Length - 1) / ShopAggregator.FrameCapacity) + 1;
        }
        else
        {
            NumPages = 0;
        }
    }
}
