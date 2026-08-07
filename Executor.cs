using aski.check;
using aski.controls;
using aski.error;
using aski.instructions;
using aski.memory;
using aski.token;

namespace aski.executor;


public class Executor
{
    public Instruction[] Instructions { get; }

    public VirtualMachine VM { get; }

    public int InstructionIndex { get; set; } = 0;

    public Dictionary<string, Action<int, Instruction>> InstructionHandlers { get; }
    public Dictionary<string, Action<int, Instruction>> ControlHandlers { get; }

    public Executor(Instruction[] instructions, VirtualMachine vm)
    {
        VM = vm;

        Instructions = instructions;
        InstructionHandlers = new()
        {
            {InstructionTypes.REG,              HandleInstructionSet},
            {InstructionTypes.SET_STR,          HandleInstructionStr},
            {InstructionTypes.SELECT,           HandleInstructionSel},
            {InstructionTypes.WRITE,            HandleInstructionWrite},
            {InstructionTypes.READ,             HandleInstructionRead},
            {InstructionTypes.ELEMENT_WRITE,    HandleInstructionElemWrite},
            {InstructionTypes.ELEMENT_READ,     HandleInstructionElemRead},
            {InstructionTypes.SHIFT,            HandleInstructionShift},
            {InstructionTypes.SYSCALL,          HandleInstructionSyscall},
            {InstructionTypes.HALT,             HandleInstructionHalt},
            {InstructionTypes.BREAK,            HandleInstructionBreak},
        };

        ControlHandlers = new()
        {
            {InstructionTypes.IF,               HandleInstructionIf},
            {InstructionTypes.UNTIL,            HandleInstructionUntil},
            {InstructionTypes.END,              HandleInstructionEnd},
            {InstructionTypes.DUPE,             HandleInstructionDup},
        };
    }

    public void Execute()
    {
        for (InstructionIndex = 0; InstructionIndex < Instructions.Length; InstructionIndex++)
        {
            ControlBlock top = VM.Controls.Top(); // top control , root if no top control.
            Instruction instruction = Instructions[InstructionIndex];

            // Handle control handlers
            if (ControlHandlers.TryGetValue(instruction.Type, out var controlHandler))
            {
                controlHandler(InstructionIndex, instruction);
            }

            if (top.Type != ControlTypes.ROOT && !top.Active) // Determine if we should skip.
            {
                continue;
            }

            // Check if handlers have a handler for said instruction
            if (InstructionHandlers.TryGetValue(instruction.Type, out Action<int, Instruction>? instructionHandler))
            {
                instructionHandler(InstructionIndex, instruction);
            }
        }
    }






    public void HandleInstructionDup(int index, Instruction instruction)
    {
        int reg1 = (int)instruction.Values[0];
        int reg2 = (int)instruction.Values[1];

        VM.SetRegister(reg2, VM.GetRegister(reg1));
    }

    public void HandleInstructionSet(int index, Instruction instruction)
    {
        VM.SetRegister((int)instruction.Values[0], (int)instruction.Values[1]);
    }

    public void HandleInstructionStr(int index, Instruction instruction)
    {
        VM.StringBuffer = (string)instruction.Values[0];
    }

    public void HandleInstructionSel(int index, Instruction instruction)
    {
        VM.Selection = (int)instruction.Values[0];
    }

    public void HandleInstructionShift(int index, Instruction instruction)
    {
        // Fetch the numeral value or the value of the specified register
        int val = FetchNumber(instruction, 0, 1);

        // Fetch the target register
        int target = instruction.Values[2] as int? ?? 0;

        int oldRegValue = VM.GetRegister(target);
        VM.SetRegister(target, oldRegValue + val);
    }


    // == CONTROL STUFF


    public void HandleInstructionIf(int i, Instruction inst)
    {
        // Get the value of the specified register/numeral to compare against
        int val = FetchNumber(inst, 0, 1);

        int operand = (int)inst.Values[2]; // Get the operand

        // Selection value
        int selectedValue = VM.GetSelectionValue();

        // Handle case for each type of operand
        bool active = operand switch
        {
            CheckTypes.EQUALS => selectedValue == val,
            CheckTypes.NOT_EQU => selectedValue != val,
            CheckTypes.LESSER => selectedValue < val,
            CheckTypes.GREATER => selectedValue > val,
            _ => false
        };

        VM.Controls.Push(ControlTypes.IF, i, active && VM.Controls.Top().Active);
    }


