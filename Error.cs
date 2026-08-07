using aski.token;

namespace aski.error;

public class Error
{
    public static void ParserError(string cause, string error, Token token, bool doHalt = true)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"(line: {token.Line}) parser error [{cause}]: {error}");
        Console.ResetColor();
        if (doHalt) Environment.Exit(1);
    }

    public static void InterpreterError(string cause, string error, Token token, bool doHalt = true)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"(line: {token.Line}) interpretion error [{cause}]: {error}");
        Console.ResetColor();
        if (doHalt) Environment.Exit(2);
    }

    public static void InterpreterError(string error, bool doHalt = true)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"anonymous interpretion error : {error}");
        Console.ResetColor();
        if (doHalt) Environment.Exit(2);
    }

    public static void SyntaxError(string cause, string error, Token token, bool doHalt = true)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"(line: {token.Line}) syntax error [{cause}]: {error}");
        Console.ResetColor();
        if (doHalt) Environment.Exit(3);
    }
}