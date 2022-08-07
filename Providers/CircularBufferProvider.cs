namespace ShopExpander.Providers;

public class CircularBufferProvider : IShopPageProvider
{
    private readonly Item[] items = new Item[ShopAggregator.FrameCapacity];
    private int nextSlot;
    private bool show;

    public CircularBufferProvider(string name, int priority)
    {
        ArgumentNullException.ThrowIfNull(name);

        Name = name;
        Priority = priority;
        for (var i = 0; i < items.Length; i++)
        {
            items[i] = new Item();
        }
    }

    public string Name { get; set; }
    public int Priority { get; set; }
    public int NumPages => show ? 1 : 0;

    public IEnumerable<Item> GetPage(int pageNum)
    {
        return items;
    }

    public void AddItem(Item item)
    {
        show = true;

        for (var i = 0; i < items.Length; i++)
        {
            if (items[i].IsAir)
            {
                items[i] = item;
                nextSlot = 0;
                return;
            }
        }

        items[nextSlot++] = item;
        if (nextSlot >= items.Length)
        {
            nextSlot = 0;
        }
    }
}
