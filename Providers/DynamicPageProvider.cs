using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace ShopExpander.Providers
{
    public class DynamicPageProvider : ArrayProvider
    {
        private struct ProvisionedSegment
        {
            public Item[] items;
            public bool noDistinct;

            public ProvisionedSegment(int size, bool noDistinct)
            {
                items = new Item[size];
                for (int i = 0; i < size; i++)
                {
                    items[i] = new Item();
                }
                this.noDistinct = noDistinct;
            }
        }

        private class ItemSameType : IEqualityComparer<Item>
        {
            public bool Equals(Item x, Item y)
            {
                return x.Name == y.Name && x.type == y.type;
            }

            public int GetHashCode(Item obj)
            {
                return obj.Name.GetHashCode() ^ obj.type.GetHashCode();
            }
        }

        private readonly List<ProvisionedSegment> provisions = new List<ProvisionedSegment>();
        private readonly Item[] vanillaShop;
        private readonly Item[] vanillaShopCopy;

        public DynamicPageProvider(string name, int priority) : this(new Item[0], name, priority) { }

        public DynamicPageProvider(Item[] vanillaShop, string name, int priority) : base(name, priority, new Item[0])
        {
            this.vanillaShop = vanillaShop;
            vanillaShopCopy = vanillaShop.Where(x => !x.IsAir).Select(x => x.Clone()).ToArray();
            FixNumPages();
        }

        public Item[] Provision(int capacity, bool noDistinct, bool vanillaCopy)
        {
            ProvisionedSegment items = new ProvisionedSegment(capacity, noDistinct);
            if (vanillaCopy)
            {
                for (int i = 0; i < vanillaShopCopy.Length; i++)
                {
                    items.items[i] = vanillaShopCopy[i].Clone();
                }
            }
            provisions.Add(items);
            return items.items;
        }

        public void UnProvision(Item[] items)
        {
            provisions.RemoveAll(x => x.items == items);
        }

        public void Compose()
        {
            ExtendedItems = ExtendedItems.Concat(
                                vanillaShop.Where(x => !x.IsAir))
                            .Concat(
                                provisions.Where(x => !x.noDistinct)
                                .SelectMany(x => x.items.Where(y => !y.IsAir)))
                            .Distinct(new ItemSameType())
                            .Concat(
                                provisions.Where(x => x.noDistinct)
                                .SelectMany(x => x.items.Where(y => !y.IsAir)))
                            .ToArray();

            FixNumPages();
            provisions.Clear();
        }
    }
}
