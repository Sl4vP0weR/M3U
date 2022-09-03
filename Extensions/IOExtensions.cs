global using static IOExtensions;

public static class IOExtensions
{
    public static FileStream FastOpenFile(string path) => new(path,
        FileMode.OpenOrCreate,
        FileAccess.ReadWrite,
        FileShare.ReadWrite,
        4096,
        FileOptions.Asynchronous | FileOptions.SequentialScan
        );
}