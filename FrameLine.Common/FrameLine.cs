using Rhino.Geometry;
using Rhino.Collections;

namespace FrameLine.Common
{
    class FrameLine
    {
        // Lists: spacing, frame numbers, if frame to be labeled
        public RhinoList<int> spacingList = new RhinoList<int>(); //[100,100,100,100,100]
        public RhinoList<int> framesList = new RhinoList<int>(); //[-2, -1, 0, 1, 2]
        public RhinoList<string> stringList = new RhinoList<string>(); //[fr -2, fr-2, fr 0, fr 1, fr 2]
        public RhinoList<bool> ifLabelList = new RhinoList<bool>(); // [true, false, true, false, true]

        // Points for main line of frameline
        public RhinoList<Point3d> polyPoints = new RhinoList<Point3d>();

        public FrameLine(RhinoList<Spacing> spacings, RhinoList<Stretch> stretches, double scaling)
        {
            // Move distance in x-axis for zero to be at zero
            int zeroMove = 0;
            int tempSum = 0;
            
            for (int i = 0; i < (spacings[0].End - spacings[0].Start) + 1; i++)
            {
                spacingList.Add(spacings[0].Space);
                framesList.Add(spacings[0].Start + i);
                stringList.Add($"{spacings[0].Start + i}");
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

            if (stretches.Count == 1)
            {
                var spacingInsert = spacingList[stretches[0].Start - spacings[0].Start];
                for (int i = 0; i < stretches[0].Interval; i++)
                {
                    spacingList.Insert(stretches[0].Start - spacings[0].Start + 1, spacingInsert);
                    stringList.Insert(stretches[0].Start - spacings[0].Start + 1 + i, $"E {i+1}");
                    ifLabelList.Insert(stretches[0].Start - spacings[0].Start + 1, true);
                    if (i == stretches[0].Interval - 1)
                    {
                        ifLabelList[stretches[0].Start - spacings[0].Start + stretches[0].Interval + 1] = true;
                    }
                }

                framesList.Clear();
                if (stretches[0].Start<0)
                {
                    for (int i = 0; i < (spacings[0].End - spacings[0].Start + stretches[0].Interval) + 1; i++)
                    {
                        framesList.Add(spacings[0].Start - stretches[0].Interval + i);
                    }
                }
                else
                {
                    for (int i = 0; i < (spacings[0].End - spacings[0].Start + stretches[0].Interval) + 1; i++)
                    {
                        framesList.Add(spacings[0].Start + i);
                    }
                }
            }

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
        }
    }
}
