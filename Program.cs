using System;
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
            Console.Write(msg);
        }
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            if (args.Length < 1)
            {
                WriteLine("Try add path to M3U file.", ConsoleColor.Gray);
                Console.ReadKey();
                return;
            }

            var path = args[0];
            string outPath = null; try { outPath = args[1]; } catch { }

            string freeStr = "                                                                                                                                                                                                                                                                                                                                                                                 ";

            outPath = outPath ?? Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), DateTime.Now.ToString("dd.MM.yyyy hh mm"));

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            var movPath = Path.Combine(outPath, Path.GetFileName(path));
            
            try
            {
                using (var movStream = File.OpenWrite(movPath))
                {
                    WriteLine("Processing...", ConsoleColor.Cyan);

                    var y = Console.CursorTop;
                    var lines = File.ReadAllLines(path).Where(ln => !string.IsNullOrEmpty(ln) && !ln.StartsWith("#")).ToList();
                    using (var wc = new WebClient())
                    {
                        lines.ForEach(url =>
                        {
                            Write(freeStr, 0, 0, y);
                            Write(url + "...", ConsoleColor.Yellow, 0, y);
                            var bytes = wc.DownloadData(url);
                            movStream.Write(bytes, 0, bytes.Length);
                        });
                    }
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
