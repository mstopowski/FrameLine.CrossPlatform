using Rhino;
using Rhino.Commands;

namespace FrameLine.Common
{
    public class FrameLineCommonCommand : Command
    {
        public override string EnglishName => "FrameLine";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            FrameLineMethods frameLineMethods = new FrameLineMethods(doc);

            return frameLineMethods.CreateFL();
        }
    }
}