using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using ShopExpander.Providers;

namespace ShopExpander
{
    public class ShopAggregator
    {
        public const int FrameCapacity = 38;

        public Item[] CurrentFrame { get; private set; }

        private readonly List<IShopPageProvider> pageProviders = new List<IShopPageProvider>();
        private int currPage = 0;

        public ShopAggregator()
        {
            CurrentFrame = new Item[Chest.maxItems];
            for (int i = 0; i < Chest.maxItems; i++)
            {
                CurrentFrame[i] = new Item();
            }
            RefreshFrame();
        }

        public void AddPage(IShopPageProvider pageProvider)
        {
            pageProviders.Add(pageProvider);
            pageProviders.Sort((x, y) => x.Priority - y.Priority);
        }

        public void RefreshFrame()
        {
            int numPages = pageProviders.Sum(x => x.NumPages);

            if (numPages == 0)
            {
                currPage = 0;
                for (int i = 0; i < Chest.maxItems; i++)
                {
                    CurrentFrame[i] = new Item();
                }
                return;
            }

            if (currPage < 0)
                currPage = 0;

            if (currPage >= numPages)
                currPage = numPages - 1;

            int providerPageNum = currPage;
            int providerIndex = 0;
            while (providerIndex < pageProviders.Count && pageProviders[providerIndex].NumPages <= providerPageNum)
            {
                providerPageNum -= pageProviders[providerIndex].NumPages;
                providerIndex++;
            }

            if (providerPageNum >= pageProviders[providerIndex].NumPages)
                return;

            int itemNum = 0;
            foreach (var item in pageProviders[providerIndex].GetPage(providerPageNum))
            {
                CurrentFrame[itemNum + 1] = item;
                item.isAShopItem = true;
                itemNum++;
                if (itemNum > FrameCapacity)
                    break;
            }

            for (int i = itemNum; i < FrameCapacity; i++)
            {
                CurrentFrame[i + 1] = new Item();
            }

            int prevPage = providerIndex;
            int prevPageNum = providerPageNum - 1;
            if (prevPageNum < 0)
            {
                prevPage--;
                while (prevPage >= 0 && pageProviders[prevPage].NumPages <= 0)
                {
                    prevPage--;
                }

                if (prevPage >= 0)
                {
                    prevPageNum = pageProviders[prevPage].NumPages - 1;
                }
            }

            if (prevPage >= 0)
            {
                CurrentFrame[0].SetDefaults(ShopExpander.Instance.ArrowLeft.Item.type);
                CurrentFrame[0].ClearNameOverride();
                CurrentFrame[0].SetNameOverride(CurrentFrame[0].Name + GetPageHintText(pageProviders[prevPage], prevPageNum));
            }
            else
            {
                CurrentFrame[0].SetDefaults(0);
                CurrentFrame[0].ClearNameOverride();
            }

            int nextPage = providerIndex;
            int nextPageNum = providerPageNum + 1;
            if (nextPageNum >= pageProviders[nextPage].NumPages)
            {
                nextPage++;
                while (nextPage < pageProviders.Count && pageProviders[nextPage].NumPages <= 0)
                {
                    nextPage++;
                }

                nextPageNum = 0;
            }

            if (nextPage < pageProviders.Count)
            {
                CurrentFrame[Chest.maxItems - 1].SetDefaults(ShopExpander.Instance.ArrowRight.Item.type);
                CurrentFrame[Chest.maxItems - 1].ClearNameOverride();
                CurrentFrame[Chest.maxItems - 1].SetNameOverride(CurrentFrame[Chest.maxItems - 1].Name + GetPageHintText(pageProviders[nextPage], nextPageNum));
            }
            else
            {
                CurrentFrame[Chest.maxItems - 1].SetDefaults(0);
                CurrentFrame[Chest.maxItems - 1].ClearNameOverride();
            }
        }

        public IEnumerable<Item> GetAllItems()
        {
            foreach (var provider in pageProviders)
            {
                for (int i = 0; i < provider.NumPages; i++)
                {
                    foreach (var item in provider.GetPage(i))
                    {
                        if (!item.IsAir)
                            yield return item;
                    }
                }
            }
        }

        public void MoveLeft()
        {
            currPage--;
            RefreshFrame();
        }

        public void MoveRight()
        {
            currPage++;
            RefreshFrame();
        }

        public void MoveFirst()
        {
            currPage = 0;
            RefreshFrame();
        }

        public void MoveLast()
        {
            currPage = int.MaxValue;
            RefreshFrame();
        }

        private string GetPageHintText(IShopPageProvider provider, int page)
        {
            if (provider.Name == null)
                return "";

            if (provider.NumPages == 1)
                return string.Format(" ({0})", provider.Name);
            else
                return string.Format(" ({0} {1}/{2})", provider.Name, page, provider.NumPages);
        }
    }
}
