using aski.check;
using aski.error;
using aski.instructions;
using aski.token;

namespace aski.parser;




public class Parser
{
    public TokenList TokenList { get; }
    public int Index { get; private set; } = -1;
    public Token CurrentToken { get; private set; } = new(TokenType.NONE);
    public List<Instruction> Instructions { get; } = [];

    public Dictionary<string, Action> KeywordHandlers { get; }

    public Parser(Token[] tokens)
    {
        TokenList = new(tokens);

        // Dictionary that maps each keyword to its set handler function.
        KeywordHandlers = [];
        AddKeywordHandler("set", HandleKeywordSet);
        AddKeywordHandler("sft", HandleKeywordShift);
        AddKeywordHandler("dup", HandleKeywordDup);
        AddKeywordHandler("sel", HandleKeywordSel);
        AddKeywordHandler("str", HandleKeywordStr);

        AddKeywordHandler("if", HandleKeywordIf);
        AddKeywordHandler("until", HandleKeywordUntil);
        AddKeywordHandler("end", HandleKeywordEnd);
        AddKeywordHandler("break", HandleKeywordBreak);

        AddKeywordHandler("syscall", HandleKeywordSyscall);
        AddKeywordHandler("halt", HandleKeywordHalt);

        AddKeywordHandler("write", HandleKeywordWrite);
        AddKeywordHandler("read", HandleKeywordRead);
        
        AddKeywordHandler("ewrite", HandleKeywordElemWrite);
        AddKeywordHandler("eread", HandleKeywordElemRead);
    }

    public void AddKeywordHandler(string kw, Action handler) => KeywordHandlers[KeywordClass.Keywords[kw]] = handler;



    public Instruction[] Parse()
    {
        while (CurrentToken.Type != TokenType.EOF) // Run until current token is EOF
        {
            // If the current token is a keyword and it has a keyword value then run its handler
            if (CurrentToken.Type == TokenType.KEYWORD && CurrentToken.Value is not null)
            {
                string kw = (string)CurrentToken.Value;
                if (KeywordHandlers.TryGetValue(kw, out Action? handler)) handler();
            }

            // Advance to the next token
            Advance();
        }


        return [.. Instructions];
    }







    public void Advance()
    {
        if (++Index >= TokenList.Tokens.Length)
        {
            Index = TokenList.Tokens.Length;
        }

        CurrentToken = TokenList.Tokens[Index];
    }


    // ============================================
    //          HANDLERS
    // ============================================


    // Duplicate register to another one
    public void HandleKeywordDup()
    {
        // Validate tokens
        if (!TokenList.CheckNext(Index, TokenType.REGISTER) || !TokenList.CheckNext2(Index, TokenType.REGISTER))
            Error.ParserError("invalid duplicand", "duplication requires two valid register addresss", CurrentToken);

        // Get register A
        int? reg1 = TokenList.GetNext<int>(Index);
        if (reg1 is null) return;

        // Get register B
        int? reg2 = TokenList.GetNext2<int>(Index);
        if (reg2 is null) return;

        AddInstruction(InstructionTypes.DUPE, reg1, reg2);
    }

    // Adds instruction to change the value of a register
    public void HandleKeywordSet()
    {
        // Validate tokens
        if (!TokenList.CheckNext(Index, TokenType.REGISTER))
            Error.ParserError("invalid register address set", "setting value of register requires valid register address", CurrentToken);
        if (!TokenList.CheckNext2(Index, TokenType.NUMBER))
            Error.ParserError("invalid register address set", "setting value of register requires a valid numeral.", CurrentToken);

        // Get desired register
        int? reg = TokenList.GetNext<int>(Index);
        if (reg is null) return;

        // Get value to set
        int? val = TokenList.GetNext2<int>(Index);
        if (val is null) return;

        AddInstruction(InstructionTypes.REG, reg, val);
    }

    // Adds instruction to select a specified register at a point in time
    public void HandleKeywordSel()
    {
        // Validate token
        if (!TokenList.CheckNext(Index, TokenType.REGISTER))
            Error.ParserError("invalid selection", "selecting register requires valid register address", CurrentToken);

        // Get specified register
        int? reg = TokenList.GetNext<int>(Index);
        if (reg is null) return;

        // Add instruction
        AddInstruction(InstructionTypes.SELECT, reg);
    }


    // Adds instruction to change the string buffer
    public void HandleKeywordStr()
    {
        if (!TokenList.CheckNext(Index, TokenType.STRING))
            Error.ParserError("invalid string set", "setting string buffer requires valid string literal.", CurrentToken);

        string? val = TokenList.GetNext<string>(Index);
        if (val is null) return;

        AddInstruction(InstructionTypes.SET_STR, val);
    }
    

