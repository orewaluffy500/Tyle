namespace aski.token;


public class Token(string type, int line = 0, object? value = null){
    public string Type = type;
    public object? Value = value;
    public int Line = line;

    public override string ToString()
    {
        return Value is null ? $"({Type})" : $"({Type}:{Value})";
    }
}