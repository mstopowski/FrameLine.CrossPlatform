using Rhino;
using Rhino.Geometry;
using Rhino.Collections;

namespace FrameLine.Common
{
    class FrameLine
    {
        // Lists: spacing, frame numbers, if frame to be labeled
        public RhinoList<int> spacingList = new RhinoList<int>();
        public RhinoList<int> framesList = new RhinoList<int>();
        public RhinoList<bool> ifLabelList = new RhinoList<bool>();

        // Points for main line of frameline
        public RhinoList<Point3d> polyPoints = new RhinoList<Point3d>();

        public FrameLine(RhinoList<Spacing> spacings, double scaling)
        {
            // Move distance in x-axis for zero to be at zero
            int zeroMove = 0;

            int tempSum = 0;

            for (int i = 0; i < (spacings[0].End - spacings[0].Start) + 1; i++)
            {
                spacingList.Add(spacings[0].Space);
                framesList.Add(spacings[0].Start + i);
                if ((spacings[0].Start + i) % 5 == 0)
                {
                    ifLabelList.Add(true);
                }
                else
                {
                    ifLabelList.Add(false);
                }
            }

            if (spacings.Count > 1)
            {
                for (int i = 1; i < spacings.Count; i++)
                {
                    for (int j = 0; j < (spacings[i].End - spacings[i].Start); j++)
                    {
                        spacingList[spacings[i].Start - spacings[0].Start + j] = spacings[i].Space;
                    }
                    ifLabelList[spacings[i].Start - spacings[0].Start] = true;
                    ifLabelList[spacings[i].End - spacings[0].Start] = true;
                }
            }

            // First and last frame always with label
            ifLabelList[0] = true;
            ifLabelList[ifLabelList.Count - 1] = true;

            for (int i = 0; i < framesList.Count; i++)
            {
                if (framesList[i] < 0)
                {
                    zeroMove += spacingList[i];
                }
                else
                {
                    break;
                }
            }

            polyPoints.Add(new Point3d((double)(-zeroMove / scaling), 0, 0));

            for (int i = 0; i < framesList.Count - 1; i++)
            {
                polyPoints.Add(new Point3d((double)((spacingList[i] + tempSum - zeroMove) / scaling), 0.0, 0.0));
                tempSum += spacingList[i];
            }

            // Removing last point (end+1)
            //polyPoints.RemoveAt(polyPoints.Count - 1);
        }
    }
}