    // Changes a register
    public void HandleKeywordShift()
    {
        // Validate the target
        if (!TokenList.CheckNext(Index,TokenType.REGISTER))
            Error.ParserError("invalid change", "can't change register when there is no valid register given.", CurrentToken);
        
        // Validate the modification
        if (!CheckIfValue(2))
            Error.ParserError("invalid change", "invalid register or numeral given when changing register value.", CurrentToken);

        string modType = TokenList.GetType(Index + 2);
        int modification = TokenList.GetNext2<int>(Index);

        int target = TokenList.GetNext<int>(Index);

        AddInstruction(InstructionTypes.SHIFT, modType, modification, target);
    }

    // == CONTROL RELATED ===================================


    public void HandleKeywordEnd() => AddInstruction(InstructionTypes.END);
    public void HandleKeywordBreak() => AddInstruction(InstructionTypes.BREAK);

    public void HandleKeywordIf()
    {
        // Check if the next value is an identifier
        if (!TokenList.CheckNext(Index, TokenType.IDENTIFIER))
            Error.ParserError("invalid if statement", "if statement requires a valid operand, e.g equ, notequ etc..", CurrentToken);
        
        // Check if the next next value is a register or valid number
        if (!CheckIfValue(2))
            Error.ParserError("invalid if statement", "if statement requires valid register address or valid numeral", CurrentToken);

        string type = GetParamType(1); // Get the type of the next next value

        string operand = TokenList.GetNext<string>(Index) ?? "nil"; // get the operand

        // Validate operand (find if it exists)
        if (!CheckTypes.CheckMap.TryGetValue(operand, out int operandInt))
            Error.ParserError("invalid if condition", $"if condition requires a valid operand, '{operand}' is not valid.", CurrentToken);

        // Get the value of the next next value.
        int? val = TokenList.GetNext2<int>(Index);

        // Add the instruction
        AddInstruction(InstructionTypes.IF, type, val, operandInt);
    }


    public void HandleKeywordUntil()
    {
        // Check if the first parameter is a register.
        if (!TokenList.CheckNext(Index, TokenType.REGISTER))
            Error.ParserError("invalid until statement", "until statement requires a valid counter register.", CurrentToken);
        
        // Check if the 2nd param is a valid register or numeral
        if (!CheckIfValue(2))
            Error.ParserError("invalid until statement", "until statement requires a valid destination register or numeral.", CurrentToken);
        
        // Get the type of the destination
        string destType = GetParamType(2); // 2 means 2 tokens forward

        // Get the dest value
        int destValue = TokenList.GetNext2<int>(Index);

        // Get the register of the counter
        int counter = TokenList.GetNext<int>(Index);

        // Add instruction
        AddInstruction(InstructionTypes.UNTIL, destType, destValue, counter);
    }






    // == STORAGE RELATED ====================================

    // Write to an address the value of the selected register

    public void HandleKeywordWrite()
    {
        if (!TokenList.CheckNext(Index, TokenType.ADDRESS))
            Error.ParserError("invalid write", "writing to large store requires a valid address", CurrentToken);

        int? addr = TokenList.GetNext<int>(Index);
        if (addr is null) return;

        AddInstruction(InstructionTypes.WRITE, addr);
    }

    // Read from an address into selected register

    public void HandleKeywordRead()
    {
        if (!TokenList.CheckNext(Index, TokenType.ADDRESS))
            Error.ParserError("invalid read", "reading from large store requires a valid address", CurrentToken);

        int? addr = TokenList.GetNext<int>(Index);
        if (addr is null) return;

        AddInstruction(InstructionTypes.READ, addr);
    }


    // Write to an address based on a register value
    public void HandleKeywordElemWrite()
    {
        if (!TokenList.CheckNext(Index, TokenType.REGISTER))
            Error.ParserError("invalid var index write", "expected a register holding the address to write to.", CurrentToken);
        
        int varaddr = TokenList.GetNext<int>(Index);

        AddInstruction(InstructionTypes.ELEMENT_WRITE, varaddr);
    }

    public void HandleKeywordElemRead()
    {
        if (!TokenList.CheckNext(Index, TokenType.REGISTER))
            Error.ParserError("invalid var index read", "expected a register holding the address to read from.", CurrentToken);
        
        int varaddr = TokenList.GetNext<int>(Index);

        AddInstruction(InstructionTypes.ELEMENT_READ, varaddr);
    }

    // =======================================================




    public void HandleKeywordHalt()
    {
        AddInstruction(InstructionTypes.HALT);
    }



    // Adds instruction for system calls
    public void HandleKeywordSyscall()
    {
        if (!TokenList.CheckNext(Index, TokenType.SYSCALL)) return;

        string? callee = TokenList.GetNext<string>(Index);
        if (callee is null) return;

        AddInstruction(InstructionTypes.SYSCALL, callee);
    }





    // ============================================


    public void AddInstruction(string type, params object[] values)
    {
        Instructions.Add(new(type, values, CurrentToken));
    }



    // == HELPERS ==

    public bool CheckIfValue(int offset = 0)
    {
        return TokenList.Check(Index + offset, TokenType.NUMBER) ||
                TokenList.Check(Index + offset, TokenType.REGISTER); 
    }

    public string GetParamType(int offset = 0)
    {
        return TokenList.GetType(Index + offset);
    }
}