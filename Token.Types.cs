namespace aski.token;

public class TokenType
{
    public const string STRING = "STR";
    public const string NUMBER = "NUM";
    public const string KEYWORD = "KW";
    public const string IDENTIFIER = "ID";
    public const string SYSCALL = "SCALL";

    // Memory related

    public const string REGISTER = "REG";
    public const string ADDRESS = "ADDR";

    // Special
    public const string NONE = "NONE";
    public const string EOS = "EOS";
    public const string EOF = "EOF";
}