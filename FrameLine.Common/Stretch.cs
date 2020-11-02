namespace FrameLine.Common
{
    public class Stretch
    {
        public int Start { get; }
        public int Interval { get; }

        public Stretch(int start, int interval)
        {
            Start = start;
            Interval = interval;
        }
    }
}
