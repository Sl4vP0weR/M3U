namespace M3U;

public record PartialFileDownloader : IDisposable
{
    public PartialFileDownloader(string outputPath, ICollection<string> urls)
    {
        OutputPath = outputPath;
        Urls = urls;
        Dispose();
    }

    public event Action Completed;
    public readonly string OutputPath;
    private readonly ICollection<string> Urls;
    private string tempDirectory;

    public long TotalLength { get; protected set; }
    public int Downloads { get; protected set; }
    public event Action FileDownloaded;
    public int Position { get; protected set; }
    public event Action PositionMoved;
    private SemaphoreSlim downloadSemaphore = new(1, 3);

    private void TryDeleteTempDirectory()
    {
        if (!string.IsNullOrWhiteSpace(tempDirectory))
            Try(() => Directory.Delete(tempDirectory, true));
    }

    public void Dispose()
    {
        Try(() => Output?.Dispose());
        TotalLength = 0;
        Downloads = 0;
        Position = 0;
        TryDeleteTempDirectory();
    }
    private void ExitHandler(object sender, EventArgs e)
    {
        Dispose();
#if DEBUG
        Try(() => Directory.Delete(Path.GetDirectoryName(OutputPath), true));
#endif
    }

    public async Task DownloadAsync()
    {
        Dispose();
        for(int i = 0; i < int.MaxValue; i++)
        {
            if (!Directory.Exists(tempDirectory = $"{Path.GetTempPath()}m3u_{i}"))
                break;
            TryDeleteTempDirectory();
        }

        Directory.CreateDirectory(tempDirectory);
        AppDomain.CurrentDomain.ProcessExit += ExitHandler;

        await Task.WhenAll(DownloadFilesAsync().Append(ConcatFilesAsync()));
        Completed?.Invoke();
    }

    private IEnumerable<Task> DownloadFilesAsync() => Enumerable.Range(0, Urls.Count).Select(x => DownloadAsync(Urls.ElementAt(x), x));

    private async Task DownloadAsync(string url, int index)
    {
    retry:
        await Task.Delay(250);
        try
        {
            await downloadSemaphore.WaitAsync();
            using var client = new HttpClient();
            using var stream = await client.GetStreamAsync(url);
            using var file = FastOpenFile(GetTempFilePath(index));
            await stream.CopyToAsync(file);
            downloadSemaphore.Release();
            TotalLength += file.Length;
            Downloads++;
            FileDownloaded?.Invoke();
        }
        catch { await Task.Delay(500); goto retry; }
    }

    private FileStream Output;
    private async Task ConcatFilesAsync()
    {
        Position = 0;
        using (Output = FastOpenFile(OutputPath))
        {
            for (int i = 0; i < Urls.Count; i++)
            {
                var path = GetTempFilePath(i);
                while (!File.Exists(path))
                    await Task.Delay(500);
                using var file = FastOpenFile(path);
                await file.CopyToAsync(Output);
                Position++;
                PositionMoved?.Invoke();
            }
        }
    }

    private string GetTempFilePath(int index) => Path.Combine(tempDirectory, $"{index}.tmp");
}
