using System.Collections.Generic;

namespace The_Milkman.Extensions
{
    public static class CollectionsExtensions
    {
        public static bool TryPopAt<T>(this List<T> list, int index, out T element)
        {
            element = default(T);
            if (list.Count < 0 || index > list.Count - 1)
                return false;

            element = list[index];
            list.Remove(element);
            return true;
        }
    }
}