using aski.controls;

namespace aski.memory;


public class VirtualMachine(int regCount, int valCount)
{
    public int[] Registers { get; } = new int[regCount]; // Fast usage registers
    public int[] Storage { get; } = new int[valCount]; // Large memory buffer
    public int Selection { get; set; } = -1; // Used for return values from syscalls
    public string StringBuffer { get; set; } = ""; // Global string buffer.
    public ControlStack Controls { get; } = new(); // Global control buffer

    public int ClampRegisterIndex(int reg) => Math.Clamp(reg - 1, 0, Registers.Length - 1);
    public int ClampStoreIndex(int address) => Math.Clamp(address - 1, 0, Storage.Length - 1);

    public void SetRegister(int reg, int val) => Registers[ClampRegisterIndex(reg)] = val;
    public void SetStore(int addr, int val) => Storage[ClampStoreIndex(addr)] = val;
    public void SetSelectionValue(int val) { if (Selection != -1) Registers[ClampRegisterIndex(Selection)] = val; }

    public int GetRegister(int reg) => Registers[ClampRegisterIndex(reg)];
    public int GetStore(int addr) => Storage[ClampStoreIndex(addr)];
    public int GetSelectionValue() => Selection != -1 ? Registers[ClampRegisterIndex(Selection)] : 0;


    public void DumpRegisters()
    {
        foreach (int register in Registers)
        {
            Console.Write($"({register})");
        }

        Console.WriteLine();
    }
}