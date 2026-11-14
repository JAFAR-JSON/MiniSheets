namespace MiniSheets.Core;
// Source - https://stackoverflow.com/a/2200389
// Posted by Frederik Gheysels, modified by community. See post 'Timeline' for change history
// Retrieved 2026-11-14, License - CC BY-SA 3.0

[Serializable]
public class ParsingException : Exception
{
    public ParsingException () {}

    public ParsingException (string message) 
        : base(message)
    {}

    public ParsingException (string message, Exception innerException)
        : base (message, innerException)
    {}    
}
