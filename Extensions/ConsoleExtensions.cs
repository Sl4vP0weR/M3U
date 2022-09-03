global using static ConsoleExtensions;

public static class ConsoleExtensions
{
    private static SemaphoreSlim writeSemaphore = new(1);
    public static void WriteLine(object msg = null, ConsoleColor color = ConsoleColor.White, int x = -1, int y = -1) => Write(msg + Environment.NewLine, color, x, y);
    public static async void Write(object msg = null, ConsoleColor color = ConsoleColor.White, int x = -1, int y = -1)
    {
        await writeSemaphore.WaitAsync();
        try
        {
            x = Math.Min(Math.Min(x < 0 ? Console.CursorLeft : x, Console.BufferWidth), Console.WindowWidth);
            y = Math.Min(Math.Min(y < 0 ? Console.CursorTop : y, Console.BufferHeight), Console.WindowHeight);
            Console.SetCursorPosition(x, y);
        }
        catch { }
        Console.ForegroundColor = color;
        Console.Write(msg + "");
        writeSemaphore.Release();
    }
    public static void SynchronousContextLoop()
    {
        while (true) Thread.Sleep(int.MaxValue); // synchronous context
    }
}