using System.Text.RegularExpressions;
using aski.executor;
using aski.lexer;
using aski.memory;
using aski.parser;

int regCount = args switch
{
    var s when s.Contains("-core1") => 8,
    var s when s.Contains("-core3") => 48,
    _ => 16,
};

int valCount = args switch
{
    var s when s.Contains("-core1") => 512,
    var s when s.Contains("-core3") => 16384,
    _ => 2048,
};

VirtualMachine vm = new(regCount, valCount);


bool showTokens = args.Contains("-st");
bool showInstruction = args.Contains("-si");

string code = File.ReadAllText("code.tyle");
code = Regex.Replace(code, @" {2,}", " ");

Lexer lexer = new(code);
var tokens = lexer.Lex();

Parser parser = new(tokens);
var instructions = parser.Parse();

Executor executor = new(instructions, vm);

if (showTokens)
{
    Console.WriteLine("<== TOKENS =============================>");
    Console.WriteLine(string.Join(' ', tokens));
    Console.WriteLine("<=======================================>");
    Console.WriteLine();
}

if (showInstruction)
{
    Console.WriteLine("<== INSTRUCTIONS =======================>");
    Console.WriteLine(string.Join(' ', instructions));
    Console.WriteLine("<=======================================>");
    Console.WriteLine();
}

Console.WriteLine("<== OUTPUT =============================>");

executor.Execute();

Console.WriteLine("<=======================================>");
