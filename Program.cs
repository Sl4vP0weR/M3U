using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace M3U
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;

            if (args.Length < 1)
            {
                Console.WriteLine("Try add path to M3U file.");
                Console.ReadKey();
                return;
            }

            var path = args[0];
            string outPath = null; try { outPath = args[1]; } catch { }

            string freeStr = "                                                                                                                                                                                                                                                                                                                                                                                 ";

            outPath = outPath ?? Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), DateTime.Now.ToLocalTime().ToString("dd.MM.yyyy hh mm"));

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            var movPath = Path.Combine(outPath, Path.GetFileName(path));
            Console.CursorVisible = false;
            try
            {
                using (var movStream = File.OpenWrite(movPath))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Processing...");
                    Console.ForegroundColor = ConsoleColor.Yellow;

                    var y = Console.CursorTop;
                    var lines = File.ReadAllLines(path).Where(ln => !string.IsNullOrEmpty(ln) && !ln.StartsWith("#")).ToList();
                    lines.ForEach(url =>
                    {
                        using (var wc = new WebClient())
                        {
                            Console.SetCursorPosition(0, y);
                            Console.Write(freeStr);
                            Console.SetCursorPosition(0, y);
                            Console.Write(url + "...");
                            var bytes = wc.DownloadData(url);
                            movStream.Write(bytes, 0, bytes.Length);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.Write("\nDone...");
            Console.CursorVisible = true;

            Console.ReadKey();
        }
    }
}