    public void HandleInstructionUntil(int i, Instruction inst)
    {
        // Get the value of the destination
        int val = FetchNumber(inst, 0, 1);

        // Get the counter
        int counter = inst.Values[2] as int? ?? 0; // Get the register index or default to 0

        // Get the value of the coutner
        int counterValue = VM.GetRegister(counter);

        // Push a new Until control block.
        VM.Controls.Push(new UntilControlBlock(
            ControlTypes.LOOP, i, counter, val, counterValue != val && VM.Controls.Top().Active
        ));
    }



    public void HandleInstructionEnd(int index, Instruction instruction)
    {
        ControlBlock top = VM.Controls.Top();

        if (top.Type == ControlTypes.ROOT)
            Error.InterpreterError("invalid end of block", "tried to exit block when there is no block, if you meant to halt the program use `halt` instead.", instruction.Token);

        // Remove if force deactive
        if (top.ForceDeactive)
        {
            VM.Controls.Pop();
            return;
        }

        // Handle loops
        if (top is UntilControlBlock loopTop)
        {
            int counter = loopTop.Counter;
            int counterValue = VM.GetRegister(counter);

            int destValue = loopTop.Destination;
            int startIndex = loopTop.Start;

            // Check if we want to end this bs or jump back.
            if (counterValue != destValue)
            {
                InstructionIndex = startIndex;
                return; // return so it doesn't pop the control
            }

            // If we dont decide to jump back it continues on and pops the control.
        }

        VM.Controls.Pop();
    }




    public void HandleInstructionBreak(int index, Instruction instruction)
    {
        ControlBlock? controlBlock = VM.Controls.Controls.FindLast(c => c.Type == ControlTypes.LOOP);

        if (controlBlock is null)
        {
            Error.SyntaxError("invalid break", "expected loop to break out of, found none.", instruction.Token);
            return;
        }

        controlBlock.ForceDeactive = true;
        controlBlock.Active = false;
    }



    // == MEMORY STUFF

    public void HandleInstructionWrite(int index, Instruction instruction)
    {
        int addr = (int)instruction.Values[0];

        VM.SetStore(addr, VM.GetSelectionValue());
    }

    public void HandleInstructionRead(int index, Instruction instruction)
    {
        int addr = (int)instruction.Values[0];

        VM.SetSelectionValue(VM.GetStore(addr));
    }

    public void HandleInstructionElemRead(int index, Instruction instruction)
    {
        int addr = VM.GetRegister((int)instruction.Values[0]);

        VM.SetSelectionValue(VM.GetStore(addr));
    }

    public void HandleInstructionElemWrite(int index, Instruction instruction)
    {
        int addr = VM.GetRegister((int)instruction.Values[0]);

        VM.SetStore(addr, VM.GetSelectionValue());
    }

    // ===============

    public void HandleInstructionHalt(int _, Instruction __)
    {
        int code = VM.GetSelectionValue();

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[[ HALT: {code} ]].");
        Console.ResetColor();

        Environment.Exit(code);
    }

    // ======================================================================
    // === SYSCALL HANDLER BELOW ============================================
    // ======================================================================

    public void HandleInstructionSyscall(int index, Instruction instruction)
    {
        string callee = (string)instruction.Values[0];

        // Handle each syscall
        switch (callee)
        {
            case "print": // Print syscall
                SyscallPrintString();
                break;

            case "scan": // Read numeral syscall
                SyscallReadNumeral();
                break;

            case "rprint": // Dump register
                SyscallDumpRegister();
                break;

            case "dump": // Dump registers
                VM.DumpRegisters();
                break;
        }
    }

    private void SyscallDumpRegister()
    {
        Console.WriteLine(VM.GetSelectionValue());
    }

    private void SyscallPrintString()
    {
        Console.WriteLine(VM.StringBuffer);
    }

    private void SyscallReadNumeral()
    {
        Console.Write(VM.StringBuffer != "" ? VM.StringBuffer : ">> ");

        // Get read value
        string? valC = Console.ReadLine();
        bool ok = int.TryParse(valC, out int val);

        if (!ok)
        {
            Error.InterpreterError($"tried to read number, got invalid value '{valC}'", false);
            return;
        }

        VM.SetSelectionValue(val); // Set selection to said value
    }




    // == HELPERS =================


    public int FetchNumber(Instruction instruction, int typeIndex, int numIndex)
    {
        // Type of value: register or numeral
        string valueType = (string)instruction.Values[typeIndex];

        // The raw value, either an index or a numeral.
        int rawVal = (int)instruction.Values[numIndex];

        // If register then get value of specified register
        // Else pass raw value.
        int val = valueType == TokenType.REGISTER ? VM.GetRegister(rawVal) : rawVal;
        return val;
    }
}