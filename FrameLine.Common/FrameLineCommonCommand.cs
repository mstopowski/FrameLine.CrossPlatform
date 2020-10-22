using System;
using Rhino;
using Rhino.Commands;
using Rhino.Collections;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.DocObjects;
#if ON_RUNTIME_APPLE
using Eto.Drawing;
using Eto.Forms;
#endif

namespace FrameLine.Common
{
    public class FrameLineCommonCommand : Rhino.Commands.Command
    {
        public override string EnglishName => "FrameLine";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            Double dimScale = doc.DimStyles.Current.DimensionScale;
            double scalingValue = RhinoMath.UnitScale(doc.ModelUnitSystem, UnitSystem.Millimeters);

            RhinoList<Spacing> spacings = new RhinoList<Spacing>(); //List of spacings

            UserInteract user = new UserInteract(ref spacings);

            if (!user.AskUser())
            {
                return Result.Cancel;
            }

            // Create object of Class FrameLine with spacings as input
            FrameLine frameLine = new FrameLine(spacings, scalingValue);

            // Backup of current layer
            var layerBackUp = doc.Layers.CurrentLayer;

            //Creating layer for frameline
            Layer flineLayer = new Layer
            {
                Name = "FRAME LINE"
            };
            doc.Layers.Add(flineLayer);
            doc.Layers.SetCurrentLayerIndex(doc.Layers.FindName(flineLayer.Name).Index, true);

            // Creating layer for labels (child of frameline)
            Layer labelLayer = new Layer
            {
                Name = "LABELS",
                ParentLayerId = doc.Layers.FindIndex(doc.Layers.FindName(flineLayer.Name).Index).Id
            };
            doc.Layers.Add(labelLayer);

            // Grupowanie
            var groupName = "FRAME LINE GROUP";
            if (doc.Groups.Count == 0 || doc.Groups.FindName(groupName) == null)
            {
                doc.Groups.Add(groupName);
            }
            if (doc.Groups.FindName(groupName) != null)
            {
                var oldFrameLine = doc.Objects.FindByGroup(doc.Groups.FindName(groupName).Index);
                foreach (var element in oldFrameLine)
                {
                    doc.Objects.Delete(element);
                }
            }

            int frameHeight = 400; // Vertical lines height in mm

            CreateCrossLines(doc, frameLine, groupName, frameHeight, scalingValue);

            doc.Layers.SetCurrentLayerIndex(doc.Layers.FindName(labelLayer.Name).Index, true);

            AddLabels(doc, frameLine, groupName, frameHeight, scalingValue, dimScale);

            // Adding polyline to document
            doc.Layers.SetCurrentLayerIndex(doc.Layers.FindName(flineLayer.Name).Index, true);
            var polyID = doc.Objects.AddPolyline(frameLine.polyPoints);
            doc.Groups.AddToGroup(doc.Groups.FindName(groupName).Index, polyID);

            // Redrawing views
            doc.Views.Redraw();

            // Restoring previous layer and locking frameline layer
            doc.Layers.SetCurrentLayerIndex(layerBackUp.Index, true);
            doc.Layers.FindIndex(doc.Layers.FindName(labelLayer.Name).Index).IsLocked = true;
            doc.Layers.FindIndex(doc.Layers.FindName(flineLayer.Name).Index).IsLocked = true;

            return Result.Success;
        }

        void CreateCrossLines(RhinoDoc doc, FrameLine frameLine, string groupName, int frameHeight, double scaling)
        {
            // Drawing frameline lines
            for (int i = 0; i < frameLine.polyPoints.Count; i++)
            {
                var line1ID = doc.Objects.AddLine(new Line(new Point3d(frameLine.polyPoints[i][0],frameLine.polyPoints[i][1] - frameHeight / scaling / 2, 0),
                                                new Point3d(frameLine.polyPoints[i][0],frameLine.polyPoints[i][1] + frameHeight / scaling / 2, 0)));
                var line2ID = doc.Objects.AddLine(new Line(new Point3d(frameLine.polyPoints[i][0], 0, frameLine.polyPoints[i][1] - frameHeight / scaling / 2),
                                                new Point3d(frameLine.polyPoints[i][0], 0, frameLine.polyPoints[i][1] + frameHeight / scaling / 2)));
                doc.Groups.AddToGroup(doc.Groups.FindName(groupName).Index, line1ID);
                doc.Groups.AddToGroup(doc.Groups.FindName(groupName).Index, line2ID);
            }
        }

        void AddLabels(RhinoDoc doc, FrameLine frameLine, string groupName,int frameHeight, double scaling, double dimScale)
        {
            int textHeight = 150; // Desired text height in mm

            // Adding labels
            for (int i = 0; i < frameLine.polyPoints.Count; i++)
            {
                if (frameLine.ifLabelList[i])
                {
                    Text3d tkst = new Text3d("Fr " + frameLine.framesList[i].ToString());
                    Text3d tkstRotated = new Text3d("Fr " + frameLine.framesList[i].ToString());

                    tkst.Height = (double)(textHeight / dimScale / scaling);
                    tkstRotated.Height = (double)(textHeight / dimScale / scaling);

                    tkst.HorizontalAlignment = TextHorizontalAlignment.Center;
                    tkst.VerticalAlignment = TextVerticalAlignment.Middle;

                    tkstRotated.HorizontalAlignment = TextHorizontalAlignment.Center;
                    tkstRotated.VerticalAlignment = TextVerticalAlignment.Middle;

                    tkst.TextPlane = new Plane(new Point3d(frameLine.polyPoints[i][0], -frameHeight / scaling, 0), new Vector3d(0.0, 0.0, 1.0));
                    Plane rotPlane = new Plane(new Point3d(frameLine.polyPoints[i][0], 0, -frameHeight / scaling), new Vector3d(0.0, 0.0, 1.0));
                    rotPlane.Rotate(Math.PI / 2, new Vector3d(1.0, 0.0, 0.0));
                    tkstRotated.TextPlane = rotPlane;

                    var tkstID = doc.Objects.AddText(tkst);
                    var tkstRotatedID = doc.Objects.AddText(tkstRotated);
                    doc.Groups.AddToGroup(doc.Groups.FindName(groupName).Index, tkstID);
                    doc.Groups.AddToGroup(doc.Groups.FindName(groupName).Index, tkstRotatedID);
                }
            }
        }
    }
}