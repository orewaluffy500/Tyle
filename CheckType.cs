namespace aski.check;


public class CheckTypes
{
    public const int EQUALS = 1;
    public const int NOT_EQU = 2;
    public const int LESSER = 3;
    public const int GREATER = 4;

    public static readonly Dictionary<string, int> CheckMap = new()
    {
        {"equ", EQUALS},
        {"notequ", NOT_EQU},
        {"smaller", LESSER},
        {"larger", GREATER}
    };
}