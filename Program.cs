namespace M3U;

public static class Program
{
    private static void Main(string[] args)
    {
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            Exit(ExitCode.Interupt, true);
        };
        AwaitAsyncMain(args);
        SynchronousContextLoop();
    }
    private static async void AwaitAsyncMain(string[] args) => Exit(await AsyncMain(args));

    public static Stopwatch Stopwatch { get; private set; }
    public static async Task<ExitCode> AsyncMain(string[] args)
    {
        Console.CursorVisible = false;
        if (args.Length < 1)
        {
#if DEBUG
            args = new[] { Path.Combine(Directory.GetCurrentDirectory(), "720.m3u8") }; // default argumets
#else
            WriteLine("Try add path to M3U file.", ConsoleColor.Gray);
            return ExitCode.InvalidArguments;
#endif
        }

        var path = args[0];
        var outputPath = args.ElementAtOrDefault(1) ?? 
            Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), $"{DateTime.Now:dd.MM.yyyy HH mm ss}", "720.ts"); // default output path

        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);

        Stopwatch = new();
        Stopwatch.Start();
        var exception = await DownloadTask(path, outputPath);
        var success = exception is null;
        var color = success ? ConsoleColor.Green : ConsoleColor.Red;
        Stopwatch.Stop();

        if (!success) WriteLine(exception, color);
        WriteLine($"Finished after {Math.Round(Stopwatch.Elapsed.TotalSeconds, 2)}s...", color);
        Console.CursorVisible = true;
        return ExitCode.Succes;
    }

    public static PartialFileDownloader Downloader { get; private set; }
    public static async Task<Exception> DownloadTask(string path, string outputPath, bool logInfo = true)
    {
        try
        {
            var urls = File
                .ReadAllLines(path)
                .Where(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("#"))
                .ToList();
            using (Downloader = new(outputPath, urls))
            {
                if(logInfo) RegisterInformationLogs(urls.Count);
                await Downloader.DownloadAsync();
            }
        }
        catch (Exception ex) { return ex; }
        return null;
    }

    private static void RegisterInformationLogs(int total)
    {
        var cursorHeight = Console.CursorTop;
        var downloadsHeight = cursorHeight;
        var downloadSpeedHeight = ++cursorHeight;
        var writeHeight = ++cursorHeight;
        Downloader.FileDownloaded += () => Write($"Downloaded {(Downloader.Downloads + 1d) / total:P}", ConsoleColor.Yellow, 0, downloadsHeight);
        Downloader.FileDownloaded += () => Write($"Average Speed {Downloader.TotalLength / Stopwatch.Elapsed.TotalSeconds / 1024:F0} KB/s\t", ConsoleColor.Yellow, 0, downloadSpeedHeight);
        Downloader.PositionMoved += () => Write($"Writen {(Downloader.Position + 1d) / total:P}", ConsoleColor.Yellow, 0, writeHeight);
        Downloader.Completed += () => WriteLine($"{Environment.NewLine}Completed...", ConsoleColor.Cyan, 0, writeHeight+1);
    }
}
