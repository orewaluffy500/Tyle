namespace aski.token;

public class KeywordClass
{
    public static readonly Dictionary<string, string> Keywords = new()
    {
        {"set", "reg"},
        {"sft", "sft"},
        {"str", "str"},
        {"dup", "dup"},
        {"move", "move"},
        {"ewrite", "storei"},
        {"eread", "loadi"},
        {"write", "store"},
        {"read", "load"},
        {"sel", "sel"},
        {"syscall", "syscall"},
        {"if", "if"},
        {"until", "until"},
        {"end", "end"},
        {"break", "break"},
        {"halt", "halt"},
    };
}