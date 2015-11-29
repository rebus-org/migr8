using System;
using System.Collections.Generic;
using System.Linq;

namespace Migr8.Test
{
    public static class TestEx
    {
        public static IEnumerable<TItem> InRandomOrder<TItem>(this IEnumerable<TItem> items)
        {
            var random = new Random(DateTime.Now.GetHashCode());
            var list = items.ToList();

            for (var count = 0; count < list.Count; count++)
            {
                var index1 = random.Next(list.Count);
                var index2 = random.Next(list.Count);

                if (index1 == index2) continue;

                var item1 = list[index1];
                var item2 = list[index2];

                list[index2] = item1;
                list[index1] = item2;
            }

            return list;
        }
    }
}