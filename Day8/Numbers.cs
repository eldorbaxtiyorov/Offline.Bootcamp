namespace Offline.Bootcamp.Day8;

public static class Numbers
{
   public static int Sonlar(this string? sonlar, out int[]? sonlarArray)
   {
        sonlarArray = sonlar?.Split(',')
                    .Select(int.Parse)
                    .ToArray() ?? [];
    return sonlarArray.Length;
            
   }
   public static int EngKattasi(this int[] sonlarArray)
   {
    return sonlarArray.Max();
   }
}
