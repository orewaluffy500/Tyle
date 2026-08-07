using aski.token;

namespace aski.instructions;

public class Instruction(string type, object[] values, Token token)
{
    public string Type = type;
    public object[] Values = values;

    public Token Token = token;

    public override string ToString()
    {
        // Return representation:
        // If values: (SET:A,B,C)
        // If no values: (SET)
        return Values.Length > 0 ? $"[{Type}:{string.Join(',', Values)}]" : $"[{Type}]";
    }
}