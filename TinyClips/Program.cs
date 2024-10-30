using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Spectre.Console;

var exitTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (c, a) => exitTokenSource.Cancel();

while (true)
{
    var files = AskFiles();
    var preset = AskPresetSpeed();
    await StartEncodingAsync(files, preset, exitTokenSource.Token);
}

static async Task StartEncodingAsync(
    IEnumerable<string> files, 
    string preset,
    CancellationToken cancellationToken = default)
{

    await AnsiConsole.Progress()
        .AutoClear(false)
        .Columns([
            new TaskDescriptionColumn(),
            new ProgressBarColumn(),
            new PercentageColumn()
        ])
        .StartAsync(async ctx => 
        {
            var durationsDictionary = new Dictionary<string, double>();

            await Task.WhenAll(
                files.Select(x => Task.Run(async () => durationsDictionary[x] = await GetVideoDuration(x, cancellationToken))));
            
            var tasks = files.Select(async file => 
            {
                var task = ctx.AddTask(Path.GetFileName(file));

                await Task.Run(async () => 
                {
                    using var process = new Process();

                    var outputPath = Path.Combine(Path.GetDirectoryName(file)!, Path.GetFileNameWithoutExtension(file) + "-compressed.mp4");

                    process.StartInfo.FileName = "ffmpeg";
                    process.StartInfo.Arguments = $"-y -i \"{file}\" -l warning -vcodec libx264 -global_quality 25 -preset {preset} -acodec copy \"{outputPath}\" -threads 5";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;

                    process.OutputDataReceived += TaskFailed;
                    process.ErrorDataReceived += UpdateProgress;

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await process.WaitForExitAsync(cancellationToken);
                }, cancellationToken);

                task.Value = 100;
                task.StopTask();

                void TaskFailed(object sender, DataReceivedEventArgs e)
                {
                    Console.WriteLine(e.Data);
                }

                void UpdateProgress(object sender, DataReceivedEventArgs e)
                {
                    if (string.IsNullOrEmpty(e.Data)) return;

                    var match = Regex.Match(e.Data, @"time=(\d+:\d+:\d+\.\d+)");
                    if (match.Success && durationsDictionary.TryGetValue(file, out var duration))
                    {
                        var currentTime = TimeSpan.Parse(match.Groups[1].Value).TotalSeconds;
                        var progress = (currentTime / duration) * 100;
                        task.Value = progress; 
                    }
                }
            });

            await Task.WhenAll(tasks);
        });

}

static async Task<double> GetVideoDuration(string path, CancellationToken cancellationToken = default)
{
    using var process = new Process();

    process.StartInfo.FileName = "ffprobe";
    process.StartInfo.Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{path}\"";
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.CreateNoWindow = true;

    process.Start();
    string output = process.StandardOutput.ReadToEnd();
    await process.WaitForExitAsync(cancellationToken);

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