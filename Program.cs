using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace M3U
{
    class Program
    {
        static void WriteLine(object msg, ConsoleColor color = ConsoleColor.White, int x = -1, int y = -1) => Write(msg + "\n", color, x, y);
        static void Write(object msg, ConsoleColor color = ConsoleColor.White, int x = -1, int y = -1)
        {
            Console.SetCursorPosition(x == -1 ? Console.CursorLeft : x, y == -1 ? Console.CursorTop : y);
            Console.ForegroundColor = color;
            Console.Write(msg.ToString());
        }
        static void Main(string[] args)
        {
            AsyncMain(args).Wait();
        }
        static async Task AsyncMain(string[] args)
        {
            Console.CursorVisible = false;
            if (args.Length < 1)
            {
#if DEBUG
                args = new[] { Path.Combine(Directory.GetCurrentDirectory(), "720.m3u8") };
#else
                WriteLine("Try add path to M3U file.", ConsoleColor.Gray);
                Console.ReadKey();
                return;
#endif
            }

            var path = args[0];
            string outPath = null; try { outPath = args[1]; } catch { }

            outPath = outPath ?? Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), DateTime.Now.ToString("dd.MM.yyyy hh mm"));

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            var movPath = Path.Combine(outPath, Path.GetFileName(path));
            var doneColor = ConsoleColor.Green;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                using (var movStream = File.OpenWrite(movPath))
                {
                    WriteLine("Processing...", ConsoleColor.Cyan);

                    var y = Console.CursorTop;
                    var lines = File.ReadAllLines(path).Where(ln => !string.IsNullOrEmpty(ln) && !ln.StartsWith("#")).ToList();
                    var Dict = new Dictionary<int, string>();
                    for (int i = 0; i < lines.Count; i++) Dict.Add(i, lines[i]);
                    var Downloaded = 0;
                    Task.WaitAll(Dict.AsParallel().Select(async kvp =>
                    {
                        var idx = kvp.Key;
                        var url = kvp.Value;
                        using (var wc = new WebClient())
                        {
                            await wc.DownloadFileTaskAsync(url, Path.Combine(outPath, $"{idx}.tmp"));
                            Write($"Downloaded {((float)++Downloaded) / Dict.Count:P}    ", ConsoleColor.Yellow, 0, y);
                        }
                    }).ToArray());
                    WriteLine("\nConcating temp files in one...", ConsoleColor.Yellow);
                    foreach (var f in Directory.GetFiles(outPath, "*.tmp").OrderBy(x => int.Parse(Path.GetFileNameWithoutExtension(x))))
                    {
                        using (var fs = File.OpenRead(f))
                            fs.CopyTo(movStream);
                        File.Delete(f);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLine(ex, doneColor = ConsoleColor.Red);
            }
            stopwatch.Stop();
            Write($"Done in {Math.Round(stopwatch.Elapsed.TotalSeconds, 2)}s...", doneColor);
            Console.CursorVisible = true;

            Console.ReadKey();
        }
    }
}
