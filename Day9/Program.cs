using System.Numerics;

var cts = new CancellationTokenSource();
while (int.TryParse(Console.ReadLine(), out var n))
{ 
    var _ = FindNthFibonacci(n, cts.Token);
}

await cts.CancelAsync();

Console.WriteLine("Dastur tugadi!");
Console.ReadKey();

Console.ForegroundColor = ConsoleColor.DarkBlue;
Console.WriteLine("Dastur to'xtatildi!");

async Task<BigInteger> FindNthFibonacci(int n, CancellationToken cancellationToken = default)
{
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine($"{n}-Fibonaccini hisoblash boshlandi.");

    if(n <= 2)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{n}-Fibonacci son = {1}.");

        return 1;
    }

    await Task.Yield();

    var a = new BigInteger(1);
    var b = new BigInteger(1);
    var c = BigInteger.Add(a, b);

    for(int i = 2; i < n - 1; i ++)
    {  
        if(cancellationToken.IsCancellationRequested)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{n}-Fibonaccini hisoblash {i}-hadga kelganda bekor qilindi! Qiymat {c} ga teng.");
            return await Task.FromCanceled<BigInteger>(cancellationToken);
        }

        a = b;
        b = c;
        c = BigInteger.Add(a, b);
    }

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"{n}-Fibonacci son = {c}.");

    return await Task.FromResult(c);
}