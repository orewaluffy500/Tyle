namespace aski.token;


public class TokenList(Token[] tokens)
{
    public Token[] Tokens { get; } = tokens;

    // Check if I token is T type.
    public bool Check(int index, string type) => IsInBounds(index) && Tokens[index].Type == type;

    // Check if the token infront of I is T type
    public bool CheckNext(int index, string type) => Check(index + 1, type);

    // Check if the token 2 elements infront of I is T type
    public bool CheckNext2(int index, string type) => Check(index + 2, type);

    // Check if said index is in bounds
    public bool IsInBounds(int index) => index >= 0 && index < Tokens.Length;

    // Get a specified token as T
    public T? Get<T>(int index) => (T?) Tokens[index].Value;
    public T? GetNext<T>(int index) => (T?) Tokens[index + 1].Value;
    public T? GetNext2<T>(int index) => (T?) Tokens[index + 2].Value;


    public string GetType(int index) => index < Tokens.Count() ? Tokens[index].Type : TokenType.NONE;
}