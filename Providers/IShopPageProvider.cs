namespace ShopExpander.Providers;

public interface IShopPageProvider
{
    string? Name { get; }

    int Priority { get; }

    int NumPages { get; }

    IEnumerable<Item> GetPage(int pageNum);
}
