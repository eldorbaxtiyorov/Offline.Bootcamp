using System.Collections.Concurrent;
using System.Diagnostics;
using Spectre.Console;

Dictionary<string, ProgressTask> tasks = [];

while (true)
{
    var files = AskFiles();
    var encoder = "libx264"; // AskForVideoEncoder();
    var preset = AskPresetSpeed();

}

static async Task TestProgress(IEnumerable<string> files)
{

    await AnsiConsole.Progress()
        .AutoClear(false)
        .Columns([
            new ProgressBarColumn()
        ])
        .StartAsync(async ctx => 
        {
            var durationsDictionary = new ConcurrentDictionary<string, double>();
            await Task.WhenAll(files.Select(x => Task.Run(async () => durationsDictionary[x] = await GetVideoDuration(x))));
            
            Dictionary<string, (ProgressTask, double)> tasks = [];

            files.ToList()
                .ForEach(async file => 
                {
                    var task = ctx.AddTask(Path.GetFileName(file));
                    tasks[file] = (task, durationsDictionary[file]);
                });
        });

}

static async Task<double> GetVideoDuration(string path)
{
    using var process = new Process();

    process.StartInfo.FileName = "ffprobe";
    process.StartInfo.Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{path}\"";
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;

    process.Start();
    string output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();

    return double.Parse(output);
}

static string AskPresetSpeed()
{
    return AnsiConsole.Prompt(new SelectionPrompt<string>()
        .EnableSearch()
        .Title("[bold green]Qanday tezlikda kompres qilamiz?[/]")
        .AddChoices([
            "ultrafast",
            "superfast",
            "veryfast",
            "faster",
            "fast",
            "medium",
            "slow",
            "slower",
            "veryslow"
        ]));
}

static string AskForVideoEncoder()
{
    if (AnsiConsole.Confirm("[bold green]GPU orqali tezlashtirishni istaysizmi?[/]") is false)
        return "libx264";

    using var process = new Process();

    process.StartInfo.FileName = "ffmpeg";
    process.StartInfo.Arguments = "-encoders";
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;

    process.Start();
    string output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();

    return output switch
    {
        string qsv when qsv.Contains("h264_qsv") => "h264_qsv",
        string qsv when qsv.Contains("h264_nvenc") => "h264_nvenc",
        string qsv when qsv.Contains("h264_amf") => "h264_amf",
        _ => "libx264"
    };
}

static IEnumerable<string> AskFiles()
{
    AnsiConsole.MarkupLine("[bold green].mp4 fayl yoki papka manzilini kiriting[/]");
    var path = AnsiConsole.Prompt(
        new TextPrompt<string>(":right_arrow:")
        .PromptStyle("green")
        .ValidationErrorMessage("[red].mp4 fayl yoki papka topilmadi[/]")
        .Validate(IsValidPathInput)
    );

    if (IsMp4File(path))
        return [path];

    ValidatePath(path, out var files);

    return files;
}

static bool IsValidPathInput(string path)
    => IsMp4File(path) || ValidatePath(path, out var files);

static bool ValidatePath(string path, out IEnumerable<string> files)
{
    files = new List<string>();

    if (Directory.Exists(path) is false)
        return false;

    files = Directory.GetFiles(path).Where(IsMp4File).ToList();

    return files?.Any() is true;
}

static bool IsMp4File(string path)
    => File.Exists(path)
    && string.Equals(Path.GetExtension(path), ".mp4", StringComparison.OrdinalIgnoreCase);