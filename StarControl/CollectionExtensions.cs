namespace StarControl;

internal static class CollectionExtensions
{
    public static bool AnyNotNull<T>(this IEnumerable<T?> collection)
    {
        return collection.Any(item => item is not null);
    }

    public static bool IsEquivalentTo<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> dict,
        IReadOnlyDictionary<TKey, TValue> other,
        Func<TValue, TValue, bool>? equals = null
    )
    {
        return dict.Count == other.Count
            && dict.All(kv =>
                other.TryGetValue(kv.Key, out var otherValue)
                && (equals?.Invoke(kv.Value, otherValue) ?? kv.Value?.Equals(otherValue) == true)
            );
    }

    public static bool SequenceEqual<T1, T2>(
        this IEnumerable<T1> first,
        IEnumerable<T2> second,
        Func<T1, T2, bool> equals
    )
    {
        using var e1 = first.GetEnumerator();
        using var e2 = second.GetEnumerator();
        while (e1.MoveNext())
        {
            if (!(e2.MoveNext() && equals(e1.Current, e2.Current)))
            {
                return false;
            }
        }
        return !e2.MoveNext();
    }
}
