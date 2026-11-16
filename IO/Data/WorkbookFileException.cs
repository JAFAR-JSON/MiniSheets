namespace MiniSheets.IO;

// Thrown when an opened .msheet file is rejected (8.3).
// The message says which check failed, e.g. "sheet 2: duplicate name", so that it can be written to the log.
// IsNewerVersion tells apart the two messages the user can get:
// M-06 when the file was saved by a newer version, and M-05 for every other problem.
[Serializable]
public class WorkbookFileException : Exception
{
    public bool IsNewerVersion { get; } = false;

    public WorkbookFileException () {}

    public WorkbookFileException (string message)
        : base(message)
    {}

    public WorkbookFileException (string message, bool isNewerVersion)
        : base(message)
    {
        IsNewerVersion = isNewerVersion;
    }

    public WorkbookFileException (string message, Exception innerException)
        : base (message, innerException)
    {}
}
