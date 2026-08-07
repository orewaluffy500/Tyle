using aski.token;

namespace aski.lexer;


public class Lexer(string code)
{
    public string Code = code;
    public int Index { get; private set; } = -1;
    public char CurrentToken { get; private set; } = '\0';

    public void Advance()
    {
        Index++;
        if (Index >= Code.Length)
        {
            CurrentToken = '\0';
        } else
        {
            CurrentToken = Code[Index];
        }
    }




    public Token[] Lex()
    {
        List<Token> tokens = [];

        Advance();

        bool PreviousTokenWasEOS = true;
        int TotalLines = 0;

        while (CurrentToken != '\0')
        {
            // Comments
            if (CurrentToken == '{')
            {
                Advance();
                while (CurrentToken != '}' && CurrentToken != '\0')
                {
                    Advance();
                }
            }

            // Skip tabs and spaces
            if (" \t".Contains(CurrentToken)) Advance();

            // Handle tokens.
            Token token = new(TokenType.NONE);

            switch (CurrentToken)
            {
                case '"':
                    token.Type = TokenType.STRING;
                    token.Value = LexString();
                    break;
                
                case char CurrentToken when TokenConst.Alphabet.Contains(CurrentToken):
                    string id = LexIdentifier();
                    token.Value = id;
                    token.Type = PreviousTokenWasEOS ? TokenType.KEYWORD : TokenType.IDENTIFIER;
                    break;
                
                case char CurrentToken when TokenConst.Numeric.Contains(CurrentToken):
                    token.Type = TokenType.NUMBER;
                    token.Value = LexNumeral();
                    break;
                
                case '-': // we dont use minus for anything else but negative numbers
                    token.Type = TokenType.NUMBER;
                    Advance(); // Skip the minus
                    token.Value = -LexNumeral(); // negate the value since we know its negative
                    break;

                // Syscall

                case '[':
                    token.Type = TokenType.SYSCALL;
                    token.Value = LexSyscall();
                    break;
                
                // Memory related

                case '#': // Register
                    token.Type = TokenType.REGISTER;
                    Advance(); // Skip the hash tag
                    token.Value = LexNumeral();
                    break;
                
                case '$': // Address
                    token.Type = TokenType.ADDRESS;
                    Advance(); // Skip the hash tag
                    token.Value = LexNumeral();
                    break;

                // Special cases
                case '\n':
                    token.Type = TokenType.EOS;
                    break;
                
                case ':':
                    token.Type = TokenType.EOS;
                    break;
            }


            // Append token
            if (PreviousTokenWasEOS) TotalLines++;
            PreviousTokenWasEOS = token.Type == TokenType.EOS;

            token.Line = TotalLines;
            if (token.Type != TokenType.NONE) tokens.Add(token);
            Advance();
        }


        tokens.Add(new(TokenType.EOF));
        return [ .. tokens ];
    }

    private string LexSyscall()
    {
        string s = "";

        Advance();

        while (CurrentToken != '\n' && CurrentToken != ']')
        {
            s += CurrentToken;
            Advance();
        }

        return s;
    }

    private int LexNumeral()
    {
        string s = "";

        while (TokenConst.Numeric.Contains(CurrentToken) && CurrentToken != '\0')
        {
            s += CurrentToken;
            Advance();
        }

        return Convert.ToInt32(s);
    }

    private string LexIdentifier()
    {
        string s = "";

        while (TokenConst.Alphabet.Contains(CurrentToken) && CurrentToken != '\0')
        {
            s += CurrentToken;
            Advance();
        }

        return s;
    }

    private string LexString()
    {
        string s = "";

        Advance();

        while (CurrentToken != '"' && CurrentToken != '\0')
        {
            s += CurrentToken;
            Advance();
        }

        return s;
    }
}