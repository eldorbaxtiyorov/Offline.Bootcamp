using System.Diagnostics;

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Kichraytirish uchan .mp4 file yoki papka manzilini kiriting.");
    Console.Write("👉 ");

    var pathInput = Console.ReadLine();

    if (
        string.IsNullOrWhiteSpace(pathInput) ||
        string.Equals(Path.GetExtension(pathInput) , ".mp4", StringComparison.InvariantCultureIgnoreCase) is false &&
        Directory.Exists(pathInput) is false)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Bunday papka yoki fayl topilmadi!");
        continue;
    }

    var files = new List<string>();
    if (Directory.Exists(pathInput))
        Directory.GetFiles(pathInput)
            .Where(file => string.Equals(Path.GetExtension(file), ".mp4", StringComparison.OrdinalIgnoreCase))
            .ToList()
            .ForEach(files.Add);
    else
        files.Add(pathInput);

    if (files.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Papkada .mp4 formatidagi fayl topilmadi!");
        continue;
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Video 1~10 yakuniy sifatini tanlang. Standard sifat 5 ga teng.");
    Console.Write("👉 ");

    var sifat = 5;
    if (int.TryParse(Console.ReadLine(), out sifat) is false)
        sifat = 5;

    sifat %= 11;

    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine($"Video {sifat} sifatda kichraytiriladi.");

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Parallelik 1~10 darajasini tanlang. Standart daraja 5 ga teng.");
    Console.Write("👉 ");

    var daraja = 5;
    if (int.TryParse(Console.ReadLine(), out daraja) is false)
        daraja = 5;

    daraja %= 11;

    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine($"Video {daraja} darajada parallel kichraytiriladi.");

    var tasks = files.Select(file => Task.Run(async () =>
    {
        var fileName = Path.GetFileName(file);
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Fayl {fileName} kichraytirilmoqda.");

        var outputPath = Path.Combine(Path.GetDirectoryName(file)!, Path.GetFileNameWithoutExtension(file) + "_compressed.mp4");

        var ffmpegStartInfo = new ProcessStartInfo();
        ffmpegStartInfo.UseShellExecute = false;
        ffmpegStartInfo.RedirectStandardOutput = true;
        ffmpegStartInfo.RedirectStandardError = true;
        ffmpegStartInfo.FileName = "ffmpeg";
        ffmpegStartInfo.Arguments = $"-y -i \"{file}\" -vcodec h264_qsv -global_quality {28 - sifat} -preset slow -acodec copy \"{outputPath}\" -threads {daraja}";

        Console.WriteLine(ffmpegStartInfo.Arguments);
        var ffmpegProcess = Process.Start(ffmpegStartInfo);

        await ffmpegProcess!.WaitForExitAsync();

        if (ffmpegProcess.ExitCode != 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fayl {fileName} kichraytirishda xatolik yuz berdi!");
            return;
        }

        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine($"Fayl {fileName} kichraytirildi.");
    }));

    await Task.WhenAll(tasks);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Barcha fayllar kichraytirildi.");
}