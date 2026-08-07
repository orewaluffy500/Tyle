namespace aski.controls;


public class ControlStack
{
    public List<ControlBlock> Controls { get; } = [];

    public void Push(int type, int start, bool active) => Controls.Add(new(type, start, active));
    public void Push(ControlBlock controlBlock) => Controls.Add(controlBlock);
    public void Pop() => Controls.RemoveAt(Controls.Count - 1);

    public ControlBlock Top() => Controls.Count > 0 ? Controls[^1] : new(ControlTypes.ROOT, 0, true);

    public bool IsTopActive() => Top().Active;
}