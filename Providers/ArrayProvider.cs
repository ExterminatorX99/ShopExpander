using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace ShopExpander.Providers
{
    public class ArrayProvider : IShopPageProvider
    {
        public string Name { get; set; }
        public int Priority { get; set; }
        public int NumPages { get; private set; }

        public Item[] ExtendedItems { get; protected set; }

        public ArrayProvider(string name, int priority, Item[] items)
        {
            Name = name;
            Priority = priority;
            ExtendedItems = items;
            FixNumPages();
        }

        public IEnumerable<Item> GetPage(int pageNum)
        {
            int offset = pageNum * 38;

            for (int i = 0; i < ShopAggregator.FrameCapacity; i++)
            {
                int sourceIndex = i + offset;
                if (sourceIndex >= ExtendedItems.Length)
                    yield break;
                yield return ExtendedItems[sourceIndex];
            }
        }

        protected void FixNumPages()
        {
            if (ExtendedItems.Length > 0)
                NumPages = (ExtendedItems.Length - 1) / ShopAggregator.FrameCapacity + 1;
            else
                NumPages = 0;
        }
    }
}
