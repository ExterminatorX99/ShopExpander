using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace ShopExpander.Providers
{
    public class CircularBufferProvider : IShopPageProvider
    {
        private readonly Item[] items = new Item[ShopAggregator.FrameCapacity];
        private int nextSlot = 0;
        bool show = false;

        public string Name { get; set; }
        public int Priority { get; set; }
        public int NumPages { get { return show ? 1 : 0; } }

        public CircularBufferProvider(string name, int priority)
        {
            Name = name;
            Priority = priority;
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new Item();
            }
        }

        public IEnumerable<Item> GetPage(int pageNum)
        {
            return items;
        }

        public void AddItem(Item item)
        {
            show = true;

            for (int i = 0; i < items.Length; i++)
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
                nextSlot = 0;
        }
    }
}
