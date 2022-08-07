using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace ShopExpander.Providers
{
    public interface IShopPageProvider
    {
        string Name { get; }
        int Priority { get; }
        int NumPages { get; }
        IEnumerable<Item> GetPage(int pageNum);
    }
}
