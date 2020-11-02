using System;
using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace FrameLine.Common
{
    public class FrameLineMethods
    {
        RhinoDoc Doc { get; }

        public FrameLineMethods(RhinoDoc rhinoDoc)
        {
            Doc = rhinoDoc;
        }

        public Result CreateFL()
        {
            Double dimScale = Doc.DimStyles.Current.DimensionScale;
            double scalingValue = RhinoMath.UnitScale(Doc.ModelUnitSystem, UnitSystem.Millimeters);

            RhinoList<Spacing> spacings = new RhinoList<Spacing>(); //List of spacings
            RhinoList<Stretch> stretches = new RhinoList<Stretch>();

            UserInteract user = new UserInteract(ref spacings, ref stretches);

            if (!user.AskUser())
            {
                return Result.Cancel;
            }

            // Create object of Class FrameLine with spacings as input
            FrameLine frameLine = new FrameLine(spacings, stretches, scalingValue);

            // Backup of current layer
            var layerBackUp = Doc.Layers.CurrentLayer;

            //Creating layer for frameline
            Layer flineLayer = new Layer
            {
                Name = "FRAME LINE"
            };
            Doc.Layers.Add(flineLayer);
            Doc.Layers.SetCurrentLayerIndex(Doc.Layers.FindName(flineLayer.Name).Index, true);

            // Creating layer for labels (child of frameline)
            Layer labelLayer = new Layer
            {
                Name = "LABELS",
                ParentLayerId = Doc.Layers.FindIndex(Doc.Layers.FindName(flineLayer.Name).Index).Id
            };
            Doc.Layers.Add(labelLayer);

            // Grupowanie
            var groupName = "FRAME LINE GROUP";
            if (Doc.Groups.Count == 0 || Doc.Groups.FindName(groupName) == null)
            {
                Doc.Groups.Add(groupName);
            }
            if (Doc.Groups.FindName(groupName) != null)
            {
                var oldFrameLine = Doc.Objects.FindByGroup(Doc.Groups.FindName(groupName).Index);
                foreach (var element in oldFrameLine)
                {
                    Doc.Objects.Delete(element);
                }
            }

            int frameHeight = 400; // Vertical lines height in mm

            CreateCrossLines(Doc, frameLine, groupName, frameHeight, scalingValue);

            Doc.Layers.SetCurrentLayerIndex(Doc.Layers.FindName(labelLayer.Name).Index, true);

            AddLabels(Doc, frameLine, groupName, frameHeight, scalingValue, dimScale);

            // Adding polyline to document
            Doc.Layers.SetCurrentLayerIndex(Doc.Layers.FindName(flineLayer.Name).Index, true);
            var polyID = Doc.Objects.AddPolyline(frameLine.polyPoints);
            Doc.Groups.AddToGroup(Doc.Groups.FindName(groupName).Index, polyID);

            // Redrawing views
            Doc.Views.Redraw();

            // Restoring previous layer and locking frameline layer
            Doc.Layers.SetCurrentLayerIndex(layerBackUp.Index, true);
            Doc.Layers.FindIndex(Doc.Layers.FindName(labelLayer.Name).Index).IsLocked = true;
            Doc.Layers.FindIndex(Doc.Layers.FindName(flineLayer.Name).Index).IsLocked = true;

            return Result.Success;
        }

        void CreateCrossLines(RhinoDoc doc, FrameLine frameLine, string groupName, int frameHeight, double scaling)
        {
            // Drawing frameline lines
            for (int i = 0; i < frameLine.polyPoints.Count; i++)
            {
                var line1ID = doc.Objects.AddLine(new Line(new Point3d(frameLine.polyPoints[i][0], frameLine.polyPoints[i][1] - frameHeight / scaling / 2, 0),
                                                new Point3d(frameLine.polyPoints[i][0], frameLine.polyPoints[i][1] + frameHeight / scaling / 2, 0)));
                var line2ID = doc.Objects.AddLine(new Line(new Point3d(frameLine.polyPoints[i][0], 0, frameLine.polyPoints[i][1] - frameHeight / scaling / 2),
                                                new Point3d(frameLine.polyPoints[i][0], 0, frameLine.polyPoints[i][1] + frameHeight / scaling / 2)));
                doc.Groups.AddToGroup(doc.Groups.FindName(groupName).Index, line1ID);
                doc.Groups.AddToGroup(doc.Groups.FindName(groupName).Index, line2ID);
            }
        }

        void AddLabels(RhinoDoc doc, FrameLine frameLine, string groupName, int frameHeight, double scaling, double dimScale)
        {
            int textHeight = 150; // Desired text height in mm

            // Adding labels
            for (int i = 0; i < frameLine.polyPoints.Count; i++)
            {
                if (frameLine.ifLabelList[i])
                {
                    Text3d tkst = new Text3d(frameLine.stringList[i]);
                    Text3d tkstRotated = new Text3d(frameLine.stringList[i]);

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
