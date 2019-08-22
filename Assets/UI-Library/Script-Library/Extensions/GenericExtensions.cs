using System.Linq;

public static class GenericExtensions
{
    public static bool EqualsAnyOf<T>(this T obj, params T[] array)
    {
        return array.Contains(obj);
    }
}

