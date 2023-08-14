namespace ShopExpander;

using Providers;

public class ShopAggregator
{
    public const int FrameCapacity = 38;

    private readonly List<IShopPageProvider> _pageProviders = new();
    private int _currPage;

    public Item[] CurrentFrame { get; }

    public ShopAggregator()
    {
        CurrentFrame = new Item[Chest.maxItems];
        for (var i = 0; i < Chest.maxItems; i++)
        {
            CurrentFrame[i] = new Item();
        }

        RefreshFrame();
    }

    public void AddPage(IShopPageProvider pageProvider)
    {
        _pageProviders.Add(pageProvider);
        _pageProviders.Sort((x, y) => x.Priority - y.Priority);
    }

    public void RefreshFrame()
    {
        var numPages = _pageProviders.Sum(x => x.NumPages);

        if (numPages == 0)
        {
            _currPage = 0;
            for (var i = 0; i < Chest.maxItems; i++)
            {
                CurrentFrame[i] = new Item();
            }

            return;
        }

        Debug.Assert(numPages > 0);

        _currPage = Math.Clamp(_currPage, 0, numPages - 1);

        var providerPageNum = _currPage;
        var providerIndex = 0;
        while (providerIndex < _pageProviders.Count && _pageProviders[providerIndex].NumPages <= providerPageNum)
        {
            providerPageNum -= _pageProviders[providerIndex].NumPages;
            providerIndex++;
        }

        if (providerPageNum >= _pageProviders[providerIndex].NumPages)
        {
            return;
        }

        var itemNum = 0;
        foreach (var item in _pageProviders[providerIndex].GetPage(providerPageNum))
        {
            CurrentFrame[itemNum + 1] = item;
            item.isAShopItem = true;
            itemNum++;
            if (itemNum > FrameCapacity)
            {
                break;
            }
        }

        for (var i = itemNum; i < FrameCapacity; i++)
        {
            CurrentFrame[i + 1] = new Item();
        }

        var prevPage = providerIndex;
        var prevPageNum = providerPageNum - 1;
        if (prevPageNum < 0)
        {
            prevPage--;
            while (prevPage >= 0 && _pageProviders[prevPage].NumPages <= 0)
            {
                prevPage--;
            }

            if (prevPage >= 0)
            {
                prevPageNum = _pageProviders[prevPage].NumPages - 1;
            }
        }

        if (prevPage >= 0)
        {
            CurrentFrame[0].SetDefaults(ShopExpanderMod.ArrowLeft.Item.type);
            CurrentFrame[0].ClearNameOverride();
            CurrentFrame[0].SetNameOverride(CurrentFrame[0].Name + GetPageHintText(_pageProviders[prevPage], prevPageNum));
        }
        else
        {
            CurrentFrame[0].SetDefaults();
            CurrentFrame[0].ClearNameOverride();
        }

        var nextPage = providerIndex;
        var nextPageNum = providerPageNum + 1;
        if (nextPageNum >= _pageProviders[nextPage].NumPages)
        {
            nextPage++;
            while (nextPage < _pageProviders.Count && _pageProviders[nextPage].NumPages <= 0)
            {
                nextPage++;
            }

            nextPageNum = 0;
        }

        if (nextPage < _pageProviders.Count)
        {
            CurrentFrame[Chest.maxItems - 1].SetDefaults(ShopExpanderMod.ArrowRight.Item.type);
            CurrentFrame[Chest.maxItems - 1].ClearNameOverride();
            CurrentFrame[Chest.maxItems - 1].SetNameOverride(CurrentFrame[Chest.maxItems - 1].Name + GetPageHintText(_pageProviders[nextPage], nextPageNum));
        }
        else
        {
            CurrentFrame[Chest.maxItems - 1].SetDefaults();
            CurrentFrame[Chest.maxItems - 1].ClearNameOverride();
        }
    }

    public IEnumerable<Item> GetAllItems()
    {
        foreach (var provider in _pageProviders)
        {
            for (var i = 0; i < provider.NumPages; i++)
            {
                foreach (var item in provider.GetPage(i))
                {
                    if (!item.IsAir)
                    {
                        yield return item;
                    }
                }
            }
        }
    }

    public void MoveLeft()
    {
        _currPage--;
        RefreshFrame();
    }

    public void MoveRight()
    {
        _currPage++;
        RefreshFrame();
    }

    public void MoveFirst()
    {
        _currPage = 0;
        RefreshFrame();
    }

    public void MoveLast()
    {
        _currPage = int.MaxValue;
        RefreshFrame();
    }

    private static string GetPageHintText(IShopPageProvider provider, int page)
    {
        if (provider.Name is null)
        {
            return "";
        }

        return provider.NumPages == 1
            ? $" ({provider.Name})"
            : $" ({provider.Name} {page}/{provider.NumPages})";
    }
}
