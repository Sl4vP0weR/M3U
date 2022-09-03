global using static ProgramExtensions;

public static class ProgramExtensions
{
    public static void Exit(int code = 0, bool force = false)
    {
#if !DEBUG
        if(!force)
            Console.ReadKey();
#endif
        WriteLine(null);
        Environment.Exit(code);
    }
    public static void Exit(ExitCode code = ExitCode.Succes, bool force = false) => Exit((int)code, force);
    public static void Try(Action action)
    {
        try
        {
            action();
        }
        catch { }
    }
}