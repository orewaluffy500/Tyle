namespace aski.controls;


public class ControlBlock(int type, int start, bool active)
{
    public int Type { get; set; } = type;
    public int Start { get; set; } = start;
    public bool Active { get; set; } = active;
    public bool ForceDeactive { get; set; } = false;
}


public class UntilControlBlock(int type, int start, int counter, int dest, bool active): ControlBlock(type, start, active)
{
    public int Counter { get; set; } = counter;
    public int Destination { get; set; } = dest;
}