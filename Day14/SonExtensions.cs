public static class SonExtensions
{
    public static IEnumerable<T> Filterlash<T>(
        this IEnumerable<T> values, 
        Func<T, bool> predicate)
    {
        foreach(var value in values)
            if(predicate?.Invoke(value) is true)
                yield return value;
    }

    public static void Print(this IEnumerable<int> sonlar)
        => Console.WriteLine($"[ {string.Join(", ", sonlar)} ]");
        // => Console.WriteLine($"[ {sonlar.First()}, {string.Join(", ", sonlar.Skip(1))} ]");
}