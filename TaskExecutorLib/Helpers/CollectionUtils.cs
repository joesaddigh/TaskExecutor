using System.Collections.Generic;
using System.Linq;

namespace TaskExecutorLib.Helpers
{
    public static class CollectionUtils
    {
        public static T SafeGetItemByIndex<T>(int index, IEnumerable<T> collection)
        {
            var item = default(T);

            if (collection != null)
            {
                if (index < collection.Count())
                {
                    item = collection.ElementAt(index);
                }
            }

            return item;
        }
    }
}