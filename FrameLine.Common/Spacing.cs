namespace FrameLine.Common
{
    public class Spacing
    {
        public int Start { get; }
        public int End { get; }
        public int Space { get; }
        
        public Spacing(int start, int end, int space)
        {
            Start = start;
            End = end;
            Space = space;
        }
    }
}